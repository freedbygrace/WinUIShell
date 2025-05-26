using System.Management.Automation;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Creates a comprehensive notification configuration
/// </summary>
[Cmdlet(VerbsCommon.New, "WinUINotificationConfig")]
[OutputType(typeof(NotificationConfig))]
public class NewWinUINotificationConfigCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Title { get; set; } = "";

    [Parameter(Mandatory = true)]
    public string Message { get; set; } = "";

    [Parameter]
    [ValidateSet("Info", "Success", "Warning", "Error", "Question")]
    public NotificationType Type { get; set; } = NotificationType.Info;

    [Parameter]
    [ValidateSet("TopLeft", "TopCenter", "TopRight", "MiddleLeft", "MiddleCenter", "MiddleRight", "BottomLeft", "BottomCenter", "BottomRight", "Custom")]
    public NotificationPosition Position { get; set; } = NotificationPosition.TopRight;

    [Parameter]
    [ValidateSet("Default", "None", "MinimizeOnly", "CloseOnly", "Custom")]
    public WindowChrome Chrome { get; set; } = WindowChrome.None;

    [Parameter]
    [ValidateSet("Visible", "Hidden", "IconOnly")]
    public TaskbarVisibility TaskbarVisibility { get; set; } = TaskbarVisibility.Hidden;

    [Parameter]
    public int Duration { get; set; } = 5;

    [Parameter]
    public int Width { get; set; } = 350;

    [Parameter]
    public int Height { get; set; } = 120;

    [Parameter]
    public string? IconPath { get; set; }

    [Parameter]
    public SwitchParameter Topmost { get; set; }

    [Parameter]
    public SwitchParameter ShowInTaskbar { get; set; }

    [Parameter]
    public SwitchParameter Resizable { get; set; }

    [Parameter]
    public NotificationTheme? Theme { get; set; }

    [Parameter]
    public int? CustomX { get; set; }

    [Parameter]
    public int? CustomY { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        var config = new NotificationConfig
        {
            Title = Title,
            Message = Message,
            Type = Type,
            Position = Position,
            Chrome = Chrome,
            TaskbarVisibility = TaskbarVisibility,
            Duration = Duration,
            Width = Width,
            Height = Height,
            IconPath = IconPath,
            Topmost = Topmost,
            ShowInTaskbar = ShowInTaskbar,
            Resizable = Resizable,
            Theme = Theme ?? NotificationThemeHelper.CreateDefaultTheme(),
            CustomX = CustomX,
            CustomY = CustomY
        };

        WriteObject(config);
    }
}

/// <summary>
/// Shows an advanced notification using a configuration object
/// </summary>
[Cmdlet(VerbsCommon.Show, "WinUIAdvancedNotification")]
[OutputType(typeof(Window))]
public class ShowWinUIAdvancedNotificationCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public NotificationConfig Config { get; set; } = new();

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        ArgumentNullException.ThrowIfNull(Config);

        if (Config.Theme == null)
        {
            Config.Theme = NotificationThemeHelper.CreateDefaultTheme();
        }

        var notification = new Window();

        // Configure the window using the helper
        NotificationWindowHelper.ConfigureWindow(notification, Config);

        // Get type-specific color
        Color typeColor = Config.Type switch
        {
            NotificationType.Success => Config.Theme.Success,
            NotificationType.Warning => Config.Theme.Warning,
            NotificationType.Error => Config.Theme.Error,
            NotificationType.Question => Config.Theme.Primary,
            NotificationType.Info => Config.Theme.Info,
            _ => Config.Theme.Info
        };

        // Create content
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(16, 16, 16, 16),
            Background = new SolidColorBrush(Config.Theme.Surface)
        };

        // Add icon if specified (simplified - would need proper image handling)
        if (!string.IsNullOrEmpty(Config.IconPath))
        {
            // For now, just add a placeholder text for the icon
            var iconText = new TextBlock
            {
                Text = "🔔", // Bell emoji as placeholder
                FontSize = 24,
                Margin = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            mainPanel.Children.Add(iconText);
        }

        // Title
        var titleBlock = new TextBlock
        {
            Text = Config.Title,
            FontWeight = new FontWeight(600),
            FontSize = 16,
            Foreground = new SolidColorBrush(typeColor),
            Margin = new Thickness(0, 0, 0, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        // Message
        var messageBlock = new TextBlock
        {
            Text = Config.Message,
            FontSize = 14,
            Foreground = new SolidColorBrush(Config.Theme.OnSurface),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        mainPanel.Children.Add(titleBlock);
        mainPanel.Children.Add(messageBlock);
        notification.Content = mainPanel;

        notification.Activate();

        // Auto-dismiss if duration is set
        if (Config.Duration > 0)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(Config.Duration * 1000);
                notification.Close();
            });
        }

        WriteObject(notification);
    }
}

/// <summary>
/// Creates a notification builder for fluent configuration
/// </summary>
[Cmdlet(VerbsCommon.New, "WinUINotificationBuilder")]
[OutputType(typeof(NotificationConfig))]
public class NewWinUINotificationBuilderCmdlet : Cmdlet
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        var config = new NotificationConfig
        {
            Theme = NotificationThemeHelper.CreateDefaultTheme()
        };

        WriteObject(config);
    }
}
