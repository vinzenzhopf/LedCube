using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core;
using LedCube.Core.UI.Dialog.BroadcastSearchDialog;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.Datagram;
using LedCube.Streamer.UdpCom;
using Microsoft.Extensions.Logging;

namespace LedCube.Streamer.SmallUI.Controls.StreamingControl;

public partial class StreamingControlViewModel : ObservableObject
{
    private readonly ILogger _logger;
    public ICubeStreamer CubeStreamer { get; }
    public CubeStreamingStatusViewModel StreamingStatusViewModel { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyPropertyChangedFor(nameof(HostAndPort))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string _host = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyPropertyChangedFor(nameof(HostAndPort))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private int? _port = null;

    public HostAndPort? HostAndPort => Port is not null ? new HostAndPort(Host, Port.Value) : null;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HostEditingEnabled))]
    private bool _isStreamActive;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyPropertyChangedFor(nameof(IsConnected))]
    [NotifyPropertyChangedFor(nameof(HostEditingEnabled))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private ConnectionState _connectionState;

    [ObservableProperty]
    private string _animationName = string.Empty;

    [ObservableProperty]
    private double _frameTimeMs = 5.0;
    
    
    
    public bool IsConnectionValid => !string.IsNullOrWhiteSpace(Host) && Port is > ushort.MinValue and < ushort.MaxValue;
    public bool IsConnected => ConnectionState is ConnectionState.Connected or ConnectionState.NotResponding;
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
    private void OpenBroadcastSearch()
    {
        var message = new OpenBroadcastSearchDialogMessage();
        WeakReferenceMessenger.Default.Send(message);   
        _logger.LogInformation("Dialog Result {DialogResult}: Destination: {destination}", message.DialogResult?.DialogResult, message.DialogResult?.HostAndPort);
        if (message.DialogResult is {DialogResult: true, HostAndPort: not null})
        {
            Application.Current.Dispatcher.Invoke(() =>
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

            await Application.Current.Dispatcher.InvokeAsync(() => { ConnectionState = ConnectionState.Connecting; },
                DispatcherPriority.Normal, token);
            var connected = await CubeStreamer.ConnectAsync(4242, IPAddress.Any, HostAndPort, token);
            await Application.Current.Dispatcher.InvokeAsync(
                () => { ConnectionState = connected ? ConnectionState.Connected : ConnectionState.Disconnected; },
                DispatcherPriority.Normal, token);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while connect");
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task DisconnectAsync(CancellationToken token)
    {
        try{
            await CubeStreamer.DisconnectAsync(token);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ConnectionState = ConnectionState.Disconnected;
            }, DispatcherPriority.Normal, token);
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
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                IsStreamActive = true;
            }, DispatcherPriority.Normal, token);
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
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                IsStreamActive = false;
            }, DispatcherPriority.Normal, token);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while stop streaming");
        }
    }
}