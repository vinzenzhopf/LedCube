using System;
using LedCube.Core.Common.Model;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.FallingLeds;

/// <summary>
/// Every column (x,y) holds exactly one lit LED at a height. One column at a time drifts to the
/// floor and settles; once all have drained the motion reverses and they rise back to the top.
/// Ported from the cube16x FallingLedsAnimation.
/// </summary>
public class FallingLedsAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("Falling LEDs", "A surface of LEDs that drains to the floor, then rises again.",
        FrameTime: TimeSpan.FromMilliseconds(16));

    public TimeSpan? FrameTime => Info.FrameTime;

    private const int StepsPerFrame = 8;

    private readonly Random _random = new();
    private int _sizeX, _sizeZ, _columns;
    private int[] _height = [];
    private int _current;
    private bool _falling;
    private int _settled;
    private bool _finished;

    public void Start(AnimationContext animationContext)
    {
        var cube = animationContext.CubeData;
        cube.Clear();
        var size = cube.Size;
        _sizeX = size.X;
        _sizeZ = size.Z;
        _columns = size.X * size.Y;

        _height = new int[_columns];
        var top = _sizeZ - 1;
        Array.Fill(_height, top);
        _falling = true;
        _settled = 0;
        _finished = false;
        _current = _random.Next(_columns);
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        for (var step = 0; step < StepsPerFrame && !_finished; step++)
        {
            Advance();
        }

        var cube = frameContext.Buffer;
        cube.Clear();
        for (var i = 0; i < _columns; i++)
        {
            cube.SetLed(new Point3D(i % _sizeX, i / _sizeX, _height[i]), true);
        }

        return _finished ? DrawingResult.Finished : DrawingResult.Continue;
    }

    private void Advance()
    {
        var target = _falling ? 0 : _sizeZ - 1;
        if (_height[_current] == target)
        {
            _current = _random.Next(_columns);
            return;
        }

        _height[_current] += _falling ? -1 : 1;
        if (_height[_current] == target)
        {
            _current = _random.Next(_columns);
            if (++_settled >= _columns)
            {
                _falling = !_falling;
                _settled = 0;
                if (_falling)
                {
                    _finished = true; // drained to the floor and risen back -> one full cycle
                }
            }
        }
    }
}
