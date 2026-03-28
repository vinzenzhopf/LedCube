using System;
using System.Collections.Generic;
using Xunit;
using LedCube.Core.UI.TimelineControl;

namespace LedCube.Core.UI.TimelineControl.Tests;

public class TimelineStateTests
{
    // ---- TotalFrames reduction ----

    [Fact]
    public void ReducingTotalFrames_ClampsCurrentFrame()
    {
        var state = new TimelineState { TotalFrames = 100, CurrentFrame = 90 };
        state.TotalFrames = 50;
        Assert.Equal(49, state.CurrentFrame);
    }

    [Fact]
    public void ReducingTotalFrames_CurrentFrameInRange_Unchanged()
    {
        var state = new TimelineState { TotalFrames = 100, CurrentFrame = 10 };
        state.TotalFrames = 50;
        Assert.Equal(10, state.CurrentFrame);
    }

    [Fact]
    public void ReducingTotalFrames_PointMarkerClamp_FrameClamped()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var marker = new PointMarker { Frame = 80, ClampBehavior = ClampBehavior.Clamp };
        state.Markers.Add(marker);
        state.TotalFrames = 50;
        Assert.Contains(marker, state.Markers);
        Assert.Equal(49, marker.Frame);
    }

    [Fact]
    public void ReducingTotalFrames_PointMarkerDrop_RemovedWhenFrameOutOfRange()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var marker = new PointMarker { Frame = 80, ClampBehavior = ClampBehavior.Drop };
        state.Markers.Add(marker);
        state.TotalFrames = 50;
        Assert.DoesNotContain(marker, state.Markers);
    }

    [Fact]
    public void ReducingTotalFrames_PointMarkerDrop_KeptWhenFrameInRange()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var marker = new PointMarker { Frame = 10, ClampBehavior = ClampBehavior.Drop };
        state.Markers.Add(marker);
        state.TotalFrames = 50;
        Assert.Contains(marker, state.Markers);
        Assert.Equal(10, marker.Frame);
    }

    [Fact]
    public void ReducingTotalFrames_RangeMarkerClamp_BothClamped()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var marker = new RangeMarker { StartFrame = 80, EndFrame = 90, ClampBehavior = ClampBehavior.Clamp };
        state.Markers.Add(marker);
        state.TotalFrames = 50;
        Assert.Contains(marker, state.Markers);
        Assert.Equal(49, marker.StartFrame);
        Assert.Equal(49, marker.EndFrame);
    }

    [Fact]
    public void ReducingTotalFrames_RangeMarkerClamp_OnlyEndOutOfRange_StartUnchanged()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var marker = new RangeMarker { StartFrame = 10, EndFrame = 80, ClampBehavior = ClampBehavior.Clamp };
        state.Markers.Add(marker);
        state.TotalFrames = 50;
        Assert.Contains(marker, state.Markers);
        Assert.Equal(10, marker.StartFrame);
        Assert.Equal(49, marker.EndFrame);
    }

    [Fact]
    public void ReducingTotalFrames_RangeMarkerDrop_RemovedWhenStartOutOfRange()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var marker = new RangeMarker { StartFrame = 80, EndFrame = 90, ClampBehavior = ClampBehavior.Drop };
        state.Markers.Add(marker);
        state.TotalFrames = 50;
        Assert.DoesNotContain(marker, state.Markers);
    }

    [Fact]
    public void ReducingTotalFrames_RangeMarkerDrop_RemovedWhenOnlyEndOutOfRange()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var marker = new RangeMarker { StartFrame = 10, EndFrame = 80, ClampBehavior = ClampBehavior.Drop };
        state.Markers.Add(marker);
        state.TotalFrames = 50;
        Assert.DoesNotContain(marker, state.Markers);
    }

    [Fact]
    public void ReducingTotalFrames_SelectionNulledWhenOutOfRange()
    {
        var state = new TimelineState { TotalFrames = 100, SelectionStart = 60, SelectionEnd = 80 };
        state.TotalFrames = 50;
        Assert.Null(state.SelectionStart);
        Assert.Null(state.SelectionEnd);
    }

    [Fact]
    public void ReducingTotalFrames_SelectionKeptWhenInRange()
    {
        var state = new TimelineState { TotalFrames = 100, SelectionStart = 10, SelectionEnd = 20 };
        state.TotalFrames = 50;
        Assert.Equal(10, state.SelectionStart);
        Assert.Equal(20, state.SelectionEnd);
    }

    // ---- Mode switching ----

    [Fact]
    public void SwitchToLive_ClearsSelection()
    {
        var state = new TimelineState { SelectionStart = 5, SelectionEnd = 15 };
        state.Mode = TimelineMode.Live;
        Assert.Null(state.SelectionStart);
        Assert.Null(state.SelectionEnd);
    }

    [Fact]
    public void SwitchToEdit_SelectionRemainsNull()
    {
        var state = new TimelineState();
        state.Mode = TimelineMode.Live;
        state.Mode = TimelineMode.Edit;
        Assert.Null(state.SelectionStart);
        Assert.Null(state.SelectionEnd);
    }

    // ---- CurrentFrame clamping ----

    [Fact]
    public void CurrentFrame_SetAboveMax_ClampsToTotalFramesMinus1()
    {
        var state = new TimelineState { TotalFrames = 50 };
        state.CurrentFrame = 999;
        Assert.Equal(49, state.CurrentFrame);
    }

    [Fact]
    public void CurrentFrame_SetBelowZero_ClampsTo0()
    {
        var state = new TimelineState { TotalFrames = 50 };
        state.CurrentFrame = -10;
        Assert.Equal(0, state.CurrentFrame);
    }

    // ---- ZoomScale clamping ----

    [Fact]
    public void ZoomScale_SetBelowMin_ClampedTo05()
    {
        var state = new TimelineState();
        state.ZoomScale = 0.1;
        Assert.Equal(0.5, state.ZoomScale);
    }

    [Fact]
    public void ZoomScale_SetValidValue_Stored()
    {
        var state = new TimelineState();
        state.ZoomScale = 15.0;
        Assert.Equal(15.0, state.ZoomScale);
    }

    // ---- ScrollOffsetPx clamping ----

    [Fact]
    public void ScrollOffsetPx_SetNegative_ClampedTo0()
    {
        var state = new TimelineState();
        state.ScrollOffsetPx = -100;
        Assert.Equal(0.0, state.ScrollOffsetPx);
    }

    [Fact]
    public void ScrollOffsetPx_SetPositive_Stored()
    {
        var state = new TimelineState();
        state.ScrollOffsetPx = 250;
        Assert.Equal(250.0, state.ScrollOffsetPx);
    }

    // ---- BuildLayout ----

    [Fact]
    public void BuildLayout_ReturnsLayoutMatchingState()
    {
        var state = new TimelineState { TotalFrames = 80 };
        state.ZoomScale = 12.0;
        state.ScrollOffsetPx = 30.0;
        var layout = state.BuildLayout(viewportWidth: 400);
        Assert.Equal(12.0, layout.ZoomScale);
        Assert.Equal(30.0, layout.ScrollOffsetPx);
        Assert.Equal(80, layout.TotalFrames);
        Assert.Equal(400.0, layout.ViewportWidth);
    }

    // ---- INPC ----

    private static List<string> CollectChanges(TimelineState state, Action act)
    {
        var changed = new List<string>();
        state.PropertyChanged += (_, e) => changed.Add(e.PropertyName ?? "");
        act();
        return changed;
    }

    [Fact]
    public void INPC_CurrentFrame_FiresOnChange()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var changed = CollectChanges(state, () => state.CurrentFrame = 5);
        Assert.Contains("CurrentFrame", changed);
    }

    [Fact]
    public void INPC_CurrentFrame_DoesNotFireWhenUnchanged()
    {
        var state = new TimelineState { TotalFrames = 100, CurrentFrame = 5 };
        var changed = CollectChanges(state, () => state.CurrentFrame = 5);
        Assert.DoesNotContain("CurrentFrame", changed);
    }

    [Fact]
    public void INPC_TotalFrames_FiresOnChange()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var changed = CollectChanges(state, () => state.TotalFrames = 50);
        Assert.Contains("TotalFrames", changed);
    }

    [Fact]
    public void INPC_TotalFrames_DoesNotFireWhenUnchanged()
    {
        var state = new TimelineState { TotalFrames = 100 };
        var changed = CollectChanges(state, () => state.TotalFrames = 100);
        Assert.DoesNotContain("TotalFrames", changed);
    }

    [Fact]
    public void INPC_Mode_FiresOnChange()
    {
        var state = new TimelineState();
        var changed = CollectChanges(state, () => state.Mode = TimelineMode.Live);
        Assert.Contains("Mode", changed);
    }

    [Fact]
    public void INPC_Mode_DoesNotFireWhenUnchanged()
    {
        var state = new TimelineState();
        var changed = CollectChanges(state, () => state.Mode = TimelineMode.Edit);
        Assert.DoesNotContain("Mode", changed);
    }

    [Fact]
    public void INPC_SelectionStart_FiresOnChange()
    {
        var state = new TimelineState();
        var changed = CollectChanges(state, () => state.SelectionStart = 5);
        Assert.Contains("SelectionStart", changed);
    }

    [Fact]
    public void INPC_SelectionEnd_FiresOnChange()
    {
        var state = new TimelineState();
        var changed = CollectChanges(state, () => state.SelectionEnd = 10);
        Assert.Contains("SelectionEnd", changed);
    }

    [Fact]
    public void INPC_LoopEnabled_FiresOnChange()
    {
        var state = new TimelineState();
        var changed = CollectChanges(state, () => state.LoopEnabled = true);
        Assert.Contains("LoopEnabled", changed);
    }

    [Fact]
    public void INPC_LoopEnabled_DoesNotFireWhenUnchanged()
    {
        var state = new TimelineState { LoopEnabled = false };
        var changed = CollectChanges(state, () => state.LoopEnabled = false);
        Assert.DoesNotContain("LoopEnabled", changed);
    }
}
