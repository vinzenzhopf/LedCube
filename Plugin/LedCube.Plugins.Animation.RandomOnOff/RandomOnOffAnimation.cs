using System;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.RandomOnOff;

/// <summary>
/// Alternates between a phase that switches random LEDs on and a phase that switches them off,
/// so the cube fills up and empties repeatedly. Ported from the cube16x RandomOnOffAnimation.
/// </summary>
public class RandomOnOffAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("Random On/Off", "Randomly fills the cube, then randomly clears it, on repeat.");

    public TimeSpan? FrameTime { get; } = TimeSpan.FromMilliseconds(16);

    private const int LedsPerFrame = 30;
    private const int PhaseFrames = 220;

    private readonly Random _random = new();
    private bool _turnOff;
    private int _frame;

    public void Start(AnimationContext animationContext)
    {
        animationContext.CubeData.Clear();
        _turnOff = false;
        _frame = 0;
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var cube = frameContext.Buffer;
        var size = cube.Size;
        for (var i = 0; i < LedsPerFrame; i++)
        {
            var p = new Point3D(_random.Next(size.X), _random.Next(size.Y), _random.Next(size.Z));
            cube.SetLed(p, !_turnOff);
        }

        var finished = false;
        if (++_frame >= PhaseFrames)
        {
            _frame = 0;
            _turnOff = !_turnOff;
            if (!_turnOff)
            {
                finished = true; // filled then cleared -> one full cycle
            }
        }

        return finished ? DrawingResult.Finished : DrawingResult.Continue;
    }
}
