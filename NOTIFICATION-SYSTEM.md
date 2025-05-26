# WinUIShell Notification System

The WinUIShell notification system provides a comprehensive set of cmdlets for creating beautiful, theme-aware notifications in PowerShell scripts. The system automatically adapts to the Windows system theme (Light/Dark) and provides consistent styling across all notification types.

## Features

- **Automatic Theme Detection**: Detects Windows Light/Dark theme and applies appropriate colors
- **Multiple Notification Types**: Toast, Dialog, and Progress notifications
- **Customizable Themes**: Override system theme with custom color schemes
- **Type-Specific Styling**: Different colors for Success, Warning, Error, Info, and Question types
- **Modern UI**: Uses WinUI 3 with Mica backdrop effects
- **Easy to Use**: Simple cmdlets with sensible defaults

## Available Cmdlets

### Theme Management

#### `Get-WinUISystemTheme`
Detects the current Windows system theme.

```powershell
$theme = Get-WinUISystemTheme
# Returns: "Light" or "Dark"
```

#### `New-WinUINotificationTheme`
Creates a theme configuration for notifications.

```powershell
# Use system theme
$theme = New-WinUINotificationTheme

# Force dark theme with custom accent
$darkTheme = New-WinUINotificationTheme -ThemeMode Dark -AccentColor ([Colors]::Purple)

# Force light theme
$lightTheme = New-WinUINotificationTheme -ThemeMode Light
```

### Toast Notifications

#### `Show-WinUIToast`
Shows brief, non-blocking toast notifications.

```powershell
# Basic toast
Show-WinUIToast -Title "Success" -Message "Operation completed!" -Type Success

# Custom duration and position
Show-WinUIToast -Title "Warning" -Message "Low disk space" -Type Warning -Duration 10 -Position TopLeft

# With custom theme
Show-WinUIToast -Title "Info" -Message "Custom styled notification" -Theme $customTheme
```

**Parameters:**
- `Title` (Required): The notification title
- `Message` (Required): The notification message
- `Type`: Info, Success, Warning, Error (default: Info)
- `Duration`: Auto-dismiss time in seconds (default: 5, 0 = no auto-dismiss)
- `Theme`: Custom theme configuration
- `Position`: TopLeft, TopRight, BottomLeft, BottomRight (default: TopRight)

### Dialog Notifications

#### `Show-WinUIDialog`
Shows modal dialog notifications with custom buttons.

```powershell
# Confirmation dialog
$result = Show-WinUIDialog -Title "Confirm" -Message "Delete this file?" -Type Question -Buttons @(
    @{ Text = "Yes"; Result = "Yes" },
    @{ Text = "No"; Result = "No" }
)

# Error dialog
Show-WinUIDialog -Title "Error" -Message "Something went wrong!" -Type Error

# Multi-choice dialog
$choice = Show-WinUIDialog -Title "Save Changes" -Message "What would you like to do?" -Type Question -Buttons @(
    @{ Text = "Save"; Result = "Save" },
    @{ Text = "Don't Save"; Result = "DontSave" },
    @{ Text = "Cancel"; Result = "Cancel" }
) -DefaultButton 0
```

**Parameters:**
- `Title` (Required): The dialog title
- `Message` (Required): The dialog message
- `Type`: Info, Success, Warning, Error, Question (default: Info)
- `Buttons`: Array of button definitions with Text and Result properties
- `Theme`: Custom theme configuration
- `DefaultButton`: Index of the default button (0-based)

**Returns:** The Result value of the clicked button

### Progress Notifications

#### `Show-WinUIProgress`
Shows progress notifications with updatable progress bars.

```powershell
# Determinate progress
$progress = Show-WinUIProgress -Title "Processing" -Message "Starting..."
$progress.UpdateProgress(50, "Half way done...")
$progress.Complete("Finished!")

# Indeterminate progress with cancellation
$progress = Show-WinUIProgress -Title "Loading" -IndeterminateMode -Cancellable
# Check if cancelled: $progress.IsCancelled.Invoke()
$progress.Close()
```

**Parameters:**
- `Title` (Required): The progress window title
- `Message`: Initial message (default: "Please wait...")
- `Theme`: Custom theme configuration
- `Cancellable`: Add a cancel button
- `IndeterminateMode`: Show spinning progress instead of percentage

**Returns:** Progress controller object with methods:
- `UpdateProgress($percentage, $message)`: Update progress and message
- `Complete($finalMessage)`: Complete and close after brief delay
- `Close()`: Close immediately
- `IsCancelled.Invoke()`: Check if user cancelled

### Quick Helpers

#### `New-WinUIQuickNotification`
Shortcut for common notification scenarios.

```powershell
New-WinUIQuickNotification -Type Success -Message "File saved!"
New-WinUIQuickNotification -Type Error -Message "Connection failed" -Title "Network Error"
```

## Theme System

The notification system uses a comprehensive theme system that adapts to Windows appearance settings:

### Light Theme Colors
- **Background**: White
- **Surface**: WhiteSmoke
- **Primary**: DodgerBlue
- **Success**: Green
- **Warning**: DarkOrange
- **Error**: Crimson
- **Info**: RoyalBlue

### Dark Theme Colors
- **Background**: Gray20
- **Surface**: Gray30
- **Primary**: SkyBlue
- **Success**: LightGreen
- **Warning**: Orange
- **Error**: LightCoral
- **Info**: LightBlue

### Custom Themes
You can create custom themes by overriding the theme mode or accent color:

```powershell
$customTheme = New-WinUINotificationTheme -ThemeMode Dark -AccentColor ([Colors]::Purple)
```

## Best Practices

1. **Use Appropriate Types**: Choose the correct notification type (Success, Warning, Error, Info) for better user experience
2. **Keep Messages Concise**: Toast notifications should be brief and scannable
3. **Provide Clear Actions**: Dialog buttons should have clear, action-oriented text
4. **Handle Cancellation**: Always check for cancellation in long-running operations
5. **Test Both Themes**: Verify your notifications look good in both Light and Dark themes

## Examples

See `examples/NotificationExamples.ps1` for comprehensive examples of all notification types and features.

## Integration with Scripts

The notification system is perfect for:
- **Script Progress**: Show progress during long-running operations
- **User Confirmations**: Get user input for critical decisions
- **Status Updates**: Inform users of script completion or errors
- **Background Tasks**: Notify when background processes complete
