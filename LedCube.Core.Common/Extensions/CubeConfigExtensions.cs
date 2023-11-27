using LedCube.Core.Common.Config;
using LedCube.Core.Common.Model;

namespace LedCube.Core.Common.Extensions;

public static class CubeConfigExtensions
{
    public static Point3D ToPoint(this CubeDimensions dimensions)
    {
        return new Point3D(dimensions.X, dimensions.Y, dimensions.Z);
    }
}