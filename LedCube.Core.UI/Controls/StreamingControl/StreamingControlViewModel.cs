using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Controls.StreamingControl;

public partial class StreamingControlViewModel : ObservableObject
{
    private readonly ILogger _logger;
    public ICubeStreamer CubeStreamer { get; }
    public CubeStreamingStatusViewModel StreamingStatusViewModel { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyPropertyChangedFor(nameof(HostAndPort))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _host = "192.168.178.41";//string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyPropertyChangedFor(nameof(HostAndPort))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private int? _port = 4242; //null;

    public HostAndPort? HostAndPort => Port is not null ? new HostAndPort(Host, Port.Value) : null;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HostEditingEnabled))]
    private bool _isStreamActive;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(IsConnectingOrConnected))]
    [NotifyPropertyChangedFor(nameof(HostEditingEnabled))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private ConnectionState _connectionState;

    [ObservableProperty]
    private string _animationName = string.Empty;

    [ObservableProperty]
    private double _frameTimeMs = 5.0;
    
    public bool IsConnectionValid => !string.IsNullOrWhiteSpace(Host) && Port is > ushort.MinValue and < ushort.MaxValue;
    public bool IsConnected => ConnectionState is ConnectionState.Connected or ConnectionState.NotResponding;
    public bool IsConnectingOrConnected => ConnectionState is not ConnectionState.Disconnected;
    public bool HostEditingEnabled => ConnectionState is ConnectionState.Disconnected;

    public StreamingControlViewModel(
        ILoggerFactory loggerFactory,
        AppInfo appInfo,
        ICubeStreamer cubeStreamer,
        CubeStreamingStatusViewModel streamingStatusViewModel
    )
    {
        _logger = loggerFactory.CreateLogger(GetType());
        CubeStreamer = cubeStreamer;
        StreamingStatusViewModel = streamingStatusViewModel;
    }

    [RelayCommand]
    private async Task OpenBroadcastSearch()
    {
        var message = new OpenBroadcastSearchDialogMessage();
        WeakReferenceMessenger.Default.Send(message);
        await message.Completion.Task;
        _logger.LogInformation("Dialog Result {DialogResult}: Destination: {destination}", message.DialogResult?.DialogResult, message.DialogResult?.HostAndPort);
        if (message.DialogResult is {DialogResult: true, HostAndPort: not null})
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                Host = message.DialogResult.HostAndPort.Hostname;
                Port = message.DialogResult.HostAndPort.Port;
            });
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(IsConnectionValid))]
    private async Task ConnectAsync(CancellationToken token)
    {
        try
        {
            if (HostAndPort == null)
            {
                _logger.LogError("Cannot connect: HostAndPort invalid");
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() => { ConnectionState = ConnectionState.Connecting; },
                DispatcherPriority.Normal);
            var connected = await CubeStreamer.ConnectAsync(4242, IPAddress.Any, HostAndPort, token);
            await Dispatcher.UIThread.InvokeAsync(
                () => { ConnectionState = connected ? ConnectionState.Connected : ConnectionState.Disconnected; },
                DispatcherPriority.Normal);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Connect cancelled");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while connect");
        }
        finally
        {
            if (ConnectionState == ConnectionState.Connecting)
            {
                await Dispatcher.UIThread.InvokeAsync(
                    () => { ConnectionState = ConnectionState.Disconnected; },
                    DispatcherPriority.Normal);
            }
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task DisconnectAsync(CancellationToken token)
    {
        if (ConnectionState == ConnectionState.Connecting)
        {
            ConnectCommand.Cancel();
            return;
        }
        try{
            await CubeStreamer.DisconnectAsync(token);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ConnectionState = ConnectionState.Disconnected;
            }, DispatcherPriority.Normal);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while disconnect");
        }
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task StartStreamingAsync(CancellationToken token)
    {
        try
        {
            var frameTimeUs = (uint)(FrameTimeMs * 1000);
            await CubeStreamer.StartAnimationAsync(frameTimeUs, AnimationName, token);
            await CubeStreamer.StartStreaming(token);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsStreamActive = true;
            }, DispatcherPriority.Normal);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while start streaming");
        }
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task StopStreamingAsync(CancellationToken token)
    {
        try
        {
            await CubeStreamer.StopStreaming(token);
            await CubeStreamer.EndAnimationAsync(token);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                IsStreamActive = false;
            }, DispatcherPriority.Normal);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while stop streaming");
        }
    }
}