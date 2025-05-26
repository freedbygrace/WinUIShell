# Test script for WinUIShell C# cmdlets

Write-Host "Testing WinUIShell C# Cmdlets" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan

# Import the module
Write-Host "`n1. Importing WinUIShell module..." -ForegroundColor Yellow
try {
    Import-Module .\module\WinUIShell\WinUIShell.psd1 -Force
    Write-Host "   ✓ Module imported successfully" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed to import module: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 1: Get system theme
Write-Host "`n2. Testing Get-WinUISystemTheme..." -ForegroundColor Yellow
try {
    $theme = Get-WinUISystemTheme
    Write-Host "   ✓ System theme: $theme" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Create notification theme
Write-Host "`n3. Testing New-WinUINotificationTheme..." -ForegroundColor Yellow
try {
    $notificationTheme = New-WinUINotificationTheme -ThemeMode Auto
    Write-Host "   ✓ Notification theme created: $($notificationTheme.Mode) mode" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Create notification config
Write-Host "`n4. Testing New-WinUINotificationConfig..." -ForegroundColor Yellow
try {
    $config = New-WinUINotificationConfig -Title "Test" -Message "Hello World" -Type Info
    Write-Host "   ✓ Notification config created: $($config.Title)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Test notifications (without actually showing them)
Write-Host "`n5. Testing Test-WinUINotifications..." -ForegroundColor Yellow
try {
    Test-WinUINotifications -Verbose
    Write-Host "   ✓ Notification test completed" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: List available cmdlets
Write-Host "`n6. Available WinUIShell cmdlets:" -ForegroundColor Yellow
try {
    $cmdlets = Get-Command -Module WinUIShell | Sort-Object Name
    if ($cmdlets.Count -gt 0) {
        foreach ($cmdlet in $cmdlets) {
            Write-Host "   • $($cmdlet.Name)" -ForegroundColor Cyan
        }
        Write-Host "   ✓ Found $($cmdlets.Count) cmdlets" -ForegroundColor Green
    } else {
        Write-Host "   ⚠ No cmdlets found - they may not be properly exported" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n✅ WinUIShell C# cmdlets testing completed!" -ForegroundColor Green
Write-Host "`nNote: Visual notifications were not displayed in this test to avoid UI interference." -ForegroundColor Gray
