using LedCube.Core.Common.Config.Entities;

namespace LedCube.Core.Common.Model.Orientation;

/// <summary>
/// Builds <see cref="CubeOrientation"/> rotations from the friendly enums. "Bring face to front"
/// rotates the cube so the chosen face points toward the viewer (display -Z), keeping a sensible up.
/// </summary>
public static class OrientationPresets
{
    /// <summary>Rotation that turns the given cube face toward the front (-Z), keeping up sensible.</summary>
    public static CubeOrientation BringFaceToFront(Orientation3D face) => face switch
    {
        Orientation3D.Front => new() { AxisX = CubeDirection.Right, AxisY = CubeDirection.Up, AxisZ = CubeDirection.Back },
        Orientation3D.Back => new() { AxisX = CubeDirection.Left, AxisY = CubeDirection.Up, AxisZ = CubeDirection.Front },
        Orientation3D.Right => new() { AxisX = CubeDirection.Front, AxisY = CubeDirection.Up, AxisZ = CubeDirection.Right },
        Orientation3D.Left => new() { AxisX = CubeDirection.Back, AxisY = CubeDirection.Up, AxisZ = CubeDirection.Left },
        Orientation3D.Top => new() { AxisX = CubeDirection.Right, AxisY = CubeDirection.Front, AxisZ = CubeDirection.Up },
        Orientation3D.Bottom => new() { AxisX = CubeDirection.Right, AxisY = CubeDirection.Back, AxisZ = CubeDirection.Down },
        _ => CubeOrientation.Default
    };

    /// <summary>Applies a mirror (handedness flip) by negating the physical X axis of the orientation.</summary>
    public static CubeOrientation ApplyHandedness(CubeOrientation orientation, CartesianOrientation handedness)
    {
        if (handedness == CartesianOrientation.RightHanded)
            return orientation;
        var mirrorX = new CubeOrientation { AxisX = CubeDirection.Left }; // flips display X
        return CubeOrientation.Compose(orientation, mirrorX);
    }
}
