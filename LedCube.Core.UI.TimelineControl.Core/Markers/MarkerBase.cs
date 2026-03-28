using SkiaSharp;

namespace LedCube.Core.UI.TimelineControl;

public abstract class MarkerBase
{
    public string Label { get; set; } = string.Empty;
    public SKColor Color { get; set; } = SKColors.White;
    public bool IsDraggable { get; set; }
    public ClampBehavior ClampBehavior { get; set; } = ClampBehavior.Clamp;

    /// <summary>Applies TotalFrames reduction. Returns false if the marker should be dropped.</summary>
    internal abstract bool ApplyFrameLimit(int newTotalFrames);
}
