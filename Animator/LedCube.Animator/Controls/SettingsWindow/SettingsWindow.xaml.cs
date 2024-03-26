using System.ComponentModel;
using MahApps.Metro.Controls;

namespace LedCube.Animator.Controls.SettingsWindow;

public partial class SettingsWindow : MetroWindow
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsViewModel settingsViewModel)
    {
        _viewModel = settingsViewModel;
        DataContext = settingsViewModel;
        InitializeComponent();

        settingsViewModel.CloseAction = this.Close;
    }

    private void SettingsWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (!_viewModel.ClosingCommand.CanExecute(sender))
        {
            e.Cancel = true;
            return;
        }
        _viewModel.ClosingCommand.Execute(sender);
        e.Cancel = false;
    }
}