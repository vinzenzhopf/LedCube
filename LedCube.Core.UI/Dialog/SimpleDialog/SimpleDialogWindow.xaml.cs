using System;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace LedCube.Core.UI.Dialog.SimpleDialog;

public partial class SimpleDialogWindow : MetroWindow
{
    public SimpleDialogWindow()
    {
        InitializeComponent();
    }

    private void SimpleDialogWindow_OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is SimpleDialogViewModel vm)
        {
            vm.ClosedCommand.Execute(this);
        }
    }
}