using System;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.Raindrops;

/// <summary>
/// Spawns drops that fall from the top of the cube; when a drop reaches the bottom it wets the
/// floor at that column. Once the whole floor is wet it dries and starts over. Ported from the
/// cube16x RaindropsAnimation.
/// </summary>
public class RaindropsAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new(
        "Raindrops",
        "Drops fall from the top and gradually wet the floor.",
        ConfigDescriptors:
        [
            new AnimationConfigDescriptor("DropsPerFrame", "Drops per frame", AnimationConfigType.Int,
                DefaultValue: 6, MinValue: 1, MaxValue: 32),
            DurationConfig.Descriptor(20.0f),
        ],
        FrameTime: TimeSpan.FromMilliseconds(20));

    public TimeSpan? FrameTime => Info.FrameTime;

    private readonly Random _random = new();
    private int _sizeX, _sizeZ, _columns;
    private int[] _dropZ = [];
    private bool[] _floor = [];
    private int _dropsPerFrame = 6;
    private float _durationSeconds = 20.0f;

    public void Configure(AnimationConfig config)
    {
        if (config.Get<int>("DropsPerFrame") is { } drops)
            _dropsPerFrame = Math.Max(1, drops);
        _durationSeconds = DurationConfig.Read(config, _durationSeconds);
    }

    public void Start(AnimationContext animationContext)
    {
        var cube = animationContext.CubeData;
        cube.Clear();
        var size = cube.Size;
        _sizeX = size.X;
        _sizeZ = size.Z;
        _columns = size.X * size.Y;
        _dropZ = new int[_columns];
        _floor = new bool[_columns];
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        // Advance falling drops; a drop that reaches the floor wets it.
        for (var i = 0; i < _columns; i++)
        {
            if (_dropZ[i] <= 0)
            {
                continue;
            }

            if (--_dropZ[i] == 0)
            {
                _floor[i] = true;
            }
        }

        // Spawn new drops at the top of empty columns.
        for (var i = 0; i < _dropsPerFrame; i++)
        {
            var idx = _random.Next(_columns);
            if (_dropZ[idx] == 0)
            {
                _dropZ[idx] = _sizeZ - 1;
            }
        }

        var cube = frameContext.Buffer;
        cube.Clear();
        var allWet = true;
        for (var i = 0; i < _columns; i++)
        {
            var x = i % _sizeX;
            var y = i / _sizeX;
            if (_dropZ[i] > 0)
            {
                cube.SetLed(new Point3D(x, y, _dropZ[i]), true);
            }

            if (_floor[i])
            {
                cube.SetLed(new Point3D(x, y, 0), true);
            }
            else
            {
                allWet = false;
            }
        }

        if (allWet)
        {
            Array.Clear(_floor);
        }

        return DurationConfig.IsFinished(frameContext, _durationSeconds) ? DrawingResult.Finished : DrawingResult.Continue;
    }
}
