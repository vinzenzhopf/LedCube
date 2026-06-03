using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.PluginBase;
using LedCube.Plugins.Animation.FileAnimation;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LedCube.Core.Animation.Test;

public class FileAnimationGeneratorTests
{
    private static readonly Point3D Cube16 = new(16, 16, 16);

    private static string WriteTempAnimation()
    {
        var manifest = PlayerFixtures.Manifest(Cube16, frameCount: 4, frameTimeUs: 1000);
        var frames = new List<Frame>
        {
            new(PlayerFixtures.BinaryFrame(Cube16, 0)),
            new(PlayerFixtures.BinaryFrame(Cube16, 1)),
        };
        var keyframes = new List<Keyframe> { new(0, 0), new(2, 1) };
        var animation = PlayerFixtures.Build(manifest, frames, keyframes);

        var path = Path.Combine(Path.GetTempPath(), $"fileanim-{Guid.NewGuid():N}.lcanimraw");
        using var fs = File.Create(path);
        LcAnimRawWriter.Write(fs, animation);
        return path;
    }

    private static FileAnimationGenerator NewGenerator() => new(
        Options.Create(new FileAnimationConfiguration()),
        NullLogger<FileAnimationGenerator>.Instance);

    [Fact]
    public async Task PlaysAnimationFromPath_EndToEnd()
    {
        var path = WriteTempAnimation();
        try
        {
            var generator = NewGenerator();
            generator.Configure(new AnimationConfig { ["FilePath"] = path });

            // FrameTime is available after Configure (peeked from the manifest).
            Assert.Equal(TimeSpan.FromMicroseconds(1000), generator.FrameTime);

            await generator.InitializeAsync(CancellationToken.None);

            var cube = new CubeData<CubeDataBuffer16>();
            generator.Start(new AnimationContext(TimeSpan.FromMicroseconds(1000), 0, cube));

            // t=0 -> keyframe 0 -> LED index 0 on.
            var r0 = generator.DrawFrame(new FrameContext(TimeSpan.Zero, TimeSpan.Zero, 0UL, cube));
            Assert.Equal(DrawingResult.Continue, r0);
            Assert.True(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 0)));
            Assert.False(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 1)));

            // t=2 -> keyframe 1 -> LED index 1 on.
            generator.DrawFrame(new FrameContext(TimeSpan.Zero, TimeSpan.Zero, 2000UL, cube));
            Assert.False(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 0)));
            Assert.True(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 1)));

            // Past the timeline end (frameCount * frameTimeUs = 4000us) -> Finished.
            var rEnd = generator.DrawFrame(new FrameContext(TimeSpan.Zero, TimeSpan.Zero, 4000UL, cube));
            Assert.Equal(DrawingResult.Finished, rEnd);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void DrawFrame_WithoutValidFile_ReportsFinished()
    {
        var generator = NewGenerator();
        generator.Configure(new AnimationConfig { ["FilePath"] = "does-not-exist.lcanimraw" });

        var cube = new CubeData<CubeDataBuffer16>();
        generator.Start(new AnimationContext(TimeSpan.Zero, 0, cube));

        var result = generator.DrawFrame(new FrameContext(TimeSpan.Zero, TimeSpan.Zero, 0UL, cube));
        Assert.Equal(DrawingResult.Finished, result);
    }
}
