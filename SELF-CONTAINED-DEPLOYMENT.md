# Self-Contained Deployment Guide for WinUIShell

This guide explains how to build and deploy WinUIShell as fully self-contained binaries that don't require any external runtime dependencies.

## Overview

Self-contained deployment packages the .NET runtime and all dependencies with your application, creating a standalone executable that can run on machines without .NET installed.

## Benefits

- **No Runtime Dependencies**: No need for .NET Desktop Runtime 8.0 or Windows App Runtime to be pre-installed
- **Simplified Deployment**: Single executable or folder contains everything needed
- **Version Isolation**: Guaranteed to use the exact runtime version you built with
- **Offline Installation**: Can be deployed to machines without internet access

## Build Instructions

### Quick Build (Recommended)

Use the provided self-contained build script:

```powershell
# Build for x64 (default)
.\Build-SelfContained.ps1

# Build for specific architecture
.\Build-SelfContained.ps1 -RuntimeIdentifier win-x64
.\Build-SelfContained.ps1 -RuntimeIdentifier win-x86
.\Build-SelfContained.ps1 -RuntimeIdentifier win-arm64

# Build Debug version
.\Build-SelfContained.ps1 -Configuration Debug

# Build without single file (separate files)
.\Build-SelfContained.ps1 -SingleFile:$false
```

### Manual Build Commands

If you prefer to build manually:

```powershell
# Build WinUIShell.Server (self-contained single file)
dotnet publish src/WinUIShell.Server -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Build WinUIShell module (self-contained)
dotnet publish src/WinUIShell -c Release -r win-x64 --self-contained true

# Build WinUIShell.Common (self-contained)
dotnet publish src/WinUIShell.Common -c Release -r win-x64 --self-contained true
```

## Output Structure

After building, you'll find the self-contained binaries in the `publish` directory:

```
publish/
├── WinUIShell.Server/
│   └── win-x64/
│       └── WinUIShell.Server.exe    # Single self-contained executable (~150MB)
├── WinUIShell/
│   └── win-x64/
│       ├── WinUIShell.dll
│       ├── Dependencies/
│       └── [runtime files]
└── module/
    └── WinUIShell/
        ├── WinUIShell.psd1
        ├── WinUIShell.psm1
        └── bin/
            ├── net8.0/              # PowerShell module binaries
            └── net8.0-windows10.0.18362.0/  # WinUI server binaries
```

## Deployment

### Option 1: PowerShell Module Deployment

1. Copy the entire `publish/module/WinUIShell` folder to a PowerShell module path:
   - User modules: `$env:USERPROFILE\Documents\PowerShell\Modules\WinUIShell`
   - System modules: `$env:ProgramFiles\PowerShell\Modules\WinUIShell`

2. Import and use the module:
   ```powershell
   Import-Module WinUIShell
   # Module is now ready to use with no external dependencies
   ```

### Option 2: Standalone Application

1. Copy `WinUIShell.Server.exe` to any location
2. Run directly: `.\WinUIShell.Server.exe`

## Size Considerations

Self-contained deployments are larger than framework-dependent deployments:

- **WinUIShell.Server.exe**: ~150MB (includes .NET runtime + WinUI dependencies)
- **WinUIShell module**: ~50MB (includes .NET runtime + PowerShell dependencies)

### Size Optimization Options

1. **Enable Trimming** (Advanced):
   ```xml
   <PublishTrimmed>true</PublishTrimmed>
   <TrimMode>link</TrimMode>
   ```
   ⚠️ **Warning**: Trimming can break reflection-based code. Test thoroughly.

2. **Compression**:
   ```xml
   <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
   ```

3. **ReadyToRun** (Already enabled):
   - Improves startup time
   - Slightly increases size

## Runtime Identifiers

Supported runtime identifiers:
- `win-x64`: Windows 64-bit (Intel/AMD)
- `win-x86`: Windows 32-bit
- `win-arm64`: Windows ARM64

## Troubleshooting

### Common Issues

1. **Large File Size**: This is expected for self-contained deployments
2. **Slow First Startup**: ReadyToRun compilation happens on first run
3. **Antivirus Warnings**: Some antivirus software may flag self-contained executables

### Verification

Test your self-contained deployment:

```powershell
# Test the module
Import-Module .\publish\module\WinUIShell
$win = [WinUIShell.Window]::new()
$win.Title = "Self-Contained Test"
$win.Activate()
```

## Comparison: Framework-Dependent vs Self-Contained

| Aspect | Framework-Dependent | Self-Contained |
|--------|-------------------|----------------|
| Size | Small (~5MB) | Large (~150MB) |
| Dependencies | Requires .NET Runtime | None |
| Deployment | Complex | Simple |
| Updates | Shared runtime updates | App-specific updates |
| Compatibility | Runtime version dependent | Guaranteed compatibility |

## Advanced Configuration

### Custom Runtime Configuration

Create a `runtimeconfig.template.json` file for advanced runtime settings:

```json
{
  "configProperties": {
    "System.GC.Server": true,
    "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false
  }
}
```

### Build Optimization

For production builds, consider:

```xml
<PropertyGroup>
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <Optimize>true</Optimize>
</PropertyGroup>
```

## Security Considerations

- Self-contained apps include the full .NET runtime
- Keep the runtime updated by rebuilding with newer .NET versions
- Consider code signing for production deployments
