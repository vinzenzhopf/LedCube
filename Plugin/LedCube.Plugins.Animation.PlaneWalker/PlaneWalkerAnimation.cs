using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.PlaneWalker;

/// <summary>
/// Sweeps a single lit plane through the cube along Z, Y and X (each forward then backward).
/// One full pass over all six directions is one cycle, after which the animation finishes so a
/// playlist can advance (use the entry's repeat count to play several cycles). Ported from the
/// cube16x PlaneWalkerAnimation2.
/// </summary>
public class PlaneWalkerAnimation : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("Plane Walker", "Sweeps a lit plane along each axis, back and forth.",
        FrameTime: TimeSpan.FromMilliseconds(60),
        // One cycle sweeps Z, Y and X each forward and back: 2*(X+Y+Z) plane positions.
        EstimateFrameCount: (cube, _) => (cube.Size.X + cube.Size.Y + cube.Size.Z) * 2);

    public TimeSpan? FrameTime => Info.FrameTime;

    private int _state;
    private int _index;

    public void Start(AnimationContext animationContext)
    {
        _state = 0;
        _index = 0;
        animationContext.CubeData.Clear();
    }

    public DrawingResult DrawFrame(FrameContext frameContext)
    {
        var cube = frameContext.Buffer;
        cube.Clear();

        var (axis, reverse) = StateToSweep(_state);
        var extent = AxisExtent(cube.Size, axis);
        DrawPlane(cube, axis, reverse ? extent - 1 - _index : _index);

        var finished = false;
        if (++_index >= extent)
        {
            _index = 0;
            if (++_state >= 6)
            {
                _state = 0;
                finished = true; // completed one full Z/Y/X back-and-forth sweep
            }
        }

        return finished ? DrawingResult.Finished : DrawingResult.Continue;
    }

    private static (int Axis, bool Reverse) StateToSweep(int state) => state switch
    {
        0 => (2, false), // Z, bottom -> top
        1 => (2, true),  // Z, top -> bottom
        2 => (1, false), // Y
        3 => (1, true),
        4 => (0, false), // X
        _ => (0, true),
    };

    private static int AxisExtent(Point3D size, int axis) => axis switch
    {
        0 => size.X,
        1 => size.Y,
        _ => size.Z,
    };

    private static void DrawPlane(ICubeData cube, int axis, int index)
    {
        var size = cube.Size;
        switch (axis)
        {
            case 0:
                for (var z = 0; z < size.Z; z++)
                for (var y = 0; y < size.Y; y++)
                    cube.SetLed(new Point3D(index, y, z), true);
                break;
            case 1:
                for (var z = 0; z < size.Z; z++)
                for (var x = 0; x < size.X; x++)
                    cube.SetLed(new Point3D(x, index, z), true);
                break;
            default:
                for (var y = 0; y < size.Y; y++)
                for (var x = 0; x < size.X; x++)
                    cube.SetLed(new Point3D(x, y, index), true);
                break;
        }
    }
}
