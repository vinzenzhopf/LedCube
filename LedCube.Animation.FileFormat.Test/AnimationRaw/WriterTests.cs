using System.Collections.Generic;
using System.Text.Json;
using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.Test.AnimationRaw;

public class WriterTests
{
    [Fact]
    public void Writer_AlwaysWritesCurrentVersion()
    {
        var bytes = AnimationBuilder.WriteToBytes(
            AnimationBuilder.Static(LedFormat.Binary, new Point3D(4, 4, 4)));

        using var doc = JsonDocument.Parse(RawZip.ReadEntry(bytes, "manifest.json"));
        Assert.Equal(LcAnimRawFormat.CurrentVersion, doc.RootElement.GetProperty("formatVersion").GetInt32());
    }

    [Fact]
    public void Writer_DedupsIdenticalFrames()
    {
        var size = new Point3D(8, 8, 8);
        var stride = LedFormat.Grayscale.BytesPerFrame(size);
        var a = AnimationBuilder.FrameBytes(stride, 0x44);
        var frames = new List<Frame> { new(a), new((byte[])a.Clone()), new((byte[])a.Clone()) };
        var keyframes = new List<Keyframe> { new(0, 0), new(3, 1), new(6, 2) };
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Grayscale, frameCount: 10), frames, keyframes);

        var bytes = AnimationBuilder.WriteToBytes(animation);

        // The deduped pool holds a single frame.
        Assert.Equal((long)stride, RawZip.EntrySizes(bytes, "frames.bin").Uncompressed);
        Assert.Single(AnimationBuilder.ReadFromBytes(bytes).Frames);
    }

    [Fact]
    public void Writer_UpdatesIdReferencesAfterDedup()
    {
        var size = new Point3D(8, 8, 8);
        var stride = LedFormat.Grayscale.BytesPerFrame(size);
        var frameA = AnimationBuilder.Frame(stride, 1);
        var frameB = AnimationBuilder.Frame(stride, 2);
        var frames = new List<Frame> { frameA, frameB, new(frameA.Data.ToArray()) };
        var keyframes = new List<Keyframe> { new(0, 0), new(2, 1), new(4, 2) };
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Grayscale, frameCount: 10), frames, keyframes);

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Equal(2, read.Frames.Count);
        Assert.Equal(0, read.Keyframes[0].Id);
        Assert.Equal(1, read.Keyframes[1].Id);
        Assert.Equal(0, read.Keyframes[2].Id); // remapped from the deduped slot 2 -> 0
    }

    [Fact]
    public void Writer_DeflateCompressesFramesBin()
    {
        // All-zero, highly compressible payload: compressed length must beat the raw size.
        var animation = AnimationBuilder.Static(LedFormat.Grayscale, new Point3D(8, 8, 8), fill: 0x00);

        var bytes = AnimationBuilder.WriteToBytes(animation);
        var (compressed, uncompressed) = RawZip.EntrySizes(bytes, "frames.bin");

        Assert.True(compressed < uncompressed, $"expected Deflate to shrink frames.bin ({compressed} < {uncompressed})");
    }
}
