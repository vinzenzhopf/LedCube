using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LedCube.Streamer.SmallUI.Controls.MenuBar;

[ObservableObject]
public partial class MenuBarViewModel
{
    // public LogAppenderViewModel AppenderViewModel { get; }

    public MenuBarViewModel()
    {
        // AppenderViewModel = logAppenderViewModel;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        // WeakReferenceMessenger.Default.Send<OpenSettingsNavigationMessage>();
    }

    [RelayCommand]
    private void ExitApplication()
    {
        // WeakReferenceMessenger.Default.Send<ExitApplicationNavigationMessage>();
    }
}