using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.Common;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.Core.UI.Controls.CubeView2D;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Controls.StreamingControl;
using LedCube.Streamer.UI.Controls.MenuBar;

namespace LedCube.Streamer.UI.Controls.MainWindow;

public partial class MainViewModel : ObservableObject
{
    private readonly AppInfo _appInfo;
    public LogAppenderViewModel LogAppenderViewModel { get; }
    public MenuBarViewModel MenuBarViewModel { get; }
    public CubeView2DViewModel CubeView2DViewModel { get; }
    public AnimationListViewModel AnimationListViewModel { get; }
    public PlaybackControlViewModel PlaybackControlViewModel { get; }
    public StreamingControlViewModel StreamingControlViewModel { get; }
    public string TitleText => $"LedCube Animator - <CurrentAnimationFile.anim>";
    public string BuildInfo => $"Build: {_appInfo.Version}, {_appInfo.BuildDate:yy-MM-dd HH:mm zz}";
    public string BuildInfoDebug => _appInfo.DebugBuild ? ", Debug" : "";
    public MainViewModel(AppInfo appInfo, LogAppenderViewModel logAppenderViewModel, 
        MenuBarViewModel menuBarViewModel, CubeView2DViewModel cubeView2DViewModel, 
        AnimationListViewModel animationListViewModel, PlaybackControlViewModel playbackControlViewModel,
        StreamingControlViewModel streamingControlViewModel)
    {
        _appInfo = appInfo;
        LogAppenderViewModel = logAppenderViewModel;
        MenuBarViewModel = menuBarViewModel;
        CubeView2DViewModel = cubeView2DViewModel;
        AnimationListViewModel = animationListViewModel;
        PlaybackControlViewModel = playbackControlViewModel;
        StreamingControlViewModel = streamingControlViewModel;
    }
        
    [RelayCommand]
    private void Test()
    {
            
        Dispatcher.CurrentDispatcher.Invoke(() => { });
    }
}