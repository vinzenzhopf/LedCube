using System;
using Xunit;
using LedCube.Core.UI.TimelineControl;

namespace LedCube.Core.UI.TimelineControl.Tests;

/// <summary>
/// Tests PointMarker and RangeMarker clamping behavior via TimelineState.TotalFrames reduction,
/// since ApplyFrameLimit is internal and invoked exclusively through that path.
/// </summary>
public class MarkerClampTests
{
    private static TimelineState StateWith(int totalFrames, MarkerBase marker)
    {
        var state = new TimelineState { TotalFrames = totalFrames };
        state.Markers.Add(marker);
        return state;
    }

    // ---- PointMarker ----

    [Fact]
    public void PointMarker_FrameInRange_KeptUnchanged()
    {
        var m = new PointMarker { Frame = 5, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(5, m.Frame);
    }

    [Fact]
    public void PointMarker_FrameAtBoundary_KeptUnchanged()
    {
        // Frame == newTotalFrames - 1 is still in range
        var m = new PointMarker { Frame = 9, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(9, m.Frame);
    }

    [Fact]
    public void PointMarker_FrameEqualsNewTotal_Clamp_KeptAndFrameClamped()
    {
        var m = new PointMarker { Frame = 10, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(9, m.Frame);
    }

    [Fact]
    public void PointMarker_FrameEqualsNewTotal_Drop_Removed()
    {
        var m = new PointMarker { Frame = 10, ClampBehavior = ClampBehavior.Drop };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.DoesNotContain(m, state.Markers);
    }

    [Fact]
    public void PointMarker_FrameFarOutOfRange_Clamp_KeptAndFrameClamped()
    {
        var m = new PointMarker { Frame = 80, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(9, m.Frame);
    }

    [Fact]
    public void PointMarker_FrameFarOutOfRange_Drop_Removed()
    {
        var m = new PointMarker { Frame = 80, ClampBehavior = ClampBehavior.Drop };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.DoesNotContain(m, state.Markers);
    }

    // ---- RangeMarker ----

    [Fact]
    public void RangeMarker_BothInRange_KeptUnchanged()
    {
        var m = new RangeMarker { StartFrame = 3, EndFrame = 7, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(3, m.StartFrame);
        Assert.Equal(7, m.EndFrame);
    }

    [Fact]
    public void RangeMarker_OnlyEndOutOfRange_Clamp_KeptStartUnchangedEndClamped()
    {
        var m = new RangeMarker { StartFrame = 3, EndFrame = 50, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(3, m.StartFrame);
        Assert.Equal(9, m.EndFrame);
    }

    [Fact]
    public void RangeMarker_OnlyEndOutOfRange_Drop_Removed()
    {
        var m = new RangeMarker { StartFrame = 3, EndFrame = 50, ClampBehavior = ClampBehavior.Drop };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.DoesNotContain(m, state.Markers);
    }

    [Fact]
    public void RangeMarker_BothOutOfRange_Clamp_KeptBothClamped()
    {
        var m = new RangeMarker { StartFrame = 20, EndFrame = 50, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(9, m.StartFrame);
        Assert.Equal(9, m.EndFrame);
    }

    [Fact]
    public void RangeMarker_BothOutOfRange_Drop_Removed()
    {
        var m = new RangeMarker { StartFrame = 20, EndFrame = 50, ClampBehavior = ClampBehavior.Drop };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.DoesNotContain(m, state.Markers);
    }

    [Fact]
    public void RangeMarker_StartAtBoundary_KeptUnchanged()
    {
        // StartFrame == newTotalFrames - 1 is still in range
        var m = new RangeMarker { StartFrame = 9, EndFrame = 9, ClampBehavior = ClampBehavior.Clamp };
        var state = StateWith(100, m);
        state.TotalFrames = 10;
        Assert.Contains(m, state.Markers);
        Assert.Equal(9, m.StartFrame);
        Assert.Equal(9, m.EndFrame);
    }
}
