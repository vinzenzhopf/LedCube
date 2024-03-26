using LedCube.Core.UI.Dialog.BaseDialog;
using AnimationInstanceViewModel = LedCube.Core.UI.Controls.AnimationInstanceList.AnimationInstanceViewModel;

namespace LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;

public record EditAnimationInstanceDialogMessage(
    AnimationInstanceViewModel Instance
)
{
    public DialogResult? Result { get; set; }
}
