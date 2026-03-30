using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.BaseDialog;
using MahApps.Metro.Controls;

namespace LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;

public partial class EditAnimationInstanceDialog : MetroWindow, IRecipient<CloseDialogMessage>
{
    public EditAnimationInstanceDialog()
    {
        WeakReferenceMessenger.Default.Register(this);
        InitializeComponent();
    }

    public void Receive(CloseDialogMessage message)
    {
        if (message.Name != nameof(EditAnimationInstanceDialogViewModel)) return;
        Close();
    }
}
