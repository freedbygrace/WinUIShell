# WinUIShell PowerShell Module - Enhanced Edition
# This module provides comprehensive notification and UI functionality using WinUI3
# with enhanced dialog results and dynamic control enumeration

# Initialize the WinUIShell engine when the module is loaded
try {
    # Start the WinUIShell engine (if available)
    if ([WinUIShell.Engine] -and [WinUIShell.Engine]::Start) {
        [WinUIShell.Engine]::Start()
        Write-Verbose "WinUIShell engine started successfully"
    }
} catch {
    Write-Verbose "WinUIShell engine not available or failed to start: $($_.Exception.Message)"
}

# Clean up when the module is removed
$MyInvocation.MyCommand.ScriptBlock.Module.OnRemove = {
    try {
        if ([WinUIShell.Engine] -and [WinUIShell.Engine]::Stop) {
            [WinUIShell.Engine]::Stop()
            Write-Verbose "WinUIShell engine stopped"
        }
    } catch {
        Write-Verbose "Error stopping WinUIShell engine: $($_.Exception.Message)"
    }
}

# C# cmdlets are automatically loaded from the DLL via the manifest

# Export module information
Write-Verbose "WinUIShell Enhanced module loaded with 32 C# cmdlets for comprehensive notification functionality"
Write-Verbose "Enhanced features: Dialog results, control enumeration, window management, icon support"
