using LedCube.Core.UI.Dialog.BaseDialog;
using AnimationViewModel = LedCube.Core.UI.Controls.AnimationInstanceList.AnimationViewModel;

namespace LedCube.Core.UI.Dialog.SelectAnimationDialog;

public record SelectAnimationDialogResult(
    bool? Result, 
    AnimationViewModel? Animation,
    string? InstanceName
) : DialogResult(Result);