using System.Threading.Tasks;

namespace LedCube.Core.UI.Dialog.BroadcastSearchDialog;

public record OpenBroadcastSearchDialogMessage
{
    public BroadcastSearchDialogResult? DialogResult { get; set; }
    public TaskCompletionSource Completion { get; } = new();
}