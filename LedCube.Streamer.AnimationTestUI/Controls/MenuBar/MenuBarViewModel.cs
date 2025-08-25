using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LedCube.Streamer.AnimationTestUI.Controls.MenuBar;

public partial class MenuBarViewModel : ObservableObject
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