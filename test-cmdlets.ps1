# Test script for WinUIShell C# cmdlets

# Copy the updated DLL
Write-Host "Copying updated DLL..." -ForegroundColor Cyan
Copy-Item src\WinUIShell\bin\Release\net8.0\WinUIShell.dll module-clean\WinUIShell\bin\net8.0\WinUIShell.dll -Force

# Import the module
Write-Host "Importing WinUIShell module..." -ForegroundColor Cyan
Import-Module .\module-clean\WinUIShell\WinUIShell.psd1 -Force

# Test the cmdlets
Write-Host "Testing Get-WinUISystemTheme..." -ForegroundColor Green
$theme = Get-WinUISystemTheme
Write-Host "System theme: $theme" -ForegroundColor Yellow

Write-Host "Testing New-WinUINotificationTheme..." -ForegroundColor Green
$notificationTheme = New-WinUINotificationTheme
Write-Host "Notification theme mode: $($notificationTheme.Mode)" -ForegroundColor Yellow

Write-Host "Testing New-WinUIQuickNotification..." -ForegroundColor Green
New-WinUIQuickNotification -Type Success -Message "C# cmdlets are working!"

Write-Host "All tests completed!" -ForegroundColor Green
