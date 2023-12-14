using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Streamer.Datagram;
using LedCube.Streamer.UdpCom;

namespace LedCube.Core.UI.Controls.StreamingControl;

[ObservableObject]
public partial class StreamingControlViewModel
{
    [ObservableProperty]
    private ObservableCollection<HostAndPort> _connections = new();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private HostAndPort? _selectedConnection;
    
    [ObservableProperty]
    private bool _isStreamActive;

    [ObservableProperty]
    private ushort _frameCount;

    [ObservableProperty]
    private TimeSpan _meanFrameTime;

    [ObservableProperty]
    private double _meanFrequency;

    [ObservableProperty]
    private TimeSpan _ping;

    [ObservableProperty]
    private TimeSpan _latency;
    
    [ObservableProperty]
    private string _cubeVersion;
    
    [ObservableProperty]
    private AnimationStatus _cubeStatus;

    [ObservableProperty]
    private CubeErrorCode _cubeErrorCode;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectionValid))]
    [NotifyPropertyChangedFor(nameof(IsConnectionSelectable))]
    private ConnectionState _connectionState;
    
    public bool IsConnectionValid => SelectedConnection is not null;
    public bool IsConnected => ConnectionState is ConnectionState.Connected or ConnectionState.OutOfSync;
    public bool IsConnectionSelectable => !IsConnected;

    public StreamingControlViewModel()
    {
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task SearchDevicesAsync(CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), token);
        await Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Connections.Clear();
            Connections.Add(new HostAndPort("10.10.10.42", 42));
            Connections.Add(new HostAndPort("127.0.0.1", 1337));
            SelectedConnection = Enumerable.First<HostAndPort>(Connections);
        });    
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(IsConnectionValid))]
    private async Task ConnectAsync(CancellationToken token)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ConnectionState = ConnectionState.Connecting;
        }, DispatcherPriority.Normal, token);
        await Task.Delay(TimeSpan.FromSeconds(2), token);
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ConnectionState = ConnectionState.Connected;
        }, DispatcherPriority.Normal, token);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task DisconnectAsync(CancellationToken token)
    {
        
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task StartStreamingAsync(CancellationToken token)
    {
        
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task StopStreamingAsync(CancellationToken token)
    {
        
    }
}

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    OutOfSync
}