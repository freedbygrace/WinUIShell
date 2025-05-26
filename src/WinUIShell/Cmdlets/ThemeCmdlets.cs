using System.Management.Automation;
using Microsoft.Win32;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Gets the current Windows system theme (Light/Dark)
/// </summary>
[Cmdlet(VerbsCommon.Get, "WinUISystemTheme")]
[OutputType(typeof(string))]
public class GetWinUISystemThemeCmdlet : Cmdlet
{
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (key?.GetValue("AppsUseLightTheme") is int value)
            {
                WriteObject(value == 0 ? "Dark" : "Light");
            }
            else
            {
                WriteObject("Light");
            }
        }
        catch
        {
            WriteObject("Light");
        }
    }
}

/// <summary>
/// Creates a notification theme configuration
/// </summary>
[Cmdlet(VerbsCommon.New, "WinUINotificationTheme")]
[OutputType(typeof(NotificationTheme))]
public class NewWinUINotificationThemeCmdlet : Cmdlet
{
    [Parameter]
    [ValidateSet("Auto", "Light", "Dark")]
    public ThemeMode ThemeMode { get; set; } = ThemeMode.Auto;

    [Parameter]
    public Color? AccentColor { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        string actualTheme;
        
        if (ThemeMode == ThemeMode.Auto)
        {
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
        }
        else
        {
            actualTheme = ThemeMode.ToString();
        }

        var theme = new NotificationTheme { Mode = actualTheme };

        if (actualTheme == "Dark")
        {
            theme.Background = Color.FromArgb(255, 32, 32, 32);  // Dark gray
            theme.Surface = Color.FromArgb(255, 48, 48, 48);     // Slightly lighter gray
            theme.OnSurface = Colors.White;
            theme.OnSurfaceVariant = Colors.LightGray;
            theme.Primary = AccentColor ?? Colors.SkyBlue;
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
            theme.Primary = AccentColor ?? Colors.DodgerBlue;
            theme.OnPrimary = Colors.White;
            theme.Success = Colors.Green;
            theme.Warning = Colors.DarkOrange;
            theme.Error = Colors.Crimson;
            theme.Info = Colors.RoyalBlue;
            theme.Border = Colors.LightGray;
        }

        WriteObject(theme);
    }
}
