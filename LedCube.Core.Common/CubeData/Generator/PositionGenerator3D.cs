using System.Collections;
using LedCube.Core.Common.Model;

namespace LedCube.Core.Common.CubeData.Generator;

public sealed class PositionGenerator3D(Point3D dimensions, bool infiniteRollover = false) : IEnumerable<Point3D>
{
    public IEnumerator<Point3D> GetEnumerator()
    {
        do
        {
            for (var z = 0; z < dimensions.Z; z++)
            {
                for (var y = 0; y < dimensions.Y; y++)
                {
                    for (var x = 0; x < dimensions.X; x++)
                    {
                        yield return new(x, y, z);
                    }
                }
            }
        } while (infiniteRollover);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}