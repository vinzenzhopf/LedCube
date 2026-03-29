using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Messages;

namespace LedCube.Streamer.UI.Controls.MenuBar;

public partial class MenuBarViewModel : ObservableObject
{
    public LogAppenderViewModel AppenderViewModel { get; }

    public MenuBarViewModel(LogAppenderViewModel logAppenderViewModel)
    {
        AppenderViewModel = logAppenderViewModel;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        WeakReferenceMessenger.Default.Send<OpenSettingsNavigationMessage>();
    }

    [RelayCommand]
    private void ExitApplication()
    {
        WeakReferenceMessenger.Default.Send<ExitApplicationNavigationMessage>();
    }
}
