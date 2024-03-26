using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Core.Common.Extensions;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.PluginBase;
using LedCube.PluginHost;
using LedCube.Streamer.CubeStreamer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Services;

[ObservableObject]
public partial class PlaybackService : BackgroundService, IPlaybackService
{
    private readonly ILogger _logger;
    private readonly IPluginManager _pluginManager;
    private readonly ICubeRepository _cubeRepository;

    public PlaybackService(
        ILogger<PlaybackService> logger, 
        IPluginManager pluginManager, 
        ICubeRepository cubeRepository)
    {
        _logger = logger;
        _pluginManager = pluginManager;
        _cubeRepository = cubeRepository;
    }

    private readonly Stopwatch _stopwatch = new Stopwatch();
    private IFrameGenerator? _frameGenerator;
    private CubeData? _cubeData;
    private PeriodicTimer? _updateTimer;

    private TimeSpan _frameTime = TimeSpan.Zero;
    private TimeSpan _lastFrameTime = TimeSpan.Zero;
    private long _elapsedTicksUntilPause = 0;
    private long _elapsedTicks = 0;
    private long _lastFrameTicks = 0;
    private long _frameTicks = 0;
    
    [ObservableProperty]
    private FrameGeneratorEntry? _frameGeneratorEntry;

    [ObservableProperty]
    private AnimationViewModel? _animation;
    
    [ObservableProperty]
    private PlaybackState _playbackState;

    [ObservableProperty]
    private int _currentFrame;

    [ObservableProperty]
    private TimeSpan _currentTime = TimeSpan.Zero;

    public void UpdateFrameGenerator(FrameGeneratorEntry entry, AnimationViewModel animation)
    {
        FrameGeneratorEntry = entry;
        PlaybackState = PlaybackState.Stopped;
        
        _elapsedTicksUntilPause = 0;
        _lastFrameTicks = 0;
        _frameTicks = 0;
        
        _frameGenerator = _pluginManager.GetFrameGenerator(FrameGeneratorEntry);
        _cubeData = new CubeData(new Point3D(16, 16, 16));
        _cubeRepository.SetCubeData(_cubeData);

        _frameTime = _frameGenerator.FrameTime ?? TimeSpan.FromMilliseconds(1);
        _updateTimer = null;
    }

    public void StartPlayback()
    {
        PlaybackState = PlaybackState.Playing;
        _elapsedTicksUntilPause = 0;
        _updateTimer = new PeriodicTimer(_frameTime);
        _stopwatch.Restart();
    }

    public void ContinuePlayback()
    {
        PlaybackState = PlaybackState.Playing;
        _updateTimer = new PeriodicTimer(_frameTime);
        _stopwatch.Restart();
    }
    
    public void StopPlayback()
    {
        PlaybackState = PlaybackState.Stopped;
        _updateTimer?.Dispose();
        _updateTimer = null;
        _stopwatch.Stop();
        _elapsedTicksUntilPause = 0;
    }

    public void PausePlayback()
    {
        PlaybackState = PlaybackState.Paused;
        _updateTimer?.Dispose();
        _updateTimer = null;
        _stopwatch.Stop();
        _elapsedTicksUntilPause += _stopwatch.ElapsedTicks;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_frameGenerator is null || 
                    _updateTimer is null || 
                    _cubeData is null ||
                    PlaybackState != PlaybackState.Playing)
                {
                    await Task.Delay(10, stoppingToken);
                }
                else
                {
                    _lastFrameTicks = _frameTicks;
                    _frameTicks = _stopwatch.ElapsedTicks;
                    if (_lastFrameTicks == 0)
                    {
                        _lastFrameTime = TimeSpan.Zero;
                    }
                    else
                    {
                        _lastFrameTime = TimeSpan.FromMicroseconds(
                                StopwatchUtil.TicksToMicroseconds(_frameTicks - _lastFrameTicks));
                    }
                    _elapsedTicks = _stopwatch.ElapsedTicks + _elapsedTicksUntilPause;
                    var context = new FrameContext(_frameTime, _lastFrameTime, 
                        StopwatchUtil.TicksToMicroseconds(_elapsedTicks), _cubeData);
                    _frameGenerator.DrawFrame(context);
                    await _updateTimer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception in Update Loop");
            }
        }
    }

}