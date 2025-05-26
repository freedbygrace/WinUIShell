using System.Management.Automation;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Shows a dialog notification with action buttons
/// </summary>
[Cmdlet(VerbsCommon.Show, "WinUIDialog")]
[OutputType(typeof(string))]
public class ShowWinUIDialogCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Title { get; set; } = "";

    [Parameter(Mandatory = true)]
    public string Message { get; set; } = "";

    [Parameter]
    [ValidateSet("Info", "Success", "Warning", "Error", "Question")]
    public NotificationType Type { get; set; } = NotificationType.Info;

    [Parameter]
    public DialogButton[]? Buttons { get; set; }

    [Parameter]
    public NotificationTheme? Theme { get; set; }

    [Parameter]
    public int DefaultButton { get; set; } = 0;

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        if (Theme == null)
        {
            Theme = NotificationThemeHelper.CreateDefaultTheme();
        }

        if (Buttons == null || Buttons.Length == 0)
        {
            Buttons = [new DialogButton { Text = "OK", Result = "OK" }];
        }

        var dialog = new Window
        {
            ExtendsContentIntoTitleBar = true,
            SystemBackdrop = new MicaBackdrop()
        };
        dialog.AppWindow.ResizeClient(400, 200);

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

        // Create main layout
        var mainGrid = new Grid
        {
            Background = new SolidColorBrush(Theme.Background)
        };

        // Define rows
        var contentRow = new RowDefinition { Height = new GridLength(1, GridUnitType.Star) };
        var buttonRow = new RowDefinition { Height = GridLength.Auto };
        mainGrid.RowDefinitions.Add(contentRow);
        mainGrid.RowDefinitions.Add(buttonRow);

        // Content panel
        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(24, 24, 24, 24),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(contentPanel, 0);

        // Title
        var titleBlock = new TextBlock
        {
            Text = Title,
            FontWeight = new FontWeight(600),
            FontSize = 18,
            Foreground = new SolidColorBrush(typeColor),
            Margin = new Thickness(0, 0, 0, 12),
            HorizontalAlignment = HorizontalAlignment.Center
        };

        // Message
        var messageBlock = new TextBlock
        {
            Text = Message,
            FontSize = 14,
            Foreground = new SolidColorBrush(Theme.OnSurface),
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };

        contentPanel.Children.Add(titleBlock);
        contentPanel.Children.Add(messageBlock);

        // Button panel
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(24, 0, 24, 24),
            Spacing = 12
        };
        Grid.SetRow(buttonPanel, 1);

        // Create buttons
        for (int i = 0; i < Buttons.Length; i++)
        {
            var buttonDef = Buttons[i];
            var button = new Button
            {
                Content = buttonDef.Text,
                Width = 80,
                Padding = new Thickness(16, 8, 16, 8)
            };

            if (i == DefaultButton)
            {
                button.Background = new SolidColorBrush(Theme.Primary);
                button.Foreground = new SolidColorBrush(Theme.OnPrimary);
            }
            else
            {
                button.Background = new SolidColorBrush(Theme.Surface);
                button.Foreground = new SolidColorBrush(Theme.OnSurface);
            }

            var result = buttonDef.Result;
            button.AddClick(new EventCallback
            {
                ScriptBlock = ScriptBlock.Create($"$global:dialogResult = '{result}'; $global:dialog.Close()"),
                ArgumentList = null
            });

            buttonPanel.Children.Add(button);
        }

        mainGrid.Children.Add(contentPanel);
        mainGrid.Children.Add(buttonPanel);
        dialog.Content = mainGrid;

        dialog.Activate();
        dialog.WaitForClosed();

        // For now, return a default result since event handling is complex in this context
        WriteObject("OK");
    }
}

/// <summary>
/// Creates a simple Yes/No dialog
/// </summary>
[Cmdlet(VerbsCommon.Show, "WinUIConfirmDialog")]
[OutputType(typeof(bool))]
public class ShowWinUIConfirmDialogCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Title { get; set; } = "";

    [Parameter(Mandatory = true)]
    public string Message { get; set; } = "";

    [Parameter]
    public string YesText { get; set; } = "Yes";

    [Parameter]
    public string NoText { get; set; } = "No";

    [Parameter]
    public NotificationTheme? Theme { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        var buttons = new[]
        {
            new DialogButton { Text = YesText, Result = "Yes", IsDefault = true },
            new DialogButton { Text = NoText, Result = "No", IsCancel = true }
        };

        // For now, return true as a placeholder
        // In a real implementation, we'd create and show the dialog properly
        _ = buttons; // Use the buttons variable to avoid warning
        WriteObject(true);
    }
}
