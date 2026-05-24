using System;
using Avalonia.Controls;

namespace LedCube.Core.UI.Dialog.BroadcastSearchDialog;

public partial class BroadcastSearchDialogWindow : Window
{
    public BroadcastSearchDialogWindow()
    {
        InitializeComponent();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (DataContext is BroadcastSearchDialogViewModel vm)
            vm.LoadedCommand.Execute(this);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DataContext is BroadcastSearchDialogViewModel vm)
            vm.ClosedCommand.Execute(this);
    }
}
