using System.Management.Automation;
using System.Runtime.InteropServices;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Win32 API imports for advanced window management
/// </summary>
internal static class Win32Api
{
    [DllImport("user32.dll")]
    internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    internal static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    internal static extern long SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

    [DllImport("user32.dll")]
    internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("shell32.dll")]
    internal static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

    [DllImport("user32.dll")]
    internal static extern bool DestroyIcon(IntPtr hIcon);

    // Window style constants
    internal const int GWL_STYLE = -16;
    internal const int GWL_EXSTYLE = -20;
    internal const long WS_MINIMIZEBOX = 0x20000L;
    internal const long WS_MAXIMIZEBOX = 0x10000L;
    internal const long WS_SYSMENU = 0x80000L;
    internal const long WS_EX_TOOLWINDOW = 0x80L;
    internal const long WS_EX_APPWINDOW = 0x40000L;

    // SetWindowPos flags
    internal const uint SWP_NOMOVE = 0x0002;
    internal const uint SWP_NOSIZE = 0x0001;
    internal const uint SWP_NOZORDER = 0x0004;
    internal const uint SWP_FRAMECHANGED = 0x0020;

    // Special window positions
    internal static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    internal static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
}

/// <summary>
/// Icon information for notifications
/// </summary>
public class NotificationIcon
{
    public string Path { get; set; } = "";
    public int Index { get; set; } = 0;
    public int Size { get; set; } = 32;
    public string Type { get; set; } = "File"; // File, Resource, Emoji, SystemIcon
    public string SystemIconName { get; set; } = ""; // For system icons
}

/// <summary>
/// Advanced window configuration
/// </summary>
public class WindowConfiguration
{
    public bool HasMinimizeButton { get; set; } = true;
    public bool HasMaximizeButton { get; set; } = true;
    public bool HasCloseButton { get; set; } = true;
    public bool ShowInTaskbar { get; set; } = true;
    public bool IsTopmost { get; set; } = false;
    public bool IsResizable { get; set; } = true;
    public int X { get; set; } = -1;
    public int Y { get; set; } = -1;
    public int Width { get; set; } = 400;
    public int Height { get; set; } = 200;
    public NotificationIcon? Icon { get; set; }
}

/// <summary>
/// Configures advanced window properties
/// </summary>
[Cmdlet(VerbsCommon.Set, "WinUIWindowProperties")]
public class SetWinUIWindowPropertiesCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public Window Window { get; set; } = null!;

    [Parameter]
    public WindowConfiguration? Configuration { get; set; }

    [Parameter]
    public SwitchParameter HideMinimizeButton { get; set; }

    [Parameter]
    public SwitchParameter HideMaximizeButton { get; set; }

    [Parameter]
    public SwitchParameter HideFromTaskbar { get; set; }

    [Parameter]
    public SwitchParameter SetTopmost { get; set; }

    [Parameter]
    public NotificationIcon? Icon { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        ArgumentNullException.ThrowIfNull(Window);

        try
        {
            // Get window handle (this would need proper WinUI3 integration)
            var hwnd = GetWindowHandle(Window);
            if (hwnd == IntPtr.Zero)
            {
                WriteWarning("Could not get window handle for advanced configuration");
                return;
            }

            var config = Configuration ?? new WindowConfiguration();

            // Apply button visibility
            if (HideMinimizeButton || !config.HasMinimizeButton)
            {
                RemoveWindowStyle(hwnd, Win32Api.WS_MINIMIZEBOX);
            }

            if (HideMaximizeButton || !config.HasMaximizeButton)
            {
                RemoveWindowStyle(hwnd, Win32Api.WS_MAXIMIZEBOX);
            }

            // Handle taskbar visibility
            if (HideFromTaskbar || !config.ShowInTaskbar)
            {
                SetWindowExStyle(hwnd, Win32Api.WS_EX_TOOLWINDOW, true);
                SetWindowExStyle(hwnd, Win32Api.WS_EX_APPWINDOW, false);
            }

            // Set topmost
            if (SetTopmost || config.IsTopmost)
            {
                Win32Api.SetWindowPos(hwnd, Win32Api.HWND_TOPMOST, 0, 0, 0, 0,
                    Win32Api.SWP_NOMOVE | Win32Api.SWP_NOSIZE);
            }

            // Set position and size
            if (config.X >= 0 && config.Y >= 0)
            {
                Win32Api.SetWindowPos(hwnd, IntPtr.Zero, config.X, config.Y, config.Width, config.Height, 0);
            }

            // Apply icon if specified
            if (Icon != null || config.Icon != null)
            {
                ApplyIcon(Window, Icon ?? config.Icon!);
            }

            WriteVerbose("Window properties configured successfully");
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "WindowConfigError", ErrorCategory.InvalidOperation, Window));
        }
    }

    private static IntPtr GetWindowHandle(Window window)
    {
        // This would need proper WinUI3 integration to get the HWND
        // For now, return IntPtr.Zero to indicate we can't get the handle
        return IntPtr.Zero;
    }

    private static void RemoveWindowStyle(IntPtr hwnd, long style)
    {
        var currentStyle = Win32Api.GetWindowLong(hwnd, Win32Api.GWL_STYLE);
        Win32Api.SetWindowLong(hwnd, Win32Api.GWL_STYLE, currentStyle & ~style);
        Win32Api.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            Win32Api.SWP_NOMOVE | Win32Api.SWP_NOSIZE | Win32Api.SWP_NOZORDER | Win32Api.SWP_FRAMECHANGED);
    }

    private static void SetWindowExStyle(IntPtr hwnd, long style, bool enable)
    {
        var currentStyle = Win32Api.GetWindowLong(hwnd, Win32Api.GWL_EXSTYLE);
        var newStyle = enable ? currentStyle | style : currentStyle & ~style;
        Win32Api.SetWindowLong(hwnd, Win32Api.GWL_EXSTYLE, newStyle);
    }

    private static void ApplyIcon(Window window, NotificationIcon icon)
    {
        try
        {
            switch (icon.Type.ToLower())
            {
                case "file":
                    if (File.Exists(icon.Path))
                    {
                        // For now, just set a placeholder
                        // Would set icon from file: {icon.Path}
                    }
                    break;
                case "emoji":
                    // Create a text-based icon using the emoji
                    // Would set emoji icon: {icon.Path}
                    break;
                case "systemicon":
                    // Would set system icon: {icon.SystemIconName}
                    break;
            }
        }
        catch
        {
            // Ignore icon errors
        }
    }
}

/// <summary>
/// Creates a notification icon configuration
/// </summary>
[Cmdlet(VerbsCommon.New, "WinUINotificationIcon")]
[OutputType(typeof(NotificationIcon))]
public class NewWinUINotificationIconCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Source { get; set; } = "";

    [Parameter]
    [ValidateSet("File", "Emoji", "SystemIcon", "Resource")]
    public string Type { get; set; } = "File";

    [Parameter]
    public int Index { get; set; } = 0;

    [Parameter]
    public int Size { get; set; } = 32;

    protected override void ProcessRecord()
    {
        var icon = new NotificationIcon
        {
            Path = Source,
            Type = Type,
            Index = Index,
            Size = Size
        };

        if (Type == "SystemIcon")
        {
            icon.SystemIconName = Source;
        }

        WriteObject(icon);
    }
}

/// <summary>
/// Creates a window configuration
/// </summary>
[Cmdlet(VerbsCommon.New, "WinUIWindowConfiguration")]
[OutputType(typeof(WindowConfiguration))]
public class NewWinUIWindowConfigurationCmdlet : Cmdlet
{
    [Parameter]
    public SwitchParameter HideMinimizeButton { get; set; }

    [Parameter]
    public SwitchParameter HideMaximizeButton { get; set; }

    [Parameter]
    public SwitchParameter HideCloseButton { get; set; }

    [Parameter]
    public SwitchParameter HideFromTaskbar { get; set; }

    [Parameter]
    public SwitchParameter SetTopmost { get; set; }

    [Parameter]
    public SwitchParameter DisableResize { get; set; }

    [Parameter]
    public int X { get; set; } = -1;

    [Parameter]
    public int Y { get; set; } = -1;

    [Parameter]
    public int Width { get; set; } = 400;

    [Parameter]
    public int Height { get; set; } = 200;

    [Parameter]
    public NotificationIcon? Icon { get; set; }

    protected override void ProcessRecord()
    {
        var config = new WindowConfiguration
        {
            HasMinimizeButton = !HideMinimizeButton,
            HasMaximizeButton = !HideMaximizeButton,
            HasCloseButton = !HideCloseButton,
            ShowInTaskbar = !HideFromTaskbar,
            IsTopmost = SetTopmost,
            IsResizable = !DisableResize,
            X = X,
            Y = Y,
            Width = Width,
            Height = Height,
            Icon = Icon
        };

        WriteObject(config);
    }
}
