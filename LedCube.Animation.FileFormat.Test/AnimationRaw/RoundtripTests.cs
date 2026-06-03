using System;
using System.Collections.Generic;
using System.Text.Json;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.Test.AnimationRaw;

public class RoundtripTests
{
    [Fact]
    public void Roundtrip_StaticBinary_16x16x16()
    {
        var animation = AnimationBuilder.Static(LedFormat.Binary, new Point3D(16, 16, 16));
        Assert.Equal(animation, AnimationBuilder.RoundTrip(animation));
    }

    [Fact]
    public void Roundtrip_StaticGrayscale_8x8x8()
    {
        var animation = AnimationBuilder.Static(LedFormat.Grayscale, new Point3D(8, 8, 8));
        Assert.Equal(animation, AnimationBuilder.RoundTrip(animation));
    }

    [Fact]
    public void Roundtrip_StaticRgb_16x16x16()
    {
        var animation = AnimationBuilder.Static(LedFormat.Rgb, new Point3D(16, 16, 16));
        Assert.Equal(animation, AnimationBuilder.RoundTrip(animation));
    }

    [Fact]
    public void Roundtrip_MultiKeyframe_NoReuse()
    {
        var size = new Point3D(8, 8, 8);
        var stride = LedFormat.Grayscale.BytesPerFrame(size);
        var frames = new List<Frame>
        {
            AnimationBuilder.Frame(stride, 1),
            AnimationBuilder.Frame(stride, 2),
            AnimationBuilder.Frame(stride, 3),
        };
        var keyframes = new List<Keyframe> { new(0, 0), new(5, 1), new(10, 2) };
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Grayscale, frameCount: 30), frames, keyframes);

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Equal(animation, read);
        Assert.Equal(3, read.Frames.Count);
    }

    [Fact]
    public void Roundtrip_MultiKeyframe_WithReuse()
    {
        var size = new Point3D(8, 8, 8);
        var stride = LedFormat.Grayscale.BytesPerFrame(size);
        var frameA = AnimationBuilder.Frame(stride, 1);
        var frameB = AnimationBuilder.Frame(stride, 2);
        // Pool intentionally contains a duplicate of A; the writer must collapse it.
        var frames = new List<Frame> { frameA, frameB, new(frameA.Data.ToArray()) };
        var keyframes = new List<Keyframe> { new(0, 0), new(4, 1), new(8, 2) };
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Grayscale, frameCount: 20), frames, keyframes);

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Equal(2, read.Frames.Count);
        Assert.Equal(new Keyframe(0, 0), read.Keyframes[0]);
        Assert.Equal(new Keyframe(4, 1), read.Keyframes[1]);
        Assert.Equal(new Keyframe(8, 0), read.Keyframes[2]);
        Assert.Equal(frameA, read.ActiveFrameAt(0));
        Assert.Equal(frameB, read.ActiveFrameAt(4));
        Assert.Equal(frameA, read.ActiveFrameAt(8));
    }

    [Fact]
    public void Roundtrip_AllOptionalManifestFields_Present()
    {
        var manifest = AnimationBuilder.Manifest(
            new Point3D(4, 4, 4), LedFormat.Binary, frameCount: 12, frameTimeUs: 33333, loop: true,
            name: "Sunrise", author: "vinzenz", description: "Slow color wash.",
            createdUtc: new DateTimeOffset(2026, 5, 31, 14, 0, 0, TimeSpan.Zero));
        var stride = LedFormat.Binary.BytesPerFrame(manifest.Size);
        var animation = new RawAnimation(
            manifest,
            new List<Frame> { new(AnimationBuilder.FrameBytes(stride, 0x0F)) },
            new List<Keyframe> { new(0, 0) });

        Assert.Equal(animation, AnimationBuilder.RoundTrip(animation));
    }

    [Fact]
    public void Roundtrip_AllOptionalManifestFields_Absent()
    {
        var animation = AnimationBuilder.Static(LedFormat.Binary, new Point3D(4, 4, 4));

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Equal(animation, read);
        Assert.Null(read.Manifest.Author);
        Assert.Null(read.Manifest.Description);
        Assert.Null(read.Manifest.CreatedUtc);
        Assert.Null(read.Manifest.ExtraFields);
    }

    [Fact]
    public void Roundtrip_LoopTrue()
    {
        var animation = WithLoop(AnimationBuilder.Static(LedFormat.Binary, new Point3D(4, 4, 4)), true);
        var read = AnimationBuilder.RoundTrip(animation);
        Assert.True(read.Manifest.SeamlessLoop);
        Assert.Equal(animation, read);
    }

    [Fact]
    public void Roundtrip_LoopFalse()
    {
        var animation = WithLoop(AnimationBuilder.Static(LedFormat.Binary, new Point3D(4, 4, 4)), false);
        var read = AnimationBuilder.RoundTrip(animation);
        Assert.False(read.Manifest.SeamlessLoop);
        Assert.Equal(animation, read);
    }

    [Fact]
    public void Roundtrip_ThumbnailEntry_Preserved()
    {
        var size = new Point3D(4, 4, 4);
        var stride = LedFormat.Binary.BytesPerFrame(size);
        var thumbnail = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3, 4, 5 };
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Binary, frameCount: 5),
            new List<Frame> { new(AnimationBuilder.FrameBytes(stride, 0x11)) },
            new List<Keyframe> { new(0, 0) },
            thumbnail: thumbnail);

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.NotNull(read.Thumbnail);
        Assert.Equal(thumbnail, read.Thumbnail);
        Assert.Equal(animation, read);
    }

    [Fact]
    public void Roundtrip_UnknownManifestField_Preserved()
    {
        var extra = new Dictionary<string, JsonElement>
        {
            ["customFlag"] = JsonSerializer.SerializeToElement(true),
            ["customCount"] = JsonSerializer.SerializeToElement(123),
        };
        var manifest = AnimationBuilder.Manifest(
            new Point3D(4, 4, 4), LedFormat.Binary, frameCount: 5, extraFields: extra);
        var stride = LedFormat.Binary.BytesPerFrame(manifest.Size);
        var animation = new RawAnimation(
            manifest,
            new List<Frame> { new(AnimationBuilder.FrameBytes(stride, 0x22)) },
            new List<Keyframe> { new(0, 0) });

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.NotNull(read.Manifest.ExtraFields);
        Assert.True(read.Manifest.ExtraFields!["customFlag"].GetBoolean());
        Assert.Equal(123, read.Manifest.ExtraFields!["customCount"].GetInt32());
    }

    [Fact]
    public void Roundtrip_UnknownZipEntry_Preserved()
    {
        var size = new Point3D(4, 4, 4);
        var stride = LedFormat.Binary.BytesPerFrame(size);
        var extraEntries = new Dictionary<string, byte[]>
        {
            ["meta/cues.bin"] = new byte[] { 9, 8, 7, 6, 5 },
        };
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Binary, frameCount: 5),
            new List<Frame> { new(AnimationBuilder.FrameBytes(stride, 0x33)) },
            new List<Keyframe> { new(0, 0) },
            extraEntries: extraEntries);

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.True(read.ExtraEntries.ContainsKey("meta/cues.bin"));
        Assert.Equal(new byte[] { 9, 8, 7, 6, 5 }, read.ExtraEntries["meta/cues.bin"]);
        Assert.Equal(animation, read);
    }

    private static RawAnimation WithLoop(RawAnimation source, bool loop) => new(
        source.Manifest with { SeamlessLoop = loop },
        source.Frames,
        source.Keyframes,
        source.Thumbnail);
}
