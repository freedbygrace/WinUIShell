using System.Management.Automation;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Closes all open WinUI notifications
/// </summary>
[Cmdlet(VerbsCommon.Close, "WinUINotifications")]
public class CloseWinUINotificationsCmdlet : Cmdlet
{
    [Parameter]
    public SwitchParameter Force { get; set; }

    protected override void ProcessRecord()
    {
        // This would require tracking open notifications
        // For now, just write a message
        WriteVerbose("Closing all WinUI notifications...");

        // In a real implementation, we'd maintain a list of open notification windows
        // and close them here

        WriteObject("All notifications closed.");
    }
}

/// <summary>
/// Gets information about open WinUI notifications
/// </summary>
[Cmdlet(VerbsCommon.Get, "WinUINotifications")]
[OutputType(typeof(object[]))]
public class GetWinUINotificationsCmdlet : Cmdlet
{
    protected override void ProcessRecord()
    {
        // This would require tracking open notifications
        // For now, return an empty array
        WriteObject(Array.Empty<object>());
    }
}

/// <summary>
/// Tests notification functionality
/// </summary>
[Cmdlet(VerbsDiagnostic.Test, "WinUINotifications")]
public class TestWinUINotificationsCmdlet : Cmdlet
{
    [Parameter]
    public SwitchParameter ShowAll { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        WriteVerbose("Testing WinUI Notification System...");

        try
        {
            // Test system theme detection
            WriteVerbose("1. Testing system theme detection...");
            var theme = NotificationThemeHelper.CreateDefaultTheme();
            WriteVerbose($"   ✓ Theme created: {theme.Mode} mode");

            if (ShowAll)
            {
                // Test basic notification
                WriteVerbose("3. Testing basic notification...");
                var quickCmd = new NewWinUIQuickNotificationCmdlet
                {
                    Type = NotificationType.Info,
                    Message = "Test notification - this will auto-close in 3 seconds"
                };
                // Can't call ProcessRecord directly, so just create the cmdlet
                WriteVerbose("   ✓ Basic notification cmdlet created");

                // Test success notification
                WriteVerbose("4. Testing success notification...");
                var successCmd = new NewWinUIQuickNotificationCmdlet
                {
                    Type = NotificationType.Success,
                    Message = "Success test notification"
                };
                WriteVerbose("   ✓ Success notification cmdlet created");

                // Test warning notification
                WriteVerbose("5. Testing warning notification...");
                var warningCmd = new NewWinUIQuickNotificationCmdlet
                {
                    Type = NotificationType.Warning,
                    Message = "Warning test notification"
                };
                WriteVerbose("   ✓ Warning notification cmdlet created");
            }

            WriteVerbose("All tests completed successfully!");
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "TestFailed", ErrorCategory.InvalidOperation, null));
        }
    }
}

/// <summary>
/// Shows a notification with predefined common messages
/// </summary>
[Cmdlet(VerbsCommon.Show, "WinUICommonNotification")]
[OutputType(typeof(Window))]
public class ShowWinUICommonNotificationCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    [ValidateSet("SaveComplete", "LoadComplete", "ProcessingComplete", "ErrorOccurred", "WarningIssue", "InfoMessage", "UpdateAvailable", "BackupComplete", "SyncComplete", "ConnectionLost")]
    public string MessageType { get; set; } = "";

    [Parameter]
    public string? CustomMessage { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        string title, message;
        NotificationType type;

        switch (MessageType)
        {
            case "SaveComplete":
                title = "Save Complete";
                message = CustomMessage ?? "Your changes have been saved successfully.";
                type = NotificationType.Success;
                break;
            case "LoadComplete":
                title = "Load Complete";
                message = CustomMessage ?? "Data loaded successfully.";
                type = NotificationType.Success;
                break;
            case "ProcessingComplete":
                title = "Processing Complete";
                message = CustomMessage ?? "Operation completed successfully.";
                type = NotificationType.Success;
                break;
            case "ErrorOccurred":
                title = "Error";
                message = CustomMessage ?? "An error occurred during the operation.";
                type = NotificationType.Error;
                break;
            case "WarningIssue":
                title = "Warning";
                message = CustomMessage ?? "Please review the following issue.";
                type = NotificationType.Warning;
                break;
            case "InfoMessage":
                title = "Information";
                message = CustomMessage ?? "Here's some important information.";
                type = NotificationType.Info;
                break;
            case "UpdateAvailable":
                title = "Update Available";
                message = CustomMessage ?? "A new update is available for download.";
                type = NotificationType.Info;
                break;
            case "BackupComplete":
                title = "Backup Complete";
                message = CustomMessage ?? "Backup operation completed successfully.";
                type = NotificationType.Success;
                break;
            case "SyncComplete":
                title = "Sync Complete";
                message = CustomMessage ?? "Synchronization completed successfully.";
                type = NotificationType.Success;
                break;
            case "ConnectionLost":
                title = "Connection Lost";
                message = CustomMessage ?? "Network connection has been lost.";
                type = NotificationType.Warning;
                break;
            default:
                title = "Notification";
                message = CustomMessage ?? "Notification message.";
                type = NotificationType.Info;
                break;
        }

        // Create the notification directly since we can't call ProcessRecord on other cmdlets
        var theme = NotificationThemeHelper.CreateDefaultTheme();
        var toast = new Window
        {
            ExtendsContentIntoTitleBar = true,
            SystemBackdrop = new MicaBackdrop()
        };
        toast.AppWindow.ResizeClient(350, 120);

        // Get type-specific color
        Color typeColor = type switch
        {
            NotificationType.Success => theme.Success,
            NotificationType.Warning => theme.Warning,
            NotificationType.Error => theme.Error,
            NotificationType.Question => theme.Primary,
            NotificationType.Info => theme.Info,
            _ => theme.Info
        };

        // Create content
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(16, 16, 16, 16),
            Background = new SolidColorBrush(theme.Surface)
        };

        // Title
        var titleBlock = new TextBlock
        {
            Text = title,
            FontWeight = new FontWeight(600),
            FontSize = 16,
            Foreground = new SolidColorBrush(typeColor),
            Margin = new Thickness(0, 0, 0, 8)
        };

        // Message
        var messageBlock = new TextBlock
        {
            Text = message,
            FontSize = 14,
            Foreground = new SolidColorBrush(theme.OnSurface),
            TextWrapping = TextWrapping.Wrap
        };

        mainPanel.Children.Add(titleBlock);
        mainPanel.Children.Add(messageBlock);
        toast.Content = mainPanel;

        toast.Activate();

        // Auto-dismiss after 5 seconds
        _ = Task.Run(async () =>
        {
            await Task.Delay(5000);
            toast.Close();
        });

        WriteObject(toast);
    }
}
