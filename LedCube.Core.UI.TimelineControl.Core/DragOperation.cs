namespace LedCube.Core.UI.TimelineControl;

public enum DragTarget
{
    Playhead,
    SelectionStart,
    SelectionEnd,
    LoopIn,
    LoopOut,
    MarkerPoint,
    MarkerRangeStart,
    MarkerRangeEnd,
    MarkerRangeBody
}

public class DragOperation
{
    public DragTarget Target { get; init; }
    public MarkerBase? Marker { get; init; }
    public int GhostFrame { get; set; }
}
