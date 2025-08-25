using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.Core.UI.Controls.CubeView2D;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Controls.StreamingControl;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Streamer.SmallUI.Controls.MenuBar;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LedCube.Streamer.SmallUI.Controls.MainWindow;

public partial class MainViewModel : ObservableObject
{
    private readonly ILogger _logger;
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
    public MainViewModel(
        ILoggerFactory loggerFactory,
        AppInfo appInfo, 
        LogAppenderViewModel logAppenderViewModel, 
        MenuBarViewModel menuBarViewModel, 
        CubeView2DViewModel cubeView2DViewModel, 
        AnimationListViewModel animationListViewModel, 
        PlaybackControlViewModel playbackControlViewModel,
        StreamingControlViewModel streamingControlViewModel
        )
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _appInfo = appInfo;
        LogAppenderViewModel = logAppenderViewModel;
        MenuBarViewModel = menuBarViewModel;
        CubeView2DViewModel = cubeView2DViewModel;
        AnimationListViewModel = animationListViewModel;
        PlaybackControlViewModel = playbackControlViewModel;
        StreamingControlViewModel = streamingControlViewModel;
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task Test(CancellationToken token)
    {
        // var message = new OpenSimpleDialogMessage(SimpleDialogButtons.YesNoCancel, "Test Text", "Test Title");
        // WeakReferenceMessenger.Default.Send(message);   
        // // Dispatcher.CurrentDispatcher.Invoke(() => { });
        //
        // _logger.LogInformation("Dialog Result {DialogResult}", message.Result);
        await Task.Run(async () =>
        {
            
        }, token);

    }
    
    [RelayCommand]
    private void BroadcastSearch()
    {
        var message = new OpenBroadcastSearchDialogMessage();
        WeakReferenceMessenger.Default.Send(message);   
        _logger.LogInformation("Dialog Result {DialogResult}: Destination: {destination}", message.DialogResult?.DialogResult, message.DialogResult?.HostAndPort);
    }
}