using System.Threading.Tasks;

namespace LedCube.Core.UI.Dialog.SimpleDialog;

public record OpenSimpleDialogMessage(
    SimpleDialogButtons Buttons,
    string Text,
    string Title,
    bool CancelIsPrimary = false)
{
    public DialogResult Result { get; set; } = default;
    public TaskCompletionSource Completion { get; } = new();
}

public enum SimpleDialogButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel
}

public enum DialogResult
{
    None,
    Ok,
    Yes,
    No,
    Cancel
}