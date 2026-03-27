using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Controls.StreamingControl;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Streamer.UI.Controls.MenuBar;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LedCube.Streamer.UI.Controls.MainWindow;

[ObservableObject]
public partial class MainViewModel
{
    private readonly ILogger _logger;
    private readonly AppInfo _appInfo;
    public LogAppenderViewModel LogAppenderViewModel { get; }
    public MenuBarViewModel MenuBarViewModel { get; }
    public AnimationListViewModel AnimationListViewModel { get; }
    public PlaybackControlViewModel PlaybackControlViewModel { get; }
    public StreamingControlViewModel StreamingControlViewModel { get; }
    public string TitleText => "LedCube Streamer";
    public string BuildInfo => $"Build: {_appInfo.Version}, {_appInfo.BuildDate:yy-MM-dd HH:mm zz}";
    public string BuildInfoDebug => _appInfo.DebugBuild ? ", Debug" : "";

    public MainViewModel(
        ILoggerFactory loggerFactory,
        AppInfo appInfo,
        LogAppenderViewModel logAppenderViewModel,
        MenuBarViewModel menuBarViewModel,
        AnimationListViewModel animationListViewModel,
        PlaybackControlViewModel playbackControlViewModel,
        StreamingControlViewModel streamingControlViewModel)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _appInfo = appInfo;
        LogAppenderViewModel = logAppenderViewModel;
        MenuBarViewModel = menuBarViewModel;
        AnimationListViewModel = animationListViewModel;
        PlaybackControlViewModel = playbackControlViewModel;
        StreamingControlViewModel = streamingControlViewModel;
    }

    [RelayCommand]
    private void BroadcastSearch()
    {
        var message = new OpenBroadcastSearchDialogMessage();
        WeakReferenceMessenger.Default.Send(message);
        _logger.LogInformation("Dialog Result {DialogResult}: Destination: {destination}", message.DialogResult?.DialogResult, message.DialogResult?.HostAndPort);
    }
}
