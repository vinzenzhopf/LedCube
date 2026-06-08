using System;
using System.Collections.Generic;
using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LedCube.Plugins.Animation.LedWalker;

public class LedWalkerAnimation(IConfiguration configuration, ILogger<LedWalkerAnimation> logger)
    : IFrameGenerator
{
    private const double FrameTimeMs = 10.0;
    private const float DefaultDurationSeconds = 16.0f;

    // Frame count is derived generically from the duration (duration / frame time) and is therefore
    // independent of the cube size; the per-LED step is what scales with the cube (see Start).
    public static FrameGeneratorInfo Info => new("Led Walker Animation", "Walks one led through the cube.",
        FrameTime: TimeSpan.FromMilliseconds(FrameTimeMs),
        ConfigDescriptors: [DurationConfig.Descriptor(DefaultDurationSeconds)]);

    public TimeSpan? FrameTime => Info.FrameTime;

    private IEnumerator<Point3D>? _activeLedPos;
    private float _durationSeconds = DefaultDurationSeconds;
    private float _ledStepMs = (float) FrameTimeMs;
    private float _lastMove;

    public void Configure(AnimationConfig config)
        => _durationSeconds = DurationConfig.Read(config, _durationSeconds);

    public void Start(AnimationContext animationContext)
    {
        var size = animationContext.CubeData.Size;
        var totalLeds = Math.Max(1, size.X * size.Y * size.Z);
        // Spread the whole walk evenly across the configured duration; 0 = run until stopped, in
        // which case we fall back to stepping one LED per frame.
        _ledStepMs = _durationSeconds > 0f
            ? _durationSeconds * 1000f / totalLeds
            : (float) FrameTimeMs;
        _lastMove = (float) animationContext.ElapsedTimeUs / 1000.0f;
        animationContext.CubeData.Clear();
        _activeLedPos?.Dispose();
        _activeLedPos = new PositionGenerator3D(size, true).GetEnumerator();
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var elapsedTimeMs = (float) frameContext.ElapsedTimeUs / 1_000;

        // Step as many LEDs as the elapsed time allows so the walk fills the cube in exactly the
        // configured duration, independent of cube size or frame time.
        while (elapsedTimeMs - _lastMove > _ledStepMs)
        {
            if (_activeLedPos?.MoveNext() is not true)
            {
                _lastMove = elapsedTimeMs;
                break;
            }

            var pos = _activeLedPos.Current;
            frameContext.Buffer.SetLed(pos, !frameContext.Buffer.GetLed(pos));
            _lastMove += _ledStepMs;
        }

        return DurationConfig.IsFinished(frameContext, _durationSeconds)
            ? DrawingResult.Finished
            : DrawingResult.Continue;
    }

    public void End(AnimationContext animationContext)
    {
        _activeLedPos?.Dispose();
        _activeLedPos = null;
    }
}
