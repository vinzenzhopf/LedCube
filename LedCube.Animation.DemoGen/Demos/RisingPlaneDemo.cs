using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;

namespace LedCube.Animation.DemoGen.Demos;

/// <summary>A single lit Z-plane sweeping up the cube and wrapping — the simplest hand-drawn demo.</summary>
public sealed class RisingPlaneDemo : IDemo
{
    public string Name => "rising-plane";

    public RawAnimation Build()
    {
        var cube = new CubeData<CubeDataBuffer16>();
        var size = cube.Size;

        var author = new AnimationAuthor(
            size,
            frameTimeUs: 50_000, // 20 fps
            loop: true,
            name: "Rising Plane",
            description: "A horizontal plane sweeping up the cube.");

        for (var z = 0; z < size.Z; z++)
        {
            var layer = z;
            author.AppendFrame(cube, c =>
            {
                for (var x = 0; x < size.X; x++)
                for (var y = 0; y < size.Y; y++)
                {
                    c.SetLed(new Point3D(x, y, layer), true);
                }
            });
        }

        return author.Build();
    }
}
