using System;
using System.Collections.Generic;
using SkiaSharp;

namespace LedCube.Core.UI.TimelineControl;

/// <summary>
/// Stateless, immediate-mode SkiaSharp renderer for the timeline control.
/// </summary>
public static class TimelineRenderer
{
    public const float RulerHeight = 24f;
    public const float DefaultTrackHeight = 48f;
    private const float MinTrackHeight = 20f;

    // Triangle handle half-width and height for loop/marker handles
    private const float HandleHalfWidth = 8f;
    private const float HandleHeight = 11f;

    // Flag label banner dimensions
    private const float FlagPadX = 4f;
    private const float FlagHeight = 13f;
    private const float FlagY = 1f;

    // Major tick label: minimum pixel spacing before choosing a wider interval
    private static readonly int[] MajorTickIntervals = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000 };

    public static void Draw(
        SKCanvas canvas,
        TimelineLayout layout,
        TimelineState state,
        DragOperation? activeDrag,
        RenderResources res)
    {
        float trackHeight = layout.ViewportHeight > RulerHeight + MinTrackHeight
            ? (float)layout.ViewportHeight - RulerHeight
            : DefaultTrackHeight;
        float totalHeight = RulerHeight + trackHeight;

        DrawBackground(canvas, totalHeight, layout, res);
        DrawLoopRegion(canvas, layout, state, trackHeight, res);
        DrawSelection(canvas, layout, state, trackHeight, res);
        DrawTicks(canvas, layout, state, res);
        DrawRulerLabels(canvas, layout, state, res);
        DrawMarkers(canvas, layout, state, trackHeight, res);
        DrawPlayhead(canvas, layout, state, totalHeight, res);
        DrawDragGhost(canvas, layout, activeDrag, totalHeight, res);
    }

    private static void DrawBackground(SKCanvas canvas, float totalHeight, TimelineLayout layout, RenderResources res)
    {
        canvas.DrawRect(0f, 0f, (float)layout.ViewportWidth, totalHeight, res.BackgroundPaint);
        // Baseline between ruler and track
        canvas.DrawLine(0f, RulerHeight, (float)layout.ViewportWidth, RulerHeight, res.BaselinePaint);
    }

    private static void DrawLoopRegion(SKCanvas canvas, TimelineLayout layout, TimelineState state, float trackHeight, RenderResources res)
    {
        if (!state.LoopEnabled || state.LoopStart is null || state.LoopEnd is null)
            return;

        float x1 = (float)layout.FrameToPixel(state.LoopStart.Value);
        float x2 = (float)layout.FrameToPixel(state.LoopEnd.Value);

        if (x2 < 0f || x1 > (float)layout.ViewportWidth)
            return;

        float clampedX1 = Math.Max(0f, x1);
        float clampedX2 = Math.Min((float)layout.ViewportWidth, x2);

        // Tinted band in track area
        canvas.DrawRect(clampedX1, RulerHeight, clampedX2 - clampedX1, trackHeight, res.LoopPaint);

        // Vertical handle lines
        canvas.DrawLine(x1, RulerHeight, x1, RulerHeight + trackHeight, res.LoopHandlePaint);
        canvas.DrawLine(x2, RulerHeight, x2, RulerHeight + trackHeight, res.LoopHandlePaint);

        // Triangle handles at top of track
        DrawDownTriangle(canvas, x1, RulerHeight, res.LoopHandleFill);
        DrawDownTriangle(canvas, x2, RulerHeight, res.LoopHandleFill);
    }

    private static void DrawSelection(SKCanvas canvas, TimelineLayout layout, TimelineState state, float trackHeight, RenderResources res)
    {
        if (state.Mode != TimelineMode.Edit)
            return;
        if (state.SelectionStart is null || state.SelectionEnd is null)
            return;

        int s = Math.Min(state.SelectionStart.Value, state.SelectionEnd.Value);
        int e = Math.Max(state.SelectionStart.Value, state.SelectionEnd.Value);

        float x1 = (float)layout.FrameToPixel(s);
        float x2 = (float)layout.FrameToPixel(e);

        if (x2 < 0f || x1 > (float)layout.ViewportWidth)
            return;

        float clampedX1 = Math.Max(0f, x1);
        float clampedX2 = Math.Min((float)layout.ViewportWidth, x2);

        canvas.DrawRect(clampedX1, RulerHeight, clampedX2 - clampedX1, trackHeight, res.SelectionPaint);
        canvas.DrawLine(x1, RulerHeight, x1, RulerHeight + trackHeight, res.SelectionBorderPaint);
        canvas.DrawLine(x2, RulerHeight, x2, RulerHeight + trackHeight, res.SelectionBorderPaint);
    }

    private static void DrawTicks(SKCanvas canvas, TimelineLayout layout, TimelineState state, RenderResources res)
    {
        var (first, last) = layout.VisibleFrameRange;
        float viewportWidth = (float)layout.ViewportWidth;

        int majorInterval = GetMajorTickInterval(layout.ZoomScale);

        const float minorTickBottom = RulerHeight;
        const float minorTickTop = RulerHeight - 4f;
        const float majorTickTop = RulerHeight - 10f;

        bool showMinor = layout.ZoomScale >= 4.0;

        for (int frame = first; frame <= last; frame++)
        {
            float x = (float)layout.FrameToPixel(frame);
            if (x < 0f || x > viewportWidth)
                continue;

            bool isMajor = frame % majorInterval == 0;

            if (isMajor)
            {
                canvas.DrawLine(x, majorTickTop, x, minorTickBottom, res.MajorTickPaint);
            }
            else if (showMinor)
            {
                canvas.DrawLine(x, minorTickTop, x, minorTickBottom, res.MinorTickPaint);
            }
        }
    }

    private static void DrawRulerLabels(SKCanvas canvas, TimelineLayout layout, TimelineState state, RenderResources res)
    {
        var (first, last) = layout.VisibleFrameRange;
        float viewportWidth = (float)layout.ViewportWidth;

        int majorInterval = GetMajorTickInterval(layout.ZoomScale);

        // Frame number baseline: just above the ruler bottom
        const float frameNumberY = RulerHeight - 12f;
        // Time label baseline: below frame number (only when FrameTime is set)
        const float timeLabelY = RulerHeight - 2f;

        bool hasFrameTime = state.FrameTime.HasValue;

        // Align first major tick at or after first visible
        int startFrame = (first / majorInterval) * majorInterval;
        if (startFrame < first) startFrame += majorInterval;

        for (int frame = startFrame; frame <= last; frame += majorInterval)
        {
            float x = (float)layout.FrameToPixel(frame);
            if (x < 0f || x > viewportWidth)
                continue;

            string frameLabel = frame.ToString();
            canvas.DrawText(frameLabel, x, frameNumberY, SKTextAlign.Center, res.RulerLabelFont, res.RulerLabelPaint);

            if (hasFrameTime)
            {
                TimeSpan t = state.FrameTime!.Value * frame;
                string timeLabel = $"{(int)t.TotalMinutes}:{t.Seconds:D2}.{t.Milliseconds:D3}";
                canvas.DrawText(timeLabel, x, timeLabelY, SKTextAlign.Center, res.TimeLabelFont, res.TimeLabelPaint);
            }
        }
    }

    private static void DrawMarkers(SKCanvas canvas, TimelineLayout layout, TimelineState state, float trackHeight, RenderResources res)
    {
        IReadOnlyList<MarkerBase> markers = state.Markers;
        var (first, last) = layout.VisibleFrameRange;
        float viewportWidth = (float)layout.ViewportWidth;
        float totalHeight = RulerHeight + trackHeight;

        for (int i = 0; i < markers.Count; i++)
        {
            var marker = markers[i];

            if (marker is PointMarker point)
            {
                if (point.Frame < first || point.Frame > last)
                    continue;

                float x = (float)layout.FrameToPixel(point.Frame);
                if (x < 0f || x > viewportWidth)
                    continue;

                res.MarkerPointPaint.Color = point.Color;
                canvas.DrawLine(x, RulerHeight, x, totalHeight, res.MarkerPointPaint);
                res.MarkerRangeFill.Color = new SKColor(point.Color.Red, point.Color.Green, point.Color.Blue, 0xCC);
                DrawDownTriangle(canvas, x, RulerHeight, res.MarkerRangeFill);

                if (!string.IsNullOrEmpty(point.Label))
                    DrawFlag(canvas, x, point.Color, point.Label, res);
            }
            else if (marker is RangeMarker range)
            {
                float x1 = (float)layout.FrameToPixel(range.StartFrame);
                float x2 = (float)layout.FrameToPixel(range.EndFrame);

                if (x2 < 0f || x1 > viewportWidth)
                    continue;

                float clampedX1 = Math.Max(0f, x1);
                float clampedX2 = Math.Min(viewportWidth, x2);

                // Semi-transparent fill
                res.MarkerRangeFill.Color = new SKColor(range.Color.Red, range.Color.Green, range.Color.Blue, 0x40);
                canvas.DrawRect(clampedX1, RulerHeight, clampedX2 - clampedX1, trackHeight, res.MarkerRangeFill);

                // Border lines
                res.MarkerRangeBorder.Color = range.Color;
                canvas.DrawLine(x1, RulerHeight, x1, totalHeight, res.MarkerRangeBorder);
                canvas.DrawLine(x2, RulerHeight, x2, totalHeight, res.MarkerRangeBorder);

                // Drag handles at top edge
                res.MarkerRangeFill.Color = new SKColor(range.Color.Red, range.Color.Green, range.Color.Blue, 0xCC);
                DrawDownTriangle(canvas, x1, RulerHeight, res.MarkerRangeFill);
                DrawDownTriangle(canvas, x2, RulerHeight, res.MarkerRangeFill);

                if (!string.IsNullOrEmpty(range.Label))
                    DrawFlag(canvas, x1, range.Color, range.Label, res);
            }
        }
    }

    private static void DrawPlayhead(SKCanvas canvas, TimelineLayout layout, TimelineState state, float totalHeight, RenderResources res)
    {
        float x = (float)layout.FrameToPixel(state.CurrentFrame);
        if (x < 0f || x > (float)layout.ViewportWidth)
            return;

        canvas.DrawLine(x, 0f, x, totalHeight, res.PlayheadPaint);
    }

    private static void DrawDragGhost(SKCanvas canvas, TimelineLayout layout, DragOperation? activeDrag, float totalHeight, RenderResources res)
    {
        if (activeDrag is null)
            return;

        float x = (float)layout.FrameToPixel(activeDrag.GhostFrame);
        if (x < 0f || x > (float)layout.ViewportWidth)
            return;

        canvas.DrawLine(x, 0f, x, totalHeight, res.GhostLinePaint);
    }

    // Draws a flag label banner (colored rounded rect with white text) in the ruler area just above the track.
    private static void DrawFlag(SKCanvas canvas, float anchorX, SKColor color, string label, RenderResources res)
    {
        res.FlagBackgroundPaint.Color = new SKColor(color.Red, color.Green, color.Blue, 0xDD);

        float textWidth = res.FlagFont.MeasureText(label);
        float rectWidth = textWidth + FlagPadX * 2f;
        float rectX = anchorX;
        float rectY = FlagY;

        canvas.DrawRoundRect(rectX, rectY, rectWidth, FlagHeight, 2f, 2f, res.FlagBackgroundPaint);
        canvas.DrawText(label, rectX + FlagPadX, rectY + FlagHeight - 3f, res.FlagFont, res.FlagTextPaint);
    }

    // Draws a downward-pointing triangle with its apex pointing down, base at the given y.
    private static void DrawDownTriangle(SKCanvas canvas, float cx, float baseY, SKPaint fillPaint)
    {
        using var path = new SKPath();
        path.MoveTo(cx - HandleHalfWidth, baseY);
        path.LineTo(cx + HandleHalfWidth, baseY);
        path.LineTo(cx, baseY + HandleHeight);
        path.Close();
        canvas.DrawPath(path, fillPaint);
    }

    private static int GetMajorTickInterval(double zoomScale)
    {
        const double minSpacingPx = 50.0;
        foreach (int interval in MajorTickIntervals)
        {
            if (interval * zoomScale >= minSpacingPx)
                return interval;
        }
        return MajorTickIntervals[MajorTickIntervals.Length - 1];
    }
}
