using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Streamer.CubeStreamer;
using LedCube.Streamer.Datagram;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Controls.StreamingControl;

public partial class CubeStreamingStatusViewModel : ObservableObject, IDisposable
{
    private readonly ILogger _logger;
    private readonly ICubeStreamingStatus _cubeStreamingStatus;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FpsCurrent))]
    [NotifyPropertyChangedFor(nameof(FpsMean))]
    private CubeStreamingStatusSnapshot _streamingStatus = new CubeStreamingStatusSnapshot();
    
    public double FpsCurrent => 1 / StreamingStatus.FrameTimeCurrent.TotalSeconds;
    
    public double FpsMean => 1 / StreamingStatus.FrameTimeMean.TotalSeconds;
    
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
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Update(_cubeStreamingStatus);
                }, DispatcherPriority.Background);
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
        StreamingStatus = new CubeStreamingStatusSnapshot(
            CubeVersion: status.CubeVersion,
            StreamerVersion: status.StreamerVersion,
            Ping: status.Ping,
            PingMean: status.PingMean,
            FrameTimeCurrent: status.FrameTimeCurrent,
            FrameTimeMean: status.FrameTimeMean,
            FrameTime95Pct: status.FrameTime95Pct,
            FrameTime05Pct: status.FrameTime05Pct,
            FpsMean: 1 / StreamingStatus.FrameTimeCurrent.TotalSeconds,
            FpsCurrent: 1 / StreamingStatus.FrameTimeMean.TotalSeconds,
            FrameNumber: status.FrameNumber,
            AnimationStatus: status.AnimationStatus,
            CurrentAnimation: status.CurrentAnimation,
            CurrentTicks: status.CurrentTicks,
            CubeErrorCode: status.CubeErrorCode,
            ConnectionStable: status.ConnectionStable
        );
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _timer.Dispose();
        _updateTask.GetAwaiter().GetResult();
    }
}