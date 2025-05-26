param (
    [ValidateSet('Debug', 'Release')]
    [String]$Configuration = 'Release',

    [ValidateSet('win-x64', 'win-x86', 'win-arm64')]
    [String]$RuntimeIdentifier = 'win-x64',

    [String]$OutputPath = "$PSScriptRoot/module"
)

$originalProgressPreference = $ProgressPreference
$ProgressPreference = 'SilentlyContinue'

Write-Host 'Building Clean WinUIShell Module' -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Runtime Identifier: $RuntimeIdentifier" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow

$netVersion = 'net8.0'
$serverTarget = 'net8.0-windows10.0.18362.0'
$copyExtensions = @('.dll', '.pdb')
$src = "$PSScriptRoot/src"
$coreSrc = "$src/WinUIShell"
$depSrc = "$src/WinUIShell.Common"
$serverSrc = "$src/WinUIShell.Server"

# Build with framework-dependent for smaller size
$corePublish = [System.IO.Path]::GetFullPath("$coreSrc/bin/$Configuration/$netVersion/publish/")
$depPublish = [System.IO.Path]::GetFullPath("$depSrc/bin/$Configuration/$netVersion/publish/")
$serverPublish = [System.IO.Path]::GetFullPath("$serverSrc/bin/$Configuration/$serverTarget/publish/")

$outDir = "$OutputPath/WinUIShell/bin/$netVersion"
$outDeps = "$outDir/Dependencies"
$outServer = "$OutputPath/WinUIShell/bin/$serverTarget"

function CopyFolderItems($FolderPath, $Destination) {
    if (Test-Path $Destination) {
        Copy-Item -Path "$FolderPath/*" -Destination $Destination -Recurse
    } else {
        Copy-Item -Path $FolderPath -Destination $Destination -Recurse
    }
}

# Clean output directories
Write-Host 'Cleaning output directories...' -ForegroundColor Cyan
Remove-Item -Path $OutputPath -Recurse -ErrorAction Ignore
New-Item -Path $OutputPath -ItemType Directory -Force | Out-Null

# Build WinUIShell.Common (framework-dependent for smaller size)
Write-Host 'Building WinUIShell.Common...' -ForegroundColor Cyan
Push-Location $depSrc
dotnet publish -c $Configuration -o $depPublish -p:PublishReadyToRun=false
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Failed to build WinUIShell.Common'
    exit 1
}
Pop-Location

# Build WinUIShell (framework-dependent for smaller size)
Write-Host 'Building WinUIShell...' -ForegroundColor Cyan
Push-Location $coreSrc
dotnet publish -c $Configuration -o $corePublish -p:PublishReadyToRun=false
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Failed to build WinUIShell'
    exit 1
}
Pop-Location

# Build WinUIShell.Server (self-contained single file for portability)
Write-Host 'Building WinUIShell.Server...' -ForegroundColor Cyan
Push-Location $serverSrc
dotnet publish -c $Configuration -r $RuntimeIdentifier --self-contained true -p:PublishSingleFile=true -o $serverPublish
if ($LASTEXITCODE -ne 0) {
    Write-Error 'Failed to build WinUIShell.Server'
    exit 1
}
Pop-Location

# Filter deps files to only include .dll and .pdb
Write-Host 'Filtering dependency files...' -ForegroundColor Cyan
Get-ChildItem -Path $depPublish -Recurse -File | Where-Object {
    $_.Extension -notin $copyExtensions
} | Remove-Item -Force

$deps = [System.Collections.Generic.List[string]]::new()
Get-ChildItem -Path $depPublish -Recurse -File | ForEach-Object {
    $deps.Add($_.FullName.Replace($depPublish, ''))
}

# Filter core dlls to avoid duplicates
Write-Host 'Filtering core files...' -ForegroundColor Cyan
Get-ChildItem -Path $corePublish -Recurse -File | Where-Object {
    $path = $_.FullName.Replace($corePublish, '')
    ($_.Extension -notin $copyExtensions) -or ($deps.Contains($path))
} | Remove-Item -Force

# Remove empty folders
Get-ChildItem -Path $corePublish -Recurse -Directory | Where-Object {
    -not (Get-ChildItem -Path $_.FullName -Recurse -File)
} | Remove-Item -Force

# Copy filtered outputs
Write-Host 'Copying filtered outputs...' -ForegroundColor Cyan
CopyFolderItems -FolderPath $corePublish -Destination $outDir
CopyFolderItems -FolderPath $depPublish -Destination $outDeps

# Copy only the server executable (single file)
Write-Host 'Copying server executable...' -ForegroundColor Cyan
New-Item -Path $outServer -ItemType Directory -Force | Out-Null
Copy-Item -Path "$serverPublish/WinUIShell.Server.exe" -Destination $outServer -Force

# Copy PowerShell module files
Write-Host 'Copying PowerShell module files...' -ForegroundColor Cyan
$moduleSource = "$PSScriptRoot/module/WinUIShell"
Copy-Item -Path "$moduleSource/WinUIShell.psd1" -Destination "$OutputPath/WinUIShell/" -Force
Copy-Item -Path "$moduleSource/WinUIShell.psm1" -Destination "$OutputPath/WinUIShell/" -Force

Write-Host 'Clean module build completed successfully!' -ForegroundColor Green
Write-Host "Module location: $OutputPath/WinUIShell" -ForegroundColor Yellow

# Display size information
$moduleSize = (Get-ChildItem -Path "$OutputPath/WinUIShell" -Recurse -File | Measure-Object -Property Length -Sum).Sum
$moduleSizeMB = [math]::Round($moduleSize / 1MB, 2)
Write-Host "Total module size: $moduleSizeMB MB" -ForegroundColor Yellow

$serverExe = Get-Item "$outServer/WinUIShell.Server.exe" -ErrorAction SilentlyContinue
if ($serverExe) {
    $sizeMB = [math]::Round($serverExe.Length / 1MB, 2)
    Write-Host "WinUIShell.Server.exe size: $sizeMB MB" -ForegroundColor Yellow
}

$ProgressPreference = $originalProgressPreference
