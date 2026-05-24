using System.Threading.Tasks;
using LedCube.Core.UI.Controls.PlaylistControl;
using LedCube.Core.UI.Dialog.BaseDialog;

namespace LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;

public record EditAnimationInstanceDialogMessage(
    PlaylistEntryControlViewModel Instance
)
{
    public DialogResult? Result { get; set; }
    public TaskCompletionSource Completion { get; } = new();
}
