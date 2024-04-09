using System;
using System.Numerics;
using LedCube.Core.Common.CubeData.Generator;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Sdf.Core;

public static class RenderExtensions
{

    public static void Render(this ICubeData cubeData, Sdf3D sdf, float time, SdfRenderOptions options)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(options.Margin);

        if (options.Centered)
        {
            Vector3 center = cubeData.Size;
            center /= 2;
            sdf = Sdf.Translate(sdf, center);
        }
        
        foreach (var point in new PositionGenerator3D(cubeData.Size))
        {
            var value = sdf(point, time);
            cubeData.SetLed(point, value <= options.Margin);
        }
    }
}