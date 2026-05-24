using System.Threading.Tasks;

namespace LedCube.Core.UI.Dialog.SelectAnimationDialog;

public record OpenSelectAnimationDialogMessage()
{
    public SelectAnimationDialogResult? Result { get; set; }
    public TaskCompletionSource Completion { get; } = new();
};
