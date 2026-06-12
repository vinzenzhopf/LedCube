namespace LedCube.Core.Common.Model.Orientation;

/// <summary>
/// A physical/display direction. In display space: Right = +X, Up = +Y, Back = +Z (away from the
/// viewer), Front = -Z (toward the viewer/camera). Used to describe where each logical data axis
/// physically advances on the real cube.
/// </summary>
public enum CubeDirection
{
    Right,
    Left,
    Up,
    Down,
    Front,
    Back
}
