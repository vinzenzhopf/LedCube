using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Orientation;

namespace LedCube.Core.UI.CubeView3D.Rendering;

/// <summary>
/// Maps cube LED indices to world-space instance positions. Index order matches
/// <c>ICubeData.Serialize</c> (LED i = bit i), so position[i] lines up with brightness[i].
/// The grid is centered on the origin with unit spacing. The <see cref="CubeOrientation"/> places
/// each logical LED at its physical/display position so the preview matches the real cube.
/// </summary>
public static class CubeInstanceLayout
{
    public static Vector3[] Build(Point3D size) => Build(size, CubeOrientation.Default);

    public static Vector3[] Build(Point3D size, CubeOrientation orientation)
    {
        var length = size.X * size.Y * size.Z;
        var positions = new Vector3[length];
        // For a cube the display size equals the logical size, so the center is shared.
        var cx = (size.X - 1) / 2f;
        var cy = (size.Y - 1) / 2f;
        var cz = (size.Z - 1) / 2f;
        for (var i = 0; i < length; i++)
        {
            var lx = i % size.X;
            var ly = (i / size.X) % size.Y;
            var lz = (i / (size.X * size.Y)) % size.Z;
            var d = orientation.ToDisplay(new Point3D(lx, ly, lz), size);
            positions[i] = new Vector3(d.X - cx, d.Y - cy, d.Z - cz);
        }
        return positions;
    }
}
