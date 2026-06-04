using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.FullOn;

/// <summary>
/// Lights every LED of the cube for a configurable duration. Ported from the cube16x FullOnAnimation.
/// </summary>
public class FullOnAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Full On",
        "Turns every LED of the cube on.",
        ConfigDescriptors: [DurationConfig.Descriptor(6.0f)]);

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(100);

    private float _durationSeconds = 6.0f;

    public void Configure(AnimationConfig config) => _durationSeconds = DurationConfig.Read(config, _durationSeconds);

    public void Start(AnimationContext animationContext) => Fill(animationContext.CubeData);

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        Fill(frameContext.Buffer);
        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }

    private static void Fill(ICubeData cube)
    {
        var size = cube.Size;
        for (var z = 0; z < size.Z; z++)
        for (var y = 0; y < size.Y; y++)
        for (var x = 0; x < size.X; x++)
        {
            cube.SetLed(new Point3D(x, y, z), true);
        }
    }
}
