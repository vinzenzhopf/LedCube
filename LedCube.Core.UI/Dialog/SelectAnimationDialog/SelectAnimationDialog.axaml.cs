using System;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.BaseDialog;

namespace LedCube.Core.UI.Dialog.SelectAnimationDialog;

public partial class SelectAnimationDialog : Window, IRecipient<CloseDialogMessage>
{
    public SelectAnimationDialog()
    {
        WeakReferenceMessenger.Default.Register(this);
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is SelectAnimationDialogViewModel vm)
            vm.LoadedCommand.Execute(null);
    }

    protected override void OnClosed(EventArgs e)
    {
        WeakReferenceMessenger.Default.Unregister<CloseDialogMessage>(this);
        base.OnClosed(e);
        if (DataContext is SelectAnimationDialogViewModel vm)
            vm.ClosedCommand.Execute(this);
    }

    public void Receive(CloseDialogMessage message)
    {
        if (message.Name != nameof(SelectAnimationDialogViewModel)) return;
        Close();
    }
}
