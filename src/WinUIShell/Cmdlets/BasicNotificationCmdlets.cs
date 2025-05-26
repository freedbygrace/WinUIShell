using System.Management.Automation;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Shows a toast notification
/// </summary>
[Cmdlet(VerbsCommon.Show, "WinUIToast")]
[OutputType(typeof(Window))]
public class ShowWinUIToastCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Title { get; set; } = "";

    [Parameter(Mandatory = true)]
    public string Message { get; set; } = "";

    [Parameter]
    [ValidateSet("Info", "Success", "Warning", "Error")]
    public NotificationType Type { get; set; } = NotificationType.Info;

    [Parameter]
    public int Duration { get; set; } = 5;

    [Parameter]
    public NotificationTheme? Theme { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        if (Theme == null)
        {
            // Create default theme using helper method
            Theme = NotificationThemeHelper.CreateDefaultTheme();
        }

        var toast = new Window
        {
            ExtendsContentIntoTitleBar = true,
            SystemBackdrop = new MicaBackdrop()
        };
        toast.AppWindow.ResizeClient(350, 120);

        // Get type-specific color
        Color typeColor = Type switch
        {
            NotificationType.Success => Theme.Success,
            NotificationType.Warning => Theme.Warning,
            NotificationType.Error => Theme.Error,
            NotificationType.Question => Theme.Primary,
            NotificationType.Info => Theme.Info,
            _ => Theme.Info
        };

        // Create content
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(16, 16, 16, 16),
            Background = new SolidColorBrush(Theme.Surface)
        };

        // Title
        var titleBlock = new TextBlock
        {
            Text = Title,
            FontWeight = new FontWeight(600),
            FontSize = 16,
            Foreground = new SolidColorBrush(typeColor),
            Margin = new Thickness(0, 0, 0, 8)
        };

        // Message
        var messageBlock = new TextBlock
        {
            Text = Message,
            FontSize = 14,
            Foreground = new SolidColorBrush(Theme.OnSurface),
            TextWrapping = TextWrapping.Wrap
        };

        mainPanel.Children.Add(titleBlock);
        mainPanel.Children.Add(messageBlock);
        toast.Content = mainPanel;

        toast.Activate();

        // Auto-dismiss if duration is set
        if (Duration > 0)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(Duration * 1000);
                toast.Close();
            });
        }

        WriteObject(toast);
    }
}

/// <summary>
/// Quick helper for common notification scenarios
/// </summary>
[Cmdlet(VerbsCommon.New, "WinUIQuickNotification")]
[OutputType(typeof(Window))]
public class NewWinUIQuickNotificationCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    [ValidateSet("Success", "Warning", "Error", "Info")]
    public NotificationType Type { get; set; }

    [Parameter(Mandatory = true)]
    public string Message { get; set; } = "";

    [Parameter]
    public string? Title { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        var title = Title ?? Type.ToString();
        
        // Create toast directly using helper
        var theme = NotificationThemeHelper.CreateDefaultTheme();
        var toast = new Window
        {
            ExtendsContentIntoTitleBar = true,
            SystemBackdrop = new MicaBackdrop()
        };
        toast.AppWindow.ResizeClient(350, 120);

        // Get type-specific color
        Color typeColor = Type switch
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
            Text = Message,
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
