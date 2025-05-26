using System.Management.Automation;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Progress controller for progress notifications
/// </summary>
public class ProgressController
{
    private readonly Window _window;
    private readonly TextBlock _messageBlock;
    private readonly ProgressBar? _progressBar;

    internal ProgressController(Window window, TextBlock messageBlock, ProgressBar? progressBar, ProgressRing? progressRing)
    {
        _window = window;
        _messageBlock = messageBlock;
        _progressBar = progressBar;
        // progressRing is used for indeterminate mode but we don't need to store it
        _ = progressRing;
    }

    public bool IsCancelled { get; private set; }

    public void UpdateProgress(int percentage, string? message = null)
    {
        if (_progressBar != null && percentage >= 0 && percentage <= 100)
        {
            _progressBar.Value = percentage;
        }

        if (!string.IsNullOrEmpty(message))
        {
            _messageBlock.Text = message;
        }
    }

    public void Complete(string? finalMessage = null)
    {
        if (!string.IsNullOrEmpty(finalMessage))
        {
            _messageBlock.Text = finalMessage;
        }

        if (_progressBar != null)
        {
            _progressBar.Value = 100;
        }

        // Brief delay to show completion
        Thread.Sleep(500);
        _window.Close();
    }

    public void Close()
    {
        _window.Close();
    }

    internal void SetCancelled()
    {
        IsCancelled = true;
    }
}

/// <summary>
/// Shows a progress notification window
/// </summary>
[Cmdlet(VerbsCommon.Show, "WinUIProgress")]
[OutputType(typeof(ProgressController))]
public class ShowWinUIProgressCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Title { get; set; } = "";

    [Parameter]
    public string Message { get; set; } = "Please wait...";

    [Parameter]
    public NotificationTheme? Theme { get; set; }

    [Parameter]
    public SwitchParameter Cancellable { get; set; }

    [Parameter]
    public SwitchParameter IndeterminateMode { get; set; }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        if (Theme == null)
        {
            Theme = NotificationThemeHelper.CreateDefaultTheme();
        }

        var progressWindow = new Window
        {
            ExtendsContentIntoTitleBar = true,
            SystemBackdrop = new MicaBackdrop()
        };
        progressWindow.AppWindow.ResizeClient(400, 160);

        // Create main layout
        var mainGrid = new Grid
        {
            Background = new SolidColorBrush(Theme.Background),
            Margin = new Thickness(24, 24, 24, 24)
        };

        // Define rows
        var titleRow = new RowDefinition { Height = GridLength.Auto };
        var messageRow = new RowDefinition { Height = GridLength.Auto };
        var progressRow = new RowDefinition { Height = GridLength.Auto };
        var buttonRow = new RowDefinition { Height = GridLength.Auto };

        mainGrid.RowDefinitions.Add(titleRow);
        mainGrid.RowDefinitions.Add(messageRow);
        mainGrid.RowDefinitions.Add(progressRow);
        mainGrid.RowDefinitions.Add(buttonRow);

        // Title
        var titleBlock = new TextBlock
        {
            Text = Title,
            FontWeight = new FontWeight(600),
            FontSize = 16,
            Foreground = new SolidColorBrush(Theme.Primary),
            Margin = new Thickness(0, 0, 0, 12),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(titleBlock, 0);

        // Message
        var messageBlock = new TextBlock
        {
            Text = Message,
            FontSize = 14,
            Foreground = new SolidColorBrush(Theme.OnSurface),
            Margin = new Thickness(0, 0, 0, 16),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        Grid.SetRow(messageBlock, 1);

        // Progress control
        ProgressBar? progressBar = null;
        ProgressRing? progressRing = null;

        if (IndeterminateMode)
        {
            progressRing = new ProgressRing
            {
                IsIndeterminate = true,
                Width = 40,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(progressRing, 2);
            mainGrid.Children.Add(progressRing);
        }
        else
        {
            progressBar = new ProgressBar
            {
                Value = 0,
                Maximum = 100,
                Height = 8,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Grid.SetRow(progressBar, 2);
            mainGrid.Children.Add(progressBar);
        }

        // Cancel button (if cancellable)
        var controller = new ProgressController(progressWindow, messageBlock, progressBar, progressRing);

        if (Cancellable)
        {
            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Padding = new Thickness(16, 8, 16, 8),
                Background = new SolidColorBrush(Theme.Surface),
                Foreground = new SolidColorBrush(Theme.OnSurface),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(cancelButton, 3);

            cancelButton.AddClick(new EventCallback
            {
                ScriptBlock = ScriptBlock.Create("$global:progressController.SetCancelled(); $global:progressWindow.Close()"),
                ArgumentList = null
            });

            mainGrid.Children.Add(cancelButton);
        }

        // Add elements to grid
        mainGrid.Children.Add(titleBlock);
        mainGrid.Children.Add(messageBlock);

        progressWindow.Content = mainGrid;
        progressWindow.Activate();

        WriteObject(controller);
    }
}

/// <summary>
/// Shows an enhanced progress notification with detailed result tracking
/// </summary>
[Cmdlet(VerbsCommon.Show, "WinUIEnhancedProgress")]
[OutputType(typeof(EnhancedProgressController))]
public class ShowWinUIEnhancedProgressCmdlet : Cmdlet
{
    [Parameter(Mandatory = true)]
    public string Title { get; set; } = "";

    [Parameter]
    public string Message { get; set; } = "Please wait...";

    [Parameter]
    public NotificationTheme? Theme { get; set; }

    [Parameter]
    public SwitchParameter Cancellable { get; set; }

    [Parameter]
    public SwitchParameter IndeterminateMode { get; set; }

    [Parameter]
    public int TimeoutSeconds { get; set; } = 0;

    [Parameter]
    public SwitchParameter AutoClose { get; set; } = true;

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    protected override void ProcessRecord()
    {
        if (Theme == null)
        {
            Theme = NotificationThemeHelper.CreateDefaultTheme();
        }

        var progressWindow = new Window
        {
            ExtendsContentIntoTitleBar = true,
            SystemBackdrop = new MicaBackdrop()
        };
        progressWindow.AppWindow.ResizeClient(400, 160);

        // Create main layout
        var mainGrid = new Grid
        {
            Background = new SolidColorBrush(Theme.Background),
            Margin = new Thickness(24, 24, 24, 24)
        };

        // Define rows
        var titleRow = new RowDefinition { Height = GridLength.Auto };
        var messageRow = new RowDefinition { Height = GridLength.Auto };
        var progressRow = new RowDefinition { Height = GridLength.Auto };
        var buttonRow = new RowDefinition { Height = GridLength.Auto };

        mainGrid.RowDefinitions.Add(titleRow);
        mainGrid.RowDefinitions.Add(messageRow);
        mainGrid.RowDefinitions.Add(progressRow);
        mainGrid.RowDefinitions.Add(buttonRow);

        // Title
        var titleBlock = new TextBlock
        {
            Text = Title,
            FontWeight = new FontWeight(600),
            FontSize = 16,
            Foreground = new SolidColorBrush(Theme.Primary),
            Margin = new Thickness(0, 0, 0, 12),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(titleBlock, 0);

        // Message
        var messageBlock = new TextBlock
        {
            Text = Message,
            FontSize = 14,
            Foreground = new SolidColorBrush(Theme.OnSurface),
            Margin = new Thickness(0, 0, 0, 16),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        Grid.SetRow(messageBlock, 1);

        // Progress control
        ProgressBar? progressBar = null;
        ProgressRing? progressRing = null;

        if (IndeterminateMode)
        {
            progressRing = new ProgressRing
            {
                IsIndeterminate = true,
                Width = 40,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(progressRing, 2);
            mainGrid.Children.Add(progressRing);
        }
        else
        {
            progressBar = new ProgressBar
            {
                Value = 0,
                Maximum = 100,
                Height = 8,
                Margin = new Thickness(0, 0, 0, 16),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Grid.SetRow(progressBar, 2);
            mainGrid.Children.Add(progressBar);
        }

        // Create enhanced controller
        var controller = new EnhancedProgressController(progressWindow, messageBlock, progressBar, progressRing);

        // Cancel button (if cancellable)
        if (Cancellable)
        {
            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 80,
                Padding = new Thickness(16, 8, 16, 8),
                Background = new SolidColorBrush(Theme.Surface),
                Foreground = new SolidColorBrush(Theme.OnSurface),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetRow(cancelButton, 3);

            // Would add click handler for cancel button

            mainGrid.Children.Add(cancelButton);
        }

        // Handle timeout (simplified)
        if (TimeoutSeconds > 0)
        {
            // Would implement timeout functionality
        }

        // Add elements to grid
        mainGrid.Children.Add(titleBlock);
        mainGrid.Children.Add(messageBlock);

        progressWindow.Content = mainGrid;
        progressWindow.Activate();

        WriteObject(controller);
    }
}
