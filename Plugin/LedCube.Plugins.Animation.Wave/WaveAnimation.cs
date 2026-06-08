using System;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.Wave;

/// <summary>
/// A wave surface that travels across the cube. Every column (x,y) is lit from the floor up to the
/// wave height at that point, so the cube looks like sloshing water. Planned in the cube16x roadmap.
/// </summary>
public class WaveAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Wave",
        "A travelling wave; LEDs below the surface are lit.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("Speed", "Speed", AnimationConfigType.Float,
                DefaultValue: 2.0f, MinValue: 0.1f, MaxValue: 10.0f),
            new AnimationConfigDescriptor("Waves", "Wave count", AnimationConfigType.Float,
                DefaultValue: 1.5f, MinValue: 0.5f, MaxValue: 6.0f),
            DurationConfig.Descriptor(15.0f),
        ],
        FrameTime: TimeSpan.FromMilliseconds(30));

    public TimeSpan? FrameTime => Info.FrameTime;

    private float _speed = 2.0f;
    private float _waves = 1.5f;
    private float _durationSeconds = 15.0f;

    public void Configure(AnimationConfig config)
    {
        if (config.Get<float>("Speed") is { } speed)
            _speed = speed;
        if (config.Get<float>("Waves") is { } waves)
            _waves = waves;
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext) => animationContext.CubeData.Clear();

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var cube = frameContext.Buffer;
        var size = cube.Size;
        var time = (float)frameContext.ElapsedTimeUs / 1_000_000f;

        var mid = (size.Z - 1) / 2f;
        var amplitude = (size.Z - 1) / 2f - 0.5f;

        cube.Clear();
        for (var y = 0; y < size.Y; y++)
        for (var x = 0; x < size.X; x++)
        {
            // A wave travelling along +X, with a gentle ripple along Y.
            var phase = (x / (float)size.X) * MathF.Tau * _waves
                        + (y / (float)size.Y) * MathF.Tau * (_waves * 0.5f)
                        - time * _speed;
            var height = (int)MathF.Round(mid + amplitude * MathF.Sin(phase));

            for (var z = 0; z <= height && z < size.Z; z++)
            {
                cube.SetLed(new Point3D(x, y, z), true);
            }
        }

        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }
}
