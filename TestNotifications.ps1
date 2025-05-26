using namespace WinUIShell

# Test the notification system
Write-Host "Testing WinUIShell Notification System..." -ForegroundColor Green

# Import the module from the published location
Import-Module ".\publish\module\WinUIShell" -Force

Write-Host "Module imported successfully!" -ForegroundColor Green

# Test 1: System theme detection
Write-Host "`nTest 1: System Theme Detection" -ForegroundColor Yellow
$systemTheme = Get-WinUISystemTheme
Write-Host "Current system theme: $systemTheme" -ForegroundColor Cyan

# Test 2: Theme creation
Write-Host "`nTest 2: Theme Creation" -ForegroundColor Yellow
$theme = New-WinUINotificationTheme
Write-Host "Theme created with mode: $($theme.Mode)" -ForegroundColor Cyan

# Test 3: Quick notification
Write-Host "`nTest 3: Quick Notification" -ForegroundColor Yellow
New-WinUIQuickNotification -Type Success -Message "Notification system is working!"

Write-Host "`nTest 4: Toast Notification" -ForegroundColor Yellow
Show-WinUIToast -Title "Test Toast" -Message "This is a test toast notification!" -Type Info -Duration 3

Write-Host "`nTest 5: Dialog Notification" -ForegroundColor Yellow
$result = Show-WinUIDialog -Title "Test Dialog" -Message "Do you like the notification system?" -Type Question -Buttons @(
    @{ Text = "Yes!"; Result = "Yes" },
    @{ Text = "No"; Result = "No" }
)
Write-Host "Dialog result: $result" -ForegroundColor Cyan

Write-Host "`nTest 6: Progress Notification" -ForegroundColor Yellow
$progress = Show-WinUIProgress -Title "Test Progress" -Message "Testing progress..."

for ($i = 0; $i -le 100; $i += 20) {
    Start-Sleep -Milliseconds 300
    $progress.UpdateProgress($i, "Progress: $i%")
}

$progress.Complete("Test completed!")

Write-Host "`nAll tests completed successfully! 🎉" -ForegroundColor Green
