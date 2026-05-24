using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.BaseDialog;

namespace LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;

public partial class EditAnimationInstanceDialog : Window, IRecipient<CloseDialogMessage>
{
    public EditAnimationInstanceDialog()
    {
        WeakReferenceMessenger.Default.Register(this);
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is EditAnimationInstanceDialogViewModel vm)
            vm.LoadedCommand.Execute(null);
    }

    protected override void OnClosed(EventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<CloseDialogMessage>(this);
        base.OnClosed(e);
    }

    public void Receive(CloseDialogMessage message)
    {
        if (message.Name != nameof(EditAnimationInstanceDialogViewModel)) return;
        Close();
    }
}
