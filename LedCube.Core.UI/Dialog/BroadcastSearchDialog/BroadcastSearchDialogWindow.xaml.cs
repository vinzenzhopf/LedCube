using System;
using System.Windows;
using MahApps.Metro.Controls;

namespace LedCube.Core.UI.Dialog.BroadcastSearchDialog;

public partial class BroadcastSearchDialogWindow : MetroWindow
{
    public BroadcastSearchDialogWindow()
    {
        InitializeComponent();
    }

    private void BroadcastSearchDialogWindow_OnClosed(object? sender, EventArgs e)
    {
        if (DataContext is BroadcastSearchDialogViewModel vm)
        {
            vm.ClosedCommand.Execute(this);
        }
    }

    private void BroadcastSearchDialogWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is BroadcastSearchDialogViewModel vm)
        {
            vm.LoadedCommand.Execute(this);
        }
    }
}