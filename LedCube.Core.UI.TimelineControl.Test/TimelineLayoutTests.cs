using System;
using Xunit;
using LedCube.Core.UI.TimelineControl;

namespace LedCube.Core.UI.TimelineControl.Tests;

public class TimelineLayoutTests
{
    private static TimelineLayout Layout(double zoom, double scroll, int total, double viewport) => new()
    {
        ZoomScale = zoom,
        ScrollOffsetPx = scroll,
        TotalFrames = total,
        ViewportWidth = viewport
    };

    // ---- FrameToPixel ----

    [Fact]
    public void FrameToPixel_Frame0_ScrollOffset0_Returns0()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 100, viewport: 200);
        Assert.Equal(0.0, layout.FrameToPixel(0));
    }

    [Theory]
    [InlineData(10, 5)]
    [InlineData(10, 10)]
    [InlineData(5, 20)]
    public void FrameToPixel_FrameN_ScrollOffset0_ReturnsNTimesZoom(double zoom, int frame)
    {
        var layout = Layout(zoom, scroll: 0, total: 100, viewport: 200);
        Assert.Equal(frame * zoom, layout.FrameToPixel(frame));
    }

    [Fact]
    public void FrameToPixel_ScrollOffsetShiftsPixelsLeft()
    {
        var layout = Layout(zoom: 10, scroll: 50, total: 100, viewport: 200);
        // frame 5 -> 5*10 - 50 = 0
        Assert.Equal(0.0, layout.FrameToPixel(5));
        // frame 0 -> 0 - 50 = -50
        Assert.Equal(-50.0, layout.FrameToPixel(0));
    }

    [Fact]
    public void FrameToPixel_CanBeNegativeWhenScrolledRight()
    {
        var layout = Layout(zoom: 10, scroll: 100, total: 100, viewport: 200);
        Assert.True(layout.FrameToPixel(0) < 0);
    }

    // ---- PixelToFrame ----

    [Theory]
    [InlineData(10, 0, 100, 0)]
    [InlineData(10, 0, 100, 5)]
    [InlineData(10, 0, 100, 50)]
    [InlineData(5, 25, 100, 10)]
    [InlineData(20, 0, 100, 99)]
    public void PixelToFrame_RoundTrip(double zoom, double scroll, int total, int frame)
    {
        var layout = Layout(zoom, scroll, total, viewport: 2000);
        var pixel = layout.FrameToPixel(frame);
        Assert.Equal(frame, layout.PixelToFrame(pixel));
    }

    [Theory]
    [InlineData(10, 0, 100, 5.0, 1)]  // 5.0 / 10 = 0.5 -> rounds to 0 (banker's rounding) or 1; Math.Round uses MidpointRounding.ToEven
    [InlineData(10, 0, 100, 15.0, 2)] // 15.0 / 10 = 1.5 -> rounds to 2
    [InlineData(10, 0, 100, 25.0, 3)] // 25.0 / 10 = 2.5 -> rounds to 3 (AwayFromZero)
    public void PixelToFrame_HalfwaySnapping(double zoom, double scroll, int total, double pixel, int expected)
    {
        var layout = Layout(zoom, scroll, total, viewport: 2000);
        Assert.Equal(expected, layout.PixelToFrame(pixel));
    }

    [Fact]
    public void PixelToFrame_BelowZero_ClampsTo0()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 100, viewport: 200);
        Assert.Equal(0, layout.PixelToFrame(-50));
    }

    [Fact]
    public void PixelToFrame_AfterLastFrame_ClampsToTotalFramesMinus1()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 100, viewport: 200);
        Assert.Equal(99, layout.PixelToFrame(10000));
    }

    [Fact]
    public void PixelToFrame_TotalFrames1_AlwaysReturns0()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 1, viewport: 200);
        Assert.Equal(0, layout.PixelToFrame(0));
        Assert.Equal(0, layout.PixelToFrame(100));
        Assert.Equal(0, layout.PixelToFrame(-50));
    }

    [Fact]
    public void PixelToFrame_TotalFrames0_Returns0()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 0, viewport: 200);
        Assert.Equal(0, layout.PixelToFrame(0));
        Assert.Equal(0, layout.PixelToFrame(100));
    }

    // ---- VisibleFrameRange ----

    [Fact]
    public void VisibleFrameRange_NoScroll_StartsAt0()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 100, viewport: 100);
        var (first, last) = layout.VisibleFrameRange;
        Assert.Equal(0, first);
        Assert.Equal(10, last);
    }

    [Fact]
    public void VisibleFrameRange_ScrolledRight_FirstShifts()
    {
        var layout = Layout(zoom: 10, scroll: 50, total: 100, viewport: 100);
        var (first, _) = layout.VisibleFrameRange;
        Assert.Equal(5, first);
    }

    [Fact]
    public void VisibleFrameRange_LastClampedToTotalFramesMinus1()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 5, viewport: 1000);
        var (_, last) = layout.VisibleFrameRange;
        Assert.Equal(4, last);
    }

    [Fact]
    public void VisibleFrameRange_EntireAnimationFits_FirstAndLastSpanAll()
    {
        var layout = Layout(zoom: 10, scroll: 0, total: 10, viewport: 100);
        var (first, last) = layout.VisibleFrameRange;
        Assert.Equal(0, first);
        Assert.Equal(9, last);
    }

    // ---- TotalWidthPx ----

    [Theory]
    [InlineData(10, 100, 1000)]
    [InlineData(5, 50, 250)]
    [InlineData(1, 1, 1)]
    public void TotalWidthPx_EqualsFramesTimesZoom(double zoom, int total, double expected)
    {
        var layout = Layout(zoom, scroll: 0, total, viewport: 500);
        Assert.Equal(expected, layout.TotalWidthPx);
    }

    // ---- ZoomScale extremes ----

    [Fact]
    public void SmallZoomScale_VisibleRangeSpansManyFrames()
    {
        var layout = Layout(zoom: 0.5, scroll: 0, total: 1000, viewport: 200);
        var (first, last) = layout.VisibleFrameRange;
        Assert.Equal(0, first);
        Assert.True(last > 100, $"Expected last > 100, got {last}");
    }

    [Fact]
    public void LargeZoomScale_SingleFrameSpansViewport()
    {
        var layout = Layout(zoom: 200, scroll: 0, total: 100, viewport: 200);
        var (first, last) = layout.VisibleFrameRange;
        // With zoom=200 and viewport=200: ceil((0+200)/200)=1
        Assert.Equal(0, first);
        Assert.Equal(1, last);
    }
}
