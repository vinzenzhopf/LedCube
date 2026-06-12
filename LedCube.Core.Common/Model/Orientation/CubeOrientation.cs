namespace LedCube.Core.Common.Model.Orientation;

/// <summary>
/// Describes how the logical cube data maps onto the physical/displayed cube. Each logical data
/// axis (X is the fastest index — the "row" direction; then Y; then Z, the slowest) is assigned a
/// physical direction. This single mapping captures everything about the installation: where LED 0
/// physically sits (the corner where every logical axis is at its minimum), the direction the index
/// runs, which way is up, and therefore which face is "front".
///
/// A "re-front" (turning a different face toward the viewer) is just a different choice of the three
/// directions, so this one record expresses both the wiring and the desired front.
/// </summary>
public sealed record CubeOrientation
{
    /// <summary>Where logical +X (the fastest index / rows) advances. Default: to the right.</summary>
    public CubeDirection AxisX { get; init; } = CubeDirection.Right;

    /// <summary>Where logical +Y advances. Default: up.</summary>
    public CubeDirection AxisY { get; init; } = CubeDirection.Up;

    /// <summary>Where logical +Z (the slowest index / layers) advances. Default: back.</summary>
    public CubeDirection AxisZ { get; init; } = CubeDirection.Back;

    /// <summary>Identity: logical axes map straight to display axes (X→right, Y→up, Z→back).</summary>
    public static CubeOrientation Default => new();

    /// <summary>True when the three axes map to three distinct physical axes (a valid rotation/reflection).</summary>
    public bool IsValid
    {
        get
        {
            var (ax, _) = Decode(AxisX);
            var (ay, _) = Decode(AxisY);
            var (az, _) = Decode(AxisZ);
            return ax != ay && ay != az && ax != az;
        }
    }

    /// <summary>Maps a direction to a (physical axis index 0=X/1=Y/2=Z, sign +1/-1).</summary>
    public static (int axis, int sign) Decode(CubeDirection d) => d switch
    {
        CubeDirection.Right => (0, +1),
        CubeDirection.Left => (0, -1),
        CubeDirection.Up => (1, +1),
        CubeDirection.Down => (1, -1),
        CubeDirection.Back => (2, +1),
        CubeDirection.Front => (2, -1),
        _ => (0, +1)
    };

    /// <summary>The (physical axis, sign) that logical input axis (0=X,1=Y,2=Z) maps to.</summary>
    public (int axis, int sign) Output(int inputAxis) => inputAxis switch
    {
        0 => Decode(AxisX),
        1 => Decode(AxisY),
        _ => Decode(AxisZ)
    };

    public static CubeDirection ToDirection(int axis, int sign) => (axis, sign) switch
    {
        (0, +1) => CubeDirection.Right,
        (0, _) => CubeDirection.Left,
        (1, +1) => CubeDirection.Up,
        (1, _) => CubeDirection.Down,
        (2, +1) => CubeDirection.Back,
        _ => CubeDirection.Front
    };

    public static CubeOrientation FromOutputs((int axis, int sign) x, (int axis, int sign) y, (int axis, int sign) z) => new()
    {
        AxisX = ToDirection(x.axis, x.sign),
        AxisY = ToDirection(y.axis, y.sign),
        AxisZ = ToDirection(z.axis, z.sign)
    };

    /// <summary>
    /// Composition: returns the orientation equivalent to applying <paramref name="inner"/> then
    /// <paramref name="outer"/> (i.e. outer ∘ inner). Works in centered coordinates where each
    /// orientation is a pure signed-axis permutation, so this is signed-permutation multiplication.
    /// </summary>
    public static CubeOrientation Compose(CubeOrientation inner, CubeOrientation outer)
    {
        System.Span<(int axis, int sign)> result = stackalloc (int, int)[3];
        for (var i = 0; i < 3; i++)
        {
            var (hAxis, hSign) = inner.Output(i);
            var (oAxis, oSign) = outer.Output(hAxis);
            result[i] = (oAxis, hSign * oSign);
        }
        return FromOutputs(result[0], result[1], result[2]);
    }

    /// <summary>
    /// Maps a logical cube coordinate to its display coordinate. <paramref name="size"/> is the
    /// logical size; the returned coordinate is within the (axis-permuted) display bounds.
    /// </summary>
    public Point3D ToDisplay(Point3D logical, Point3D size)
    {
        System.Span<int> d = stackalloc int[3];
        Place(d, AxisX, logical.X, size.X);
        Place(d, AxisY, logical.Y, size.Y);
        Place(d, AxisZ, logical.Z, size.Z);
        return new Point3D(d[0], d[1], d[2]);

        static void Place(System.Span<int> dst, CubeDirection dir, int value, int axisSize)
        {
            var (axis, sign) = Decode(dir);
            dst[axis] = sign > 0 ? value : axisSize - 1 - value;
        }
    }

    /// <summary>The display-space size (logical size with axes permuted to match this orientation).</summary>
    public Point3D DisplaySize(Point3D size)
    {
        System.Span<int> d = stackalloc int[3];
        d[Decode(AxisX).axis] = size.X;
        d[Decode(AxisY).axis] = size.Y;
        d[Decode(AxisZ).axis] = size.Z;
        return new Point3D(d[0], d[1], d[2]);
    }
}
