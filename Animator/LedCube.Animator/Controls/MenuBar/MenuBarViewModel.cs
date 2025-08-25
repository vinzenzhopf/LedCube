using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Animator.Controls.LogAppender;
using LedCube.Animator.Messages;

namespace LedCube.Animator.Controls.MenuBar;

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