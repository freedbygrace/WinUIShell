using namespace WinUIShell
if (-not (Get-Module WinUIShell)) {
    Import-Module WinUIShell
}

# Example 1: Basic Toast Notifications
Write-Host "Example 1: Basic Toast Notifications" -ForegroundColor Green

# Success notification
Show-WinUIToast -Title "Success" -Message "File saved successfully!" -Type Success -Duration 3

Start-Sleep -Seconds 1

# Warning notification
Show-WinUIToast -Title "Warning" -Message "Low disk space detected" -Type Warning -Duration 3

Start-Sleep -Seconds 1

# Error notification
Show-WinUIToast -Title "Error" -Message "Failed to connect to server" -Type Error -Duration 3

Start-Sleep -Seconds 1

# Info notification
Show-WinUIToast -Title "Info" -Message "New update available" -Type Info -Duration 3

Start-Sleep -Seconds 4

# Example 2: Custom Theme
Write-Host "Example 2: Custom Theme" -ForegroundColor Green

$customTheme = New-WinUINotificationTheme -ThemeMode Dark -AccentColor ([Colors]::Purple)
Show-WinUIToast -Title "Custom Theme" -Message "This uses a custom purple theme!" -Type Info -Theme $customTheme -Duration 3

Start-Sleep -Seconds 4

# Example 3: Dialog Notifications
Write-Host "Example 3: Dialog Notifications" -ForegroundColor Green

# Simple confirmation dialog
$result = Show-WinUIDialog -Title "Confirm Action" -Message "Are you sure you want to delete this file?" -Type Question -Buttons @(
    @{ Text = "Yes"; Result = "Yes" },
    @{ Text = "No"; Result = "No" }
) -DefaultButton 1

Write-Host "User selected: $result" -ForegroundColor Yellow

# Error dialog with single button
Show-WinUIDialog -Title "Error" -Message "An unexpected error occurred. Please try again later." -Type Error -Buttons @(
    @{ Text = "OK"; Result = "OK" }
)

# Multi-option dialog
$choice = Show-WinUIDialog -Title "Save Changes" -Message "You have unsaved changes. What would you like to do?" -Type Question -Buttons @(
    @{ Text = "Save"; Result = "Save" },
    @{ Text = "Don't Save"; Result = "DontSave" },
    @{ Text = "Cancel"; Result = "Cancel" }
) -DefaultButton 0

Write-Host "User choice: $choice" -ForegroundColor Yellow

# Example 4: Progress Notifications
Write-Host "Example 4: Progress Notifications" -ForegroundColor Green

# Determinate progress
$progress = Show-WinUIProgress -Title "Processing Files" -Message "Starting operation..."

for ($i = 0; $i -le 100; $i += 10) {
    Start-Sleep -Milliseconds 200
    $progress.UpdateProgress($i, "Processing file $i of 100...")
}

$progress.Complete("All files processed successfully!")

Start-Sleep -Seconds 2

# Indeterminate progress with cancellation
$progress2 = Show-WinUIProgress -Title "Downloading" -Message "Connecting to server..." -IndeterminateMode -Cancellable

# Simulate some work
for ($i = 0; $i -lt 50; $i++) {
    Start-Sleep -Milliseconds 100
    if ($progress2.IsCancelled.Invoke()) {
        Write-Host "Operation was cancelled by user" -ForegroundColor Yellow
        break
    }
}

if (-not $progress2.IsCancelled.Invoke()) {
    $progress2.Complete("Download completed!")
} else {
    $progress2.Close()
}

# Example 5: Quick Notifications
Write-Host "Example 5: Quick Notifications" -ForegroundColor Green

New-WinUIQuickNotification -Type Success -Message "Operation completed successfully!"
Start-Sleep -Seconds 2

New-WinUIQuickNotification -Type Warning -Message "Please check your settings"
Start-Sleep -Seconds 2

New-WinUIQuickNotification -Type Error -Message "Connection failed" -Title "Network Error"
Start-Sleep -Seconds 2

# Example 6: System Theme Detection
Write-Host "Example 6: System Theme Detection" -ForegroundColor Green

$systemTheme = Get-WinUISystemTheme
Write-Host "Current system theme: $systemTheme" -ForegroundColor Yellow

$theme = New-WinUINotificationTheme
Write-Host "Theme colors:" -ForegroundColor Yellow
Write-Host "  Mode: $($theme.Mode)"
Write-Host "  Primary: $($theme.Primary)"
Write-Host "  Background: $($theme.Background)"

Show-WinUIToast -Title "Theme Demo" -Message "This notification uses your system theme: $systemTheme" -Type Info -Duration 5

Write-Host "All notification examples completed!" -ForegroundColor Green
