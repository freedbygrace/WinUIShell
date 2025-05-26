param (
    [ValidateSet('Debug', 'Release')]
    [String]$Configuration = 'Release',

    [ValidateSet('win-x64', 'win-x86', 'win-arm64')]
    [String]$RuntimeIdentifier = 'win-x64',

    [Switch]$SelfContained = $true,

    [Switch]$SingleFile = $true,

    [String]$OutputPath = "$PSScriptRoot/publish"
)

$originalProgressPreference = $ProgressPreference
$ProgressPreference = 'SilentlyContinue'

Write-Host 'Building WinUIShell with Self-Contained Deployment' -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Runtime Identifier: $RuntimeIdentifier" -ForegroundColor Yellow
Write-Host "Self-Contained: $SelfContained" -ForegroundColor Yellow
Write-Host "Single File: $SingleFile" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow

$netVersion = 'net8.0'
$serverTarget = 'net8.0-windows10.0.18362.0'
$src = "$PSScriptRoot/src"
$coreSrc = "$src/WinUIShell"
$depSrc = "$src/WinUIShell.Common"
$serverSrc = "$src/WinUIShell.Server"

# Output directories for self-contained builds
$outDir = "$OutputPath/WinUIShell/$RuntimeIdentifier"
$outDeps = "$outDir/Dependencies"
$outServer = "$OutputPath/WinUIShell.Server/$RuntimeIdentifier"

function CopyFolderItems($FolderPath, $Destination) {
    if (Test-Path $Destination) {
        Copy-Item -Path "$FolderPath/*" -Destination $Destination -Recurse -Force
    } else {
        Copy-Item -Path $FolderPath -Destination $Destination -Recurse -Force
    }
}

# Clean output directories
Write-Host 'Cleaning output directories...' -ForegroundColor Cyan
Remove-Item -Path $outDir -Recurse -ErrorAction Ignore
Remove-Item -Path $outServer -Recurse -ErrorAction Ignore
New-Item -Path $outDir -ItemType Directory -Force | Out-Null
New-Item -Path $outServer -ItemType Directory -Force | Out-Null

# Build parameters for libraries (no single file)
$libraryPublishArgs = @(
    'publish'
    '-c', $Configuration
    '-r', $RuntimeIdentifier
)

if ($SelfContained) {
    $libraryPublishArgs += '--self-contained', 'true'
} else {
    $libraryPublishArgs += '--self-contained', 'false'
}

# Build parameters for executables (with single file option)
$executablePublishArgs = $libraryPublishArgs.Clone()
if ($SingleFile -and $SelfContained) {
    $executablePublishArgs += '-p:PublishSingleFile=true'
}

# Build WinUIShell.Common (library - no single file)
Write-Host 'Building WinUIShell.Common...' -ForegroundColor Cyan
$depPublish = "$depSrc/bin/$Configuration/$netVersion/$RuntimeIdentifier/publish/"
Push-Location $depSrc
$depArgs = $libraryPublishArgs + @('-o', $depPublish)
& dotnet @depArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Failed to build WinUIShell.Common'
    exit 1
}
Pop-Location

# Build WinUIShell (library - no single file)
Write-Host 'Building WinUIShell...' -ForegroundColor Cyan
$corePublish = "$coreSrc/bin/$Configuration/$netVersion/$RuntimeIdentifier/publish/"
Push-Location $coreSrc
$coreArgs = $libraryPublishArgs + @('-o', $corePublish)
& dotnet @coreArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Failed to build WinUIShell'
    exit 1
}
Pop-Location

# Build WinUIShell.Server (executable - can use single file)
Write-Host 'Building WinUIShell.Server...' -ForegroundColor Cyan
$serverPublish = "$serverSrc/bin/$Configuration/$serverTarget/$RuntimeIdentifier/publish/"
Push-Location $serverSrc
$serverArgs = $executablePublishArgs + @('-o', $serverPublish)
& dotnet @serverArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Failed to build WinUIShell.Server'
    exit 1
}
Pop-Location

# Copy outputs
Write-Host 'Copying build outputs...' -ForegroundColor Cyan

if ($SelfContained -and $SingleFile) {
    # For single file deployments, copy the main executables and essential files
    Copy-Item -Path "$corePublish/*.dll" -Destination $outDir -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$corePublish/*.pdb" -Destination $outDir -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$depPublish/*.dll" -Destination $outDeps -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$depPublish/*.pdb" -Destination $outDeps -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "$serverPublish/WinUIShell.Server.exe" -Destination $outServer -Force
    Copy-Item -Path "$serverPublish/*.dll" -Destination $outServer -Force -ErrorAction SilentlyContinue
} else {
    # For regular self-contained deployments, copy everything
    CopyFolderItems -FolderPath $corePublish -Destination $outDir
    CopyFolderItems -FolderPath $depPublish -Destination $outDeps
    CopyFolderItems -FolderPath $serverPublish -Destination $outServer
}

# Copy PowerShell module files
Write-Host 'Copying PowerShell module files...' -ForegroundColor Cyan
$moduleSource = "$PSScriptRoot/module/WinUIShell"
$moduleOutput = "$OutputPath/module/WinUIShell"
New-Item -Path $moduleOutput -ItemType Directory -Force | Out-Null
Copy-Item -Path "$moduleSource/*.psd1" -Destination $moduleOutput -Force
Copy-Item -Path "$moduleSource/*.psm1" -Destination $moduleOutput -Force

# Create bin directories in module output
$moduleBinCore = "$moduleOutput/bin/$netVersion"
$moduleBinServer = "$moduleOutput/bin/$serverTarget"
New-Item -Path $moduleBinCore -ItemType Directory -Force | Out-Null
New-Item -Path $moduleBinServer -ItemType Directory -Force | Out-Null

# Copy built binaries to module structure
Copy-Item -Path "$outDir/*" -Destination $moduleBinCore -Recurse -Force
Copy-Item -Path "$outServer/*" -Destination $moduleBinServer -Recurse -Force

Write-Host 'Self-contained build completed successfully!' -ForegroundColor Green
Write-Host "Output location: $OutputPath" -ForegroundColor Yellow
Write-Host "Module location: $moduleOutput" -ForegroundColor Yellow

# Display size information
$serverExe = Get-Item "$outServer/WinUIShell.Server.exe" -ErrorAction SilentlyContinue
if ($serverExe) {
    $sizeMB = [math]::Round($serverExe.Length / 1MB, 2)
    Write-Host "WinUIShell.Server.exe size: $sizeMB MB" -ForegroundColor Yellow
}

$ProgressPreference = $originalProgressPreference
