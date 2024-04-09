using System.Collections;
using LedCube.Core.Common.Model;

namespace LedCube.Core.Common.CubeData.Generator;

public sealed class PositionGenerator2D(Point2D dimensions, bool infiniteRollover = false) : IEnumerable<Point2D>
{
    public IEnumerator<Point2D> GetEnumerator()
    {
        do
        {
            for (var y = 0; y < dimensions.Y; y++)
            {
                for (var x = 0; x < dimensions.X; x++)
                {
                    yield return new(x, y);
                }
            }
        } while (infiniteRollover);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}