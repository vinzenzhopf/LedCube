using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common.CubeData.Repository;
using LedCube.Core.Common.Extensions;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginBase;
using LedCube.PluginHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Services.Playback;

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
    private ICubeData? _cubeData;
    private PeriodicTimer? _updateTimer;

    public TimeSpan FrameTime => _frameTime;
    private TimeSpan _frameTime = TimeSpan.Zero;
    public int? FrameCount => _frameCount;
    private int? _frameCount;
    private TimeSpan _lastFrameTime = TimeSpan.Zero;
    private long _elapsedTicksUntilPause = 0;
    private long _elapsedTicks = 0;
    private long _lastFrameTicks = 0;
    private long _frameTicks = 0;
    // Times the current entry has finished playing since playback (re)started. Compared against the
    // entry's live RepeatCount on each finish, so editing Repeat mid-playback takes effect immediately.
    private int _playCount = 0;

    [ObservableProperty]
    private PlaylistEntry? _currentEntry;

    [ObservableProperty]
    private PlaybackState _playbackState;

    [ObservableProperty]
    private int _currentFrame;

    [ObservableProperty]
    private TimeSpan _currentTime = TimeSpan.Zero;

    public async Task UpdateFrameGeneratorAsync(PlaylistEntry entry, CancellationToken token)
    {
        UnloadFrameGenerator();

        CurrentEntry = entry;
        PlaybackState = PlaybackState.Stopped;

        var frameGeneratorEntry = _pluginManager.AllFrameGeneratorInfos()
            .FirstOrDefault(x => x.TypeInfo == entry.TypeInfo);
        if (frameGeneratorEntry is null)
        {
            _logger.LogWarning("No FrameGeneratorEntry found for type {TypeInfo}", entry.TypeInfo);
            return;
        }

        _frameGenerator = _pluginManager.GetFrameGenerator(frameGeneratorEntry);
        _cubeData = new CubeData<CubeDataBuffer16>();
        _cubeRepository.SetCubeData(_cubeData);

        _frameGenerator.Configure(entry.Config);

        _frameTime = entry.FrameTimeOverride
            ?? _frameGenerator.FrameTime
            ?? TimeSpan.FromMilliseconds(1);
        OnPropertyChanged(nameof(FrameTime));
        _frameCount = _frameGenerator.FrameCount;
        OnPropertyChanged(nameof(FrameCount));
        _updateTimer = null;

        await _frameGenerator.InitializeAsync(token).ConfigureAwait(false);
    }

    private void UnloadFrameGenerator()
    {
        _elapsedTicksUntilPause = 0;
        _lastFrameTicks = 0;
        _frameTicks = 0;

        CurrentEntry = null;
        PlaybackState = PlaybackState.Stopped;
        _frameCount = null;
        OnPropertyChanged(nameof(FrameCount));

        (_frameGenerator as IDisposable)?.Dispose();
        _frameGenerator = null;
        _updateTimer = null;
    }

    public void SeekToFrame(int frame)
    {
        CurrentFrame = frame;
        CurrentTime = TimeSpan.FromTicks((long)(frame * _frameTime.Ticks));
        _elapsedTicksUntilPause = (long)(frame * _frameTime.TotalSeconds * Stopwatch.Frequency);
        if (PlaybackState == PlaybackState.Playing)
        {
            _stopwatch.Restart();
        }
    }

    public void StartPlayback()
    {
        PlaybackState = PlaybackState.Playing;
        CurrentFrame = 0;
        CurrentTime = TimeSpan.Zero;
        _elapsedTicksUntilPause = 0;
        _playCount = 0;
        _updateTimer = new PeriodicTimer(_frameTime);
        _stopwatch.Restart();

        if (_cubeData is null || _frameGenerator is null)
            return;

        var context = new AnimationContext(_frameTime, _stopwatch.ElapsedTicks + _elapsedTicksUntilPause, _cubeData);
        _frameGenerator.Start(context);
    }

    public void ContinuePlayback()
    {
        PlaybackState = PlaybackState.Playing;
        _updateTimer = new PeriodicTimer(_frameTime);
        _stopwatch.Restart();

        if (_cubeData is null || _frameGenerator is null)
            return;

        var context = new AnimationContext(_frameTime, _stopwatch.ElapsedTicks + _elapsedTicksUntilPause, _cubeData);
        _frameGenerator.Continue(context);
    }

    public void StopPlayback()
    {
        PlaybackState = PlaybackState.Stopped;
        CurrentFrame = 0;
        CurrentTime = TimeSpan.Zero;
        _updateTimer?.Dispose();
        _updateTimer = null;
        _stopwatch.Stop();
        _elapsedTicksUntilPause = 0;

        if (_cubeData is null || _frameGenerator is null)
            return;

        var context = new AnimationContext(_frameTime, _stopwatch.ElapsedTicks + _elapsedTicksUntilPause, _cubeData);
        _frameGenerator.End(context);
    }

    public void PausePlayback()
    {
        PlaybackState = PlaybackState.Paused;
        _updateTimer?.Dispose();
        _updateTimer = null;
        _stopwatch.Stop();
        _elapsedTicksUntilPause += _stopwatch.ElapsedTicks;

        if (_cubeData is null || _frameGenerator is null)
            return;

        var context = new AnimationContext(_frameTime, _stopwatch.ElapsedTicks + _elapsedTicksUntilPause, _cubeData);
        _frameGenerator.Pause(context);
    }

    private void HandleRepeat()
    {
        CurrentFrame = 0;
        CurrentTime = TimeSpan.Zero;
        _elapsedTicksUntilPause = 0;
        _lastFrameTicks = 0;
        _frameTicks = 0;
        _stopwatch.Restart();

        if (_cubeData is null || _frameGenerator is null)
            return;

        var context = new AnimationContext(_frameTime, 0, _cubeData);
        _frameGenerator.Start(context);
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
                    _lastFrameTime = _lastFrameTicks == 0
                        ? TimeSpan.Zero
                        : TimeSpan.FromMicroseconds(StopwatchUtil.TicksToMicroseconds(_frameTicks - _lastFrameTicks));

                    _elapsedTicks = _stopwatch.ElapsedTicks + _elapsedTicksUntilPause;
                    var context = new FrameContext(_frameTime, _lastFrameTime,
                        (ulong)StopwatchUtil.TicksToMicroseconds(_elapsedTicks), _cubeData);
                    var result = _frameGenerator.DrawFrame(context);
                    CurrentFrame++;
                    CurrentTime = TimeSpan.FromMicroseconds(StopwatchUtil.TicksToMicroseconds(_elapsedTicks));

                    if (result == DrawingResult.Finished)
                    {
                        _playCount++;
                        // RepeatCount is read live every finish: 0 = repeat forever, N = play N times.
                        var count = CurrentEntry?.RepeatCount ?? 1;
                        if (count == 0 || _playCount < count)
                        {
                            _logger.LogInformation("Animation finished, repeating (play {Play} of {Count}).",
                                _playCount + 1, count == 0 ? "∞" : count.ToString());
                            HandleRepeat();
                        }
                        else
                        {
                            _logger.LogInformation("Animation finished after {Count} play(s).", count);
                            StopPlayback();
                            WeakReferenceMessenger.Default.Send(new PlaybackFinishedMessage());
                        }
                    }

                    if (_updateTimer is not null && PlaybackState == PlaybackState.Playing)
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
