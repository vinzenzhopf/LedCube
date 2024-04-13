using System.Collections.Generic;
using LedCube.Core.Common.CubeData.Generator;

namespace LedCube.Core.Common.Model.Cube;

public static class CubeDataExtensions
{

    public static IEnumerable<Point3D> EnumeratePositions(this ICubeData cubeData) => new PositionGenerator3D(cubeData.Size);

    public static void ForEach(this ICubeData cubeData, LedFunc ledFunc)
    {
        foreach (var position in new PositionGenerator3D(cubeData.Size))
        {
            var value = ledFunc(position, cubeData.GetLed(position));
            if (value.HasValue)
            {
                cubeData.SetLed(position, value.Value);
            }
        }
    }
    
    public static void ForEach(this ICubeData cubeData, LedAction ledAction)
    {
        foreach (var position in new PositionGenerator3D(cubeData.Size))
        {
            ledAction(position, cubeData.GetLed(position));
        }
    }
    
}

public delegate bool? LedFunc(Point3D position, bool currentValue);
public delegate void LedAction(Point3D position, bool currentValue);