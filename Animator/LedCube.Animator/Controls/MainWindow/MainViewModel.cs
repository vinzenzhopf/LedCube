using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Animator.Controls.LogAppender;
using LedCube.Animator.Controls.MenuBar;
using LedCube.Core;
using LedCube.Core.Common;

namespace LedCube.Animator.Controls.MainWindow;

public partial class MainViewModel : ObservableObject
{
    private readonly AppInfo _appInfo;
    public LogAppenderViewModel LogAppenderViewModel { get; }
    public MenuBarViewModel MenuBarViewModel { get; }

    public string TitleText => $"LedCube Animator - <CurrentAnimationFile.anim>";
    public string BuildInfo => $"Build: {_appInfo.Version}, {_appInfo.BuildDate:yy-MM-dd HH:mm zz}";
    public string BuildInfoDebug => _appInfo.DebugBuild ? ", Debug" : "";
    public MainViewModel(AppInfo appInfo, LogAppenderViewModel logAppenderViewModel, MenuBarViewModel menuBarViewModel)
    {
        _appInfo = appInfo;
        LogAppenderViewModel = logAppenderViewModel;
        MenuBarViewModel = menuBarViewModel;
    }
}