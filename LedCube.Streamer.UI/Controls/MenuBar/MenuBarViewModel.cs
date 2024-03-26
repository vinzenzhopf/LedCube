using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.UI.Controls.LogAppender;

namespace LedCube.Streamer.UI.Controls.MenuBar;

[ObservableObject]
public partial class MenuBarViewModel
{
    public LogAppenderViewModel AppenderViewModel { get; }

    public MenuBarViewModel(LogAppenderViewModel logAppenderViewModel)
    {
        AppenderViewModel = logAppenderViewModel;
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