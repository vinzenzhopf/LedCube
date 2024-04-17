using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Plugins.Animation.Snake3D;

public static class Snake3DExtensions
{
    public static Point3D RandomPoint(this Random random, Point3D limit)
    {
        return Point3D.CoordinateFromIndex(limit, random.Next(limit.ElementProduct));
    }

    public static void SetPlane(this ICubeData cube, int z, bool value)
    {
        for(var y = 0; y < cube.Size.Y; y++)
        {
            for (var x = 0; x < cube.Size.X; x++)
            {
                cube.SetLed(new Point3D(x, y, z), value);
            }    
        }
    }

    public static void SetCube(this ICubeData cube, bool value)
    {
        for (var z = 0; z < cube.Size.Z; z++)
        {
            for (var y = 0; y < cube.Size.Y; y++)
            {
                for (var x = 0; x < cube.Size.X; x++)
                {
                    cube.SetLed(new Point3D(x, y, z), value);
                }
            }
        }
    }
}