using System;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.RandomToggle;

/// <summary>
/// Toggles a handful of random LEDs every frame, producing an evolving sparkle.
/// Ported from the cube16x RandomToggleAnimation. The cube state persists, so it slowly fills.
/// </summary>
public class RandomToggleAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Random Toggle",
        "Continuously toggles random LEDs on and off.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("TogglesPerFrame", "Toggles per frame", AnimationConfigType.Int,
                DefaultValue: 6, MinValue: 1, MaxValue: 64),
            DurationConfig.Descriptor(12.0f),
        ],
        FrameTime: TimeSpan.FromMilliseconds(16));

    public TimeSpan? FrameTime => Info.FrameTime;

    private readonly Random _random = new();
    private int _togglesPerFrame = 6;
    private float _durationSeconds = 12.0f;

    public void Configure(AnimationConfig config)
    {
        if (config.Get<int>("TogglesPerFrame") is { } toggles)
            _togglesPerFrame = Math.Max(1, toggles);
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext) => animationContext.CubeData.Clear();

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var cube = frameContext.Buffer;
        var size = cube.Size;
        for (var i = 0; i < _togglesPerFrame; i++)
        {
            var p = new Point3D(_random.Next(size.X), _random.Next(size.Y), _random.Next(size.Z));
            cube.SetLed(p, !cube.GetLed(p));
        }

        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }
}
