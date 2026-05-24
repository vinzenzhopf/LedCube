using System;
using Avalonia.Controls;

namespace LedCube.Core.UI.Dialog.SimpleDialog;

public partial class SimpleDialogWindow : Window
{
    public SimpleDialogWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DataContext is SimpleDialogViewModel vm)
            vm.ClosedCommand.Execute(this);
    }
}
