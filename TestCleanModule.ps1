using namespace WinUIShell

# Test the clean module
Write-Host "Testing Clean WinUIShell Module..." -ForegroundColor Green

# Import the clean module
Import-Module ".\module-clean\WinUIShell" -Force

Write-Host "Module imported successfully!" -ForegroundColor Green

# Test 1: System theme detection
Write-Host "`nTest 1: System Theme Detection" -ForegroundColor Yellow
try {
    $systemTheme = Get-WinUISystemTheme
    Write-Host "✅ Current system theme: $systemTheme" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to detect system theme: $_" -ForegroundColor Red
}

# Test 2: Theme creation
Write-Host "`nTest 2: Theme Creation" -ForegroundColor Yellow
try {
    $theme = New-WinUINotificationTheme
    Write-Host "✅ Theme created with mode: $($theme.Mode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create theme: $_" -ForegroundColor Red
}

# Test 3: Quick notification
Write-Host "`nTest 3: Quick Notification" -ForegroundColor Yellow
try {
    New-WinUIQuickNotification -Type Success -Message "Clean module is working!"
    Write-Host "✅ Quick notification displayed" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to show quick notification: $_" -ForegroundColor Red
}

# Test 4: Basic window creation
Write-Host "`nTest 4: Basic Window Creation" -ForegroundColor Yellow
try {
    $window = [Window]::new()
    $window.Title = "Clean Module Test"
    $window.AppWindow.ResizeClient(300, 200)
    
    $textBlock = [TextBlock]::new()
    $textBlock.Text = "Clean module working!"
    $textBlock.HorizontalAlignment = "Center"
    $textBlock.VerticalAlignment = "Center"
    $textBlock.FontSize = 16
    
    $window.Content = $textBlock
    $window.Activate()
    
    Write-Host "✅ Window created and displayed" -ForegroundColor Green
    
    # Close window after 2 seconds
    Start-Job -ScriptBlock {
        Start-Sleep -Seconds 2
    } | Out-Null
    
} catch {
    Write-Host "❌ Failed to create window: $_" -ForegroundColor Red
}

Write-Host "`nClean module test completed! 🎉" -ForegroundColor Green

# Display module information
Write-Host "`nModule Information:" -ForegroundColor Cyan
$module = Get-Module WinUIShell
Write-Host "Name: $($module.Name)" -ForegroundColor White
Write-Host "Version: $($module.Version)" -ForegroundColor White
Write-Host "Path: $($module.Path)" -ForegroundColor White
Write-Host "Exported Functions: $($module.ExportedFunctions.Count)" -ForegroundColor White

Write-Host "`nExported Functions:" -ForegroundColor Cyan
$module.ExportedFunctions.Keys | Sort-Object | ForEach-Object {
    Write-Host "  - $_" -ForegroundColor White
}
