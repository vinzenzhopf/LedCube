using System;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Sdf.Core;
using RawAnimation = LedCube.Animation.FileFormat.AnimationRaw.Model.Animation;
using SdfFactory = LedCube.Sdf.Core.Sdf;

namespace LedCube.Animation.DemoGen.Demos;

/// <summary>A sphere pulsing between small and large radius, rasterized via the SDF library.</summary>
public sealed class PulsingSphereDemo : IDemo
{
    private const int FrameCount = 60;

    public string Name => "pulsing-sphere";

    public RawAnimation Build()
    {
        var cube = new CubeData<CubeDataBuffer16>();
        var options = new SdfRenderOptions { Centered = true, Margin = 0.5f };

        var author = new AnimationAuthor(
            cube.Size,
            frameTimeUs: 33_000, // ~30 fps
            seamlessLoop: true,
            name: "Pulsing Sphere",
            description: "A sphere breathing in and out, rendered from a signed distance field.");

        for (var f = 0; f < FrameCount; f++)
        {
            var phase = f / (float)FrameCount * MathF.Tau;
            var radius = 3f + 3.5f * (0.5f * (1f + MathF.Sin(phase)));
            var sdf = SdfFactory.Sphere(radius);

            author.AppendFrame(cube, c => c.Render(sdf, 0f, options));
        }

        return author.Build();
    }
}
