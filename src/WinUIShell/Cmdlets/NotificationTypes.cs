using Microsoft.Win32;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error,
    Question
}

/// <summary>
/// Notification position on screen
/// </summary>
public enum NotificationPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
    Custom
}

/// <summary>
/// Window chrome options for notifications
/// </summary>
public enum WindowChrome
{
    Default,
    None,
    MinimizeOnly,
    CloseOnly,
    Custom
}

/// <summary>
/// Taskbar visibility options
/// </summary>
public enum TaskbarVisibility
{
    Visible,
    Hidden,
    IconOnly
}

/// <summary>
/// Theme mode enumeration
/// </summary>
public enum ThemeMode
{
    Auto,
    Light,
    Dark
}

/// <summary>
/// Notification theme configuration
/// </summary>
public class NotificationTheme
{
    public string Mode { get; set; } = "Light";
    public Color Background { get; set; } = Colors.White;
    public Color Surface { get; set; } = Colors.WhiteSmoke;
    public Color OnSurface { get; set; } = Colors.Black;
    public Color OnSurfaceVariant { get; set; } = Colors.DarkGray;
    public Color Primary { get; set; } = Colors.DodgerBlue;
    public Color OnPrimary { get; set; } = Colors.White;
    public Color Success { get; set; } = Colors.Green;
    public Color Warning { get; set; } = Colors.DarkOrange;
    public Color Error { get; set; } = Colors.Crimson;
    public Color Info { get; set; } = Colors.RoyalBlue;
    public Color Border { get; set; } = Colors.LightGray;
}

/// <summary>
/// Enhanced notification configuration
/// </summary>
public class NotificationConfig
{
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public NotificationType Type { get; set; } = NotificationType.Info;
    public NotificationPosition Position { get; set; } = NotificationPosition.TopRight;
    public WindowChrome Chrome { get; set; } = WindowChrome.None;
    public TaskbarVisibility TaskbarVisibility { get; set; } = TaskbarVisibility.Hidden;
    public int Duration { get; set; } = 5; // seconds, 0 = no auto-dismiss
    public int Width { get; set; } = 350;
    public int Height { get; set; } = 120;
    public string? IconPath { get; set; }
    public bool Topmost { get; set; } = true;
    public bool ShowInTaskbar { get; set; }
    public bool Resizable { get; set; }
    public NotificationTheme? Theme { get; set; }
    public int? CustomX { get; set; }
    public int? CustomY { get; set; }
}

/// <summary>
/// Button definition for dialogs
/// </summary>
public class DialogButton
{
    public string Text { get; set; } = "";
    public string Result { get; set; } = "";
    public bool IsDefault { get; set; }
    public bool IsCancel { get; set; }
}

/// <summary>
/// Helper class for notification themes
/// </summary>
public static class NotificationThemeHelper
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static NotificationTheme CreateDefaultTheme()
    {
        string actualTheme;
        
        // Detect system theme
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                actualTheme = value == 0 ? "Dark" : "Light";
            }
            else
            {
                actualTheme = "Light";
            }
        }
        catch
        {
            actualTheme = "Light";
        }

        var theme = new NotificationTheme { Mode = actualTheme };

        if (actualTheme == "Dark")
        {
            theme.Background = Color.FromArgb(255, 32, 32, 32);  // Dark gray
            theme.Surface = Color.FromArgb(255, 48, 48, 48);     // Slightly lighter gray
            theme.OnSurface = Colors.White;
            theme.OnSurfaceVariant = Colors.LightGray;
            theme.Primary = Colors.SkyBlue;
            theme.OnPrimary = Colors.Black;
            theme.Success = Colors.LightGreen;
            theme.Warning = Colors.Orange;
            theme.Error = Colors.LightCoral;
            theme.Info = Colors.LightBlue;
            theme.Border = Color.FromArgb(255, 64, 64, 64);      // Border gray
        }
        else
        {
            theme.Background = Colors.White;
            theme.Surface = Colors.WhiteSmoke;
            theme.OnSurface = Colors.Black;
            theme.OnSurfaceVariant = Colors.DarkGray;
            theme.Primary = Colors.DodgerBlue;
            theme.OnPrimary = Colors.White;
            theme.Success = Colors.Green;
            theme.Warning = Colors.DarkOrange;
            theme.Error = Colors.Crimson;
            theme.Info = Colors.RoyalBlue;
            theme.Border = Colors.LightGray;
        }

        return theme;
    }
}

/// <summary>
/// Window management helper for notifications
/// </summary>
public static class NotificationWindowHelper
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static void ConfigureWindow(Window window, NotificationConfig config)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(config);

        // Set basic properties
        window.ExtendsContentIntoTitleBar = config.Chrome == WindowChrome.None;
        window.SystemBackdrop = new MicaBackdrop();
        
        // Set size
        window.AppWindow.ResizeClient(config.Width, config.Height);
        
        // Configure window chrome (simplified for now)
        if (config.Chrome == WindowChrome.None)
        {
            window.ExtendsContentIntoTitleBar = true;
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static NotificationConfig CreateConfig(
        string title,
        string message,
        NotificationType type = NotificationType.Info,
        NotificationPosition position = NotificationPosition.TopRight,
        WindowChrome chrome = WindowChrome.None,
        int duration = 5)
    {
        return new NotificationConfig
        {
            Title = title,
            Message = message,
            Type = type,
            Position = position,
            Chrome = chrome,
            Duration = duration,
            Theme = NotificationThemeHelper.CreateDefaultTheme()
        };
    }
}
