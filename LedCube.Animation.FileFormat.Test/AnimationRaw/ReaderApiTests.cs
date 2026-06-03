using System;
using System.Collections.Generic;
using LedCube.Animation.FileFormat.AnimationRaw.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;
using LedCube.Core.Common.Model;

namespace LedCube.Animation.FileFormat.Test.AnimationRaw;

public class ReaderApiTests
{
    // Timeline of 30 frames, keyframes at 0, 10, 20 -> pool ids 0, 1, 2.
    private static RawAnimation ThreeKeyframeAnimation()
    {
        var size = new Point3D(8, 8, 8);
        var stride = LedFormat.Grayscale.BytesPerFrame(size);
        var frames = new List<Frame>
        {
            AnimationBuilder.Frame(stride, 1),
            AnimationBuilder.Frame(stride, 2),
            AnimationBuilder.Frame(stride, 3),
        };
        var keyframes = new List<Keyframe> { new(0, 0), new(10, 1), new(20, 2) };
        return AnimationBuilder.RoundTrip(new RawAnimation(
            AnimationBuilder.Manifest(size, LedFormat.Grayscale, frameCount: 30), frames, keyframes));
    }

    [Fact]
    public void Reader_KeyframeIndexAt_AtKeyframeBoundary()
    {
        var animation = ThreeKeyframeAnimation();
        Assert.Equal(0, animation.KeyframeIndexAt(0));
        Assert.Equal(1, animation.KeyframeIndexAt(10));
        Assert.Equal(2, animation.KeyframeIndexAt(20));
    }

    [Fact]
    public void Reader_KeyframeIndexAt_BetweenKeyframes()
    {
        var animation = ThreeKeyframeAnimation();
        Assert.Equal(0, animation.KeyframeIndexAt(5));
        Assert.Equal(1, animation.KeyframeIndexAt(15));
        Assert.Equal(2, animation.KeyframeIndexAt(25));
    }

    [Fact]
    public void Reader_KeyframeIndexAt_AtZero()
    {
        Assert.Equal(0, ThreeKeyframeAnimation().KeyframeIndexAt(0));
    }

    [Fact]
    public void Reader_KeyframeIndexAt_AtLastFrame()
    {
        // frameCount is 30, so the last valid position is 29 and holds the last keyframe.
        Assert.Equal(2, ThreeKeyframeAnimation().KeyframeIndexAt(29));
    }

    [Fact]
    public void Reader_KeyframeIndexAt_OutOfRange()
    {
        var animation = ThreeKeyframeAnimation();
        Assert.Throws<ArgumentOutOfRangeException>(() => animation.KeyframeIndexAt(30));
        Assert.Throws<ArgumentOutOfRangeException>(() => animation.KeyframeIndexAt(-1));
    }

    [Fact]
    public void Reader_FramesIsReadOnly()
    {
        var animation = ThreeKeyframeAnimation();
        var asList = animation.Frames as IList<Frame>;

        Assert.NotNull(asList);
        Assert.True(asList!.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => asList.Add(AnimationBuilder.Frame(1, 9)));
    }
}
