using System.Management.Automation;

namespace WinUIShell.Cmdlets;

/// <summary>
/// Represents the result of a dialog interaction
/// </summary>
public class DialogResult
{
    public string ButtonClicked { get; set; } = "";
    public string ButtonResult { get; set; } = "";
    public int ButtonIndex { get; set; } = -1;
    public bool IsDefault { get; set; }
    public bool IsCancel { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.Now;
    public Dictionary<string, object> ControlValues { get; set; } = new();
    public bool WasCancelled { get; set; }
    public TimeSpan DisplayDuration { get; set; }
}

/// <summary>
/// Represents a control in a dialog for dynamic enumeration
/// </summary>
public class DialogControl
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public object? Value { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Enhanced button definition with more properties
/// </summary>
public class EnhancedDialogButton : DialogButton
{
    public string Id { get; set; } = "";
    public string ToolTip { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public string Icon { get; set; } = "";
    public int TabIndex { get; set; } = 0;
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Progress result with detailed information
/// </summary>
public class ProgressResult
{
    public bool WasCompleted { get; set; }
    public bool WasCancelled { get; set; }
    public int FinalPercentage { get; set; }
    public string FinalMessage { get; set; } = "";
    public TimeSpan Duration { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Exception? Error { get; set; }
}

/// <summary>
/// Enhanced progress controller with better result tracking
/// </summary>
public class EnhancedProgressController : ProgressController
{
    private readonly DateTime _startTime;
    private readonly ProgressResult _result;

    internal EnhancedProgressController(Window window, TextBlock messageBlock, ProgressBar? progressBar, ProgressRing? progressRing)
        : base(window, messageBlock, progressBar, progressRing)
    {
        _startTime = DateTime.Now;
        _result = new ProgressResult
        {
            StartTime = _startTime
        };
    }

    public ProgressResult GetResult()
    {
        _result.Duration = DateTime.Now - _startTime;
        _result.EndTime = DateTime.Now;
        return _result;
    }

    public new void Complete(string? finalMessage = null)
    {
        _result.WasCompleted = true;
        _result.FinalMessage = finalMessage ?? "";
        _result.FinalPercentage = 100;
        base.Complete(finalMessage);
    }

    public new void SetCancelled()
    {
        _result.WasCancelled = true;
        _result.EndTime = DateTime.Now;
        base.SetCancelled();
    }

    public void SetError(Exception error)
    {
        _result.Error = error;
        _result.EndTime = DateTime.Now;
    }
}

/// <summary>
/// Dialog manager for tracking and managing open dialogs
/// </summary>
public static class DialogManager
{
    private static readonly List<Window> _openDialogs = new();
    private static readonly Dictionary<Window, DialogResult> _dialogResults = new();

    public static void RegisterDialog(Window dialog, DialogResult result)
    {
        _openDialogs.Add(dialog);
        _dialogResults[dialog] = result;
    }

    public static void UnregisterDialog(Window dialog)
    {
        _openDialogs.Remove(dialog);
        _dialogResults.Remove(dialog);
    }

    public static DialogResult? GetDialogResult(Window dialog)
    {
        return _dialogResults.TryGetValue(dialog, out var result) ? result : null;
    }

    public static IReadOnlyList<Window> GetOpenDialogs()
    {
        return _openDialogs.AsReadOnly();
    }

    public static void CloseAllDialogs()
    {
        var dialogsToClose = _openDialogs.ToList();
        foreach (var dialog in dialogsToClose)
        {
            try
            {
                dialog.Close();
            }
            catch
            {
                // Ignore errors when closing
            }
        }
        _openDialogs.Clear();
        _dialogResults.Clear();
    }

    public static List<DialogControl> EnumerateControls(Window dialog)
    {
        var controls = new List<DialogControl>();

        // Simplified control enumeration - just return basic info
        if (dialog.Content != null)
        {
            var control = new DialogControl
            {
                Name = "DialogContent",
                Type = dialog.Content.GetType().Name,
                Value = dialog.Content.GetType().Name,
                IsEnabled = true,
                IsVisible = true
            };
            controls.Add(control);
        }

        return controls;
    }
}

/// <summary>
/// Event args for dialog events
/// </summary>
public class DialogEventArgs : EventArgs
{
    public DialogResult Result { get; set; } = new();
    public List<DialogControl> Controls { get; set; } = new();
    public Window Dialog { get; set; } = null!;
}
