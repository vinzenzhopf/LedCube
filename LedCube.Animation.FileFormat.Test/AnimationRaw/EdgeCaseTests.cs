using System.Collections.Generic;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.Test.AnimationRaw;

public class EdgeCaseTests
{
    [Fact]
    public void Edge_MinimalValidFile_SingleKeyframeSingleFrame()
    {
        var animation = AnimationBuilder.Static(LedFormat.Binary, new Point3D(1, 1, 1), frameCount: 1);
        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Equal(animation, read);
        Assert.Single(read.Frames);
        Assert.Single(read.Keyframes);
        Assert.Equal(1, read.Manifest.FrameCount);
    }

    [Fact]
    public void Edge_MaxDimensions_LargeCube()
    {
        // 64^3 binary -> N=262144, stride = 32768 bytes per frame. Exercises stride math at the upper end.
        var size = new Point3D(64, 64, 64);
        var stride = LedFormat.Binary.BytesPerFrame(size);
        Assert.Equal(32768, stride);

        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Binary, frameCount: 1),
            new List<Frame> { AnimationBuilder.Frame(stride, 7) },
            new List<Keyframe> { new(0, 0) });

        Assert.Equal(animation, AnimationBuilder.RoundTrip(animation));
    }

    [Fact]
    public void Edge_AllZeroFrameData()
    {
        var animation = AnimationBuilder.Static(LedFormat.Rgb, new Point3D(8, 8, 8), fill: 0x00);
        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Equal(animation, read);
        Assert.All(read.Frames[0].Data.ToArray(), b => Assert.Equal(0, b));
    }

    [Fact]
    public void Edge_BinaryStride_TrailingBitsZeroed()
    {
        // N=5 -> stride = ceil(5/8) = 1. Producer leaves bits 5..7 of the last byte zero.
        var size = new Point3D(5, 1, 1);
        var stride = LedFormat.Binary.BytesPerFrame(size);
        Assert.Equal(1, stride);

        var payload = new byte[] { 0b0001_1111 }; // five LEDs on, trailing high bits zero
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Binary, frameCount: 1),
            new List<Frame> { new(payload) },
            new List<Keyframe> { new(0, 0) });

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Equal(payload, read.Frames[0].Data.ToArray());
        Assert.Equal(0, read.Frames[0].Data.Span[0] & 0b1110_0000); // trailing bits remain zero
    }

    [Fact]
    public void Edge_SinglePoolFrame_ReusedByManyKeyframes()
    {
        var size = new Point3D(4, 4, 4);
        var stride = LedFormat.Binary.BytesPerFrame(size);
        var frames = new List<Frame> { new(AnimationBuilder.FrameBytes(stride, 0x5A)) };
        var keyframes = new List<Keyframe> { new(0, 0), new(5, 0), new(10, 0), new(15, 0) };
        var animation = new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Binary, frameCount: 20), frames, keyframes);

        var read = AnimationBuilder.RoundTrip(animation);

        Assert.Single(read.Frames);
        Assert.Equal(4, read.Keyframes.Count);
        Assert.All(read.Keyframes, kf => Assert.Equal(0, kf.Id));
        Assert.Equal(read.Frames[0], read.ActiveFrameAt(0));
        Assert.Equal(read.Frames[0], read.ActiveFrameAt(12));
        Assert.Equal(read.Frames[0], read.ActiveFrameAt(19));
    }
}
