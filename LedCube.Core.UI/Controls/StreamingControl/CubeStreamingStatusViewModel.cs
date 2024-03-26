using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.Datagram;
using Microsoft.Extensions.Logging;
using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

namespace LedCube.Core.UI.Controls.StreamingControl;

public partial class CubeStreamingStatusViewModel : ObservableObject, ICubeStreamingStatus, IDisposable
{
    private readonly ILogger _logger;
    private readonly ICubeStreamingStatus _cubeStreamingStatus;

    [ObservableProperty]
    private string _cubeVersion = string.Empty;
    [ObservableProperty]
    private string _streamerVersion = string.Empty;
    [ObservableProperty]
    private TimeSpan _ping;
    [ObservableProperty]
    private TimeSpan _pingMean;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FpsCurrent))]
    private TimeSpan _frameTimeCurrent;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FpsMean))]
    private TimeSpan _frameTimeMean;
    [ObservableProperty]
    private TimeSpan _frameTime95Pct;
    [ObservableProperty]
    private TimeSpan _frameTime05Pct;
    [ObservableProperty]
    private long _frameNumber;
    [ObservableProperty]
    private AnimationStatus _animationStatus;
    [ObservableProperty]
    private string _currentAnimation = string.Empty;
    [ObservableProperty]
    private uint _currentTicks;
    [ObservableProperty]
    private CubeErrorCode _cubeErrorCode;
    [ObservableProperty]
    private bool _connectionStable;

    public double FpsCurrent => 1 / FrameTimeCurrent.TotalSeconds;
    
    public double FpsMean => 1 / FrameTimeMean.TotalSeconds;
    
    private PeriodicTimer _timer;
    private CancellationTokenSource _cancellationTokenSource;
    private Task _updateTask;

    public CubeStreamingStatusViewModel(ILoggerFactory loggerFactory, ICubeStreamingStatus cubeStreamingStatus)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _cubeStreamingStatus = cubeStreamingStatus;
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        _cancellationTokenSource = new CancellationTokenSource();
        _updateTask = Task.Run(async () => await UpdateStreamingStatus(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
    }

    private async Task UpdateStreamingStatus(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync( () =>
                {
                    Update(_cubeStreamingStatus);
                }, DispatcherPriority.Background, token);
                await _timer.WaitForNextTickAsync(token);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "StreamingStatusUpdate Error");
            }
        }
    }

    public void Update(ICubeStreamingStatus status)
    {
        CubeVersion = status.CubeVersion;
        StreamerVersion = status.StreamerVersion;
        Ping = status.Ping;
        PingMean = status.PingMean;
        FrameTimeCurrent = status.FrameTimeCurrent;
        FrameTimeMean = status.FrameTimeMean;
        FrameTime95Pct = status.FrameTime95Pct;
        FrameTime05Pct = status.FrameTime05Pct;
        FrameNumber = status.FrameNumber;
        AnimationStatus = status.AnimationStatus;
        CurrentAnimation = status.CurrentAnimation;
        CurrentTicks = status.CurrentTicks;
        CubeErrorCode = status.CubeErrorCode;
        ConnectionStable = status.ConnectionStable;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _timer.Dispose();
        _updateTask.GetAwaiter().GetResult();
    }
}