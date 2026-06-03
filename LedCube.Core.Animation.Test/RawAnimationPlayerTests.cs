using System;
using System.Collections.Generic;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;

namespace LedCube.Core.Animation.Test;

public class RawAnimationPlayerTests
{
    private static readonly Point3D Cube16 = new(16, 16, 16);

    private static RawAnimationPlayer TwoKeyframePlayer(bool loop = false, int frameCount = 10, bool? loopOverride = null)
    {
        var manifest = PlayerFixtures.Manifest(Cube16, frameCount: frameCount, frameTimeUs: 1000, loop: loop);
        var frames = new List<Frame>
        {
            new(PlayerFixtures.BinaryFrame(Cube16, 0)),
            new(PlayerFixtures.BinaryFrame(Cube16, 1)),
        };
        var keyframes = new List<Keyframe> { new(0, 0), new(5, 1) };
        return new RawAnimationPlayer(PlayerFixtures.Build(manifest, frames, keyframes), new CubeRenderOptions(Cube16), loopOverride);
    }

    [Fact]
    public void LoopOverride_False_ForcesFinish_EvenWhenManifestLoops()
    {
        // A file authored with loop=true must still report finished after one pass when the host
        // overrides loop to false (the playlist player controls repetition itself).
        var player = TwoKeyframePlayer(loop: true, frameCount: 10, loopOverride: false);
        Assert.False(player.Loop);
        Assert.True(player.IsFinishedAt(10_000));
        Assert.Equal(9, player.TimelinePositionAt(50_000)); // holds last frame, no wrap
    }

    [Fact]
    public void LoopOverride_Null_UsesManifestLoopFlag()
    {
        Assert.True(TwoKeyframePlayer(loop: true).Loop);
        Assert.False(TwoKeyframePlayer(loop: false).Loop);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3000, 3)]
    [InlineData(5000, 5)]
    [InlineData(9000, 9)]
    [InlineData(9999, 9)]
    public void TimelinePositionAt_WithinRange(double elapsedUs, int expected)
    {
        Assert.Equal(expected, TwoKeyframePlayer().TimelinePositionAt(elapsedUs));
    }

    [Fact]
    public void TimelinePositionAt_PastEnd_NoLoop_HoldsLastFrame()
    {
        Assert.Equal(9, TwoKeyframePlayer(loop: false).TimelinePositionAt(50_000));
    }

    [Theory]
    [InlineData(10_000, 0)]
    [InlineData(12_000, 2)]
    [InlineData(27_000, 7)]
    public void TimelinePositionAt_PastEnd_Loop_Wraps(double elapsedUs, int expected)
    {
        Assert.Equal(expected, TwoKeyframePlayer(loop: true).TimelinePositionAt(elapsedUs));
    }

    [Fact]
    public void IsFinishedAt_NoLoop_TrueAtAndPastEnd()
    {
        var player = TwoKeyframePlayer(loop: false, frameCount: 10);
        Assert.False(player.IsFinishedAt(9_000));
        Assert.True(player.IsFinishedAt(10_000));
        Assert.True(player.IsFinishedAt(11_000));
    }

    [Fact]
    public void IsFinishedAt_Loop_NeverFinishes()
    {
        Assert.False(TwoKeyframePlayer(loop: true).IsFinishedAt(1_000_000));
    }

    [Fact]
    public void Advance_RendersActiveFrame()
    {
        var player = TwoKeyframePlayer();
        var cube = new CubeData<CubeDataBuffer16>();

        player.Advance(0, cube);
        Assert.True(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 0)));
        Assert.False(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 1)));

        player.Advance(5_000, cube); // crosses into keyframe 1 -> frame 1 (LED index 1)
        Assert.False(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 0)));
        Assert.True(cube.GetLed(Point3D.CoordinateFromIndex(Cube16, 1)));
    }

    [Fact]
    public void Advance_SkipsRenderWhileHoldingSameFrame()
    {
        var player = TwoKeyframePlayer();
        var cube = new CubeData<CubeDataBuffer16>();

        Assert.True(player.Advance(0, cube).Rendered);       // first render of frame 0
        Assert.False(player.Advance(1_000, cube).Rendered);  // still keyframe 0 -> no change
        Assert.False(player.Advance(4_000, cube).Rendered);  // still keyframe 0
        Assert.True(player.Advance(5_000, cube).Rendered);   // keyframe 1 -> changed
        Assert.False(player.Advance(6_000, cube).Rendered);  // still keyframe 1
    }

    [Fact]
    public void Reset_ForcesNextRender()
    {
        var player = TwoKeyframePlayer();
        var cube = new CubeData<CubeDataBuffer16>();

        Assert.True(player.Advance(0, cube).Rendered);
        Assert.False(player.Advance(1_000, cube).Rendered);
        player.Reset();
        Assert.True(player.Advance(1_000, cube).Rendered);
    }

    [Fact]
    public void Advance_ReportsFinished_NoLoop()
    {
        var player = TwoKeyframePlayer(loop: false, frameCount: 10);
        var cube = new CubeData<CubeDataBuffer16>();

        Assert.False(player.Advance(9_000, cube).Finished);
        Assert.True(player.Advance(10_000, cube).Finished);
    }

    [Fact]
    public void Constructor_RejectsSizeMismatch()
    {
        var manifest = PlayerFixtures.Manifest(new Point3D(8, 8, 8));
        var animation = PlayerFixtures.Build(
            manifest,
            new List<Frame> { new(PlayerFixtures.BinaryFrame(new Point3D(8, 8, 8), 0)) },
            new List<Keyframe> { new(0, 0) });

        Assert.Throws<NotSupportedException>(() => new RawAnimationPlayer(animation, new CubeRenderOptions(Cube16)));
    }

    [Fact]
    public void Constructor_RejectsNonBinaryFormat()
    {
        var size = new Point3D(8, 8, 8);
        var manifest = PlayerFixtures.Manifest(size, LedFormat.Grayscale);
        var stride = LedFormat.Grayscale.BytesPerFrame(size);
        var animation = PlayerFixtures.Build(
            manifest,
            new List<Frame> { new(new byte[stride]) },
            new List<Keyframe> { new(0, 0) });

        Assert.Throws<NotSupportedException>(() => new RawAnimationPlayer(animation, new CubeRenderOptions(size)));
    }
}
