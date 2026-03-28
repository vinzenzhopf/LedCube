using System;

namespace LedCube.Core.UI.TimelineControl;

/// <summary>
/// Immutable snapshot of all coordinate math for a given zoom/scroll/viewport state.
/// Rebuilt every frame from TimelineState + control size.
/// </summary>
public readonly record struct TimelineLayout
{
    public double ZoomScale { get; init; }       // pixels per frame
    public double ScrollOffsetPx { get; init; }
    public int TotalFrames { get; init; }
    public double ViewportWidth { get; init; }

    public double TotalWidthPx => TotalFrames * ZoomScale;

    public (int First, int Last) VisibleFrameRange
    {
        get
        {
            var first = Math.Max(0, (int)Math.Floor(ScrollOffsetPx / ZoomScale));
            var last = Math.Min(TotalFrames - 1, (int)Math.Ceiling((ScrollOffsetPx + ViewportWidth) / ZoomScale));
            return (first, last);
        }
    }

    public double FrameToPixel(int frame) => frame * ZoomScale - ScrollOffsetPx;

    /// <summary>Converts a pixel x-coordinate to the nearest frame. Always snaps — the environment is frame-discrete.</summary>
    public int PixelToFrame(double x)
    {
        var frame = (int)Math.Round((x + ScrollOffsetPx) / ZoomScale, MidpointRounding.AwayFromZero);
        return Math.Clamp(frame, 0, Math.Max(0, TotalFrames - 1));
    }
}
