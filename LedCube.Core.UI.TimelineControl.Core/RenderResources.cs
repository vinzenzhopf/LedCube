using System;
using SkiaSharp;

namespace LedCube.Core.UI.TimelineControl;

/// <summary>
/// Cached SKPaint instances for timeline rendering. Allocate once, dispose when done.
/// </summary>
public sealed class RenderResources : IDisposable
{
    public SKPaint BackgroundPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0x1E, 0x1E, 0x1E)
    };

    public SKPaint BaselinePaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0x55, 0x55, 0x55),
        StrokeWidth = 1f
    };

    public SKPaint MinorTickPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0x44, 0x44, 0x44),
        StrokeWidth = 1f
    };

    public SKPaint MajorTickPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0x88, 0x88, 0x88),
        StrokeWidth = 1f
    };

    public SKFont RulerLabelFont { get; } = new SKFont(SKTypeface.Default, 10f);

    public SKPaint RulerLabelPaint { get; } = new SKPaint
    {
        Color = SKColors.White
    };

    public SKFont TimeLabelFont { get; } = new SKFont(SKTypeface.Default, 9f);

    public SKPaint TimeLabelPaint { get; } = new SKPaint
    {
        Color = new SKColor(0xCC, 0xCC, 0xCC)
    };

    public SKPaint PlayheadPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = SKColors.White,
        StrokeWidth = 2f
    };

    public SKPaint GhostLinePaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0xFF, 0xFF, 0xFF, 0x99),
        StrokeWidth = 1f,
        PathEffect = SKPathEffect.CreateDash(new float[] { 4f, 4f }, 0f)
    };

    public SKPaint SelectionPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0x56, 0x9A, 0xE8, 0x40)
    };

    public SKPaint SelectionBorderPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0x56, 0x9A, 0xE8),
        StrokeWidth = 1f
    };

    public SKPaint LoopPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0xF0, 0xA0, 0x30, 0x40)
    };

    public SKPaint LoopHandlePaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = new SKColor(0xF0, 0xA0, 0x30),
        StrokeWidth = 2f
    };

    public SKPaint LoopHandleFill { get; } = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        Color = new SKColor(0xF0, 0xA0, 0x30)
    };

    public SKPaint MarkerPointPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 2f
    };

    public SKPaint MarkerRangeFill { get; } = new SKPaint
    {
        Style = SKPaintStyle.Fill
    };

    public SKPaint MarkerRangeBorder { get; } = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1f
    };
    
    public SKFont FlagFont { get; } = new SKFont(SKTypeface.Default, 9f);

    public SKPaint FlagTextPaint { get; } = new SKPaint
    {
        Color = SKColors.White
    };

    public SKPaint FlagBackgroundPaint { get; } = new SKPaint
    {
        Style = SKPaintStyle.Fill
    };

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        BackgroundPaint.Dispose();
        BaselinePaint.Dispose();
        MinorTickPaint.Dispose();
        MajorTickPaint.Dispose();
        RulerLabelFont.Dispose();
        RulerLabelPaint.Dispose();
        TimeLabelFont.Dispose();
        TimeLabelPaint.Dispose();
        PlayheadPaint.Dispose();
        GhostLinePaint.PathEffect?.Dispose();
        GhostLinePaint.Dispose();
        SelectionPaint.Dispose();
        SelectionBorderPaint.Dispose();
        LoopPaint.Dispose();
        LoopHandlePaint.Dispose();
        LoopHandleFill.Dispose();
        MarkerPointPaint.Dispose();
        MarkerRangeFill.Dispose();
        MarkerRangeBorder.Dispose();
        FlagFont.Dispose();
        FlagTextPaint.Dispose();
        FlagBackgroundPaint.Dispose();
    }
}
