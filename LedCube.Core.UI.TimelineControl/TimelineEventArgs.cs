using Avalonia.Interactivity;

namespace LedCube.Core.UI.TimelineControl;

public class PlayheadChangedEventArgs : RoutedEventArgs
{
    public int OldFrame { get; }
    public int NewFrame { get; }

    public PlayheadChangedEventArgs(int oldFrame, int newFrame)
    {
        OldFrame = oldFrame;
        NewFrame = newFrame;
    }
}

public class SelectionChangedEventArgs : RoutedEventArgs
{
    public int? OldStart { get; }
    public int? OldEnd { get; }
    public int? NewStart { get; }
    public int? NewEnd { get; }

    public SelectionChangedEventArgs(int? oldStart, int? oldEnd, int? newStart, int? newEnd)
    {
        OldStart = oldStart;
        OldEnd = oldEnd;
        NewStart = newStart;
        NewEnd = newEnd;
    }
}

public class MarkerDragStartedEventArgs : RoutedEventArgs
{
    public MarkerBase Marker { get; }

    public MarkerDragStartedEventArgs(MarkerBase marker)
    {
        Marker = marker;
    }
}

public class MarkerDraggingEventArgs : RoutedEventArgs
{
    public MarkerBase Marker { get; }
    public int CurrentFrame { get; }

    public MarkerDraggingEventArgs(MarkerBase marker, int currentFrame)
    {
        Marker = marker;
        CurrentFrame = currentFrame;
    }
}

public class MarkerDragCompletedEventArgs : RoutedEventArgs
{
    public MarkerBase Marker { get; }

    /// <summary>Start frame before drag. For <see cref="PointMarker"/> this is the only relevant old frame value.</summary>
    public int OldStartFrame { get; }

    /// <summary>End frame before drag. -1 for <see cref="PointMarker"/>.</summary>
    public int OldEndFrame { get; }

    /// <summary>Start frame after drag.</summary>
    public int NewStartFrame { get; }

    /// <summary>End frame after drag. -1 for <see cref="PointMarker"/>.</summary>
    public int NewEndFrame { get; }

    // Convenience aliases for point markers
    public int OldFrame => OldStartFrame;
    public int NewFrame => NewStartFrame;

    public MarkerDragCompletedEventArgs(MarkerBase marker,
        int oldStartFrame, int oldEndFrame, int newStartFrame, int newEndFrame)
    {
        Marker = marker;
        OldStartFrame = oldStartFrame;
        OldEndFrame = oldEndFrame;
        NewStartFrame = newStartFrame;
        NewEndFrame = newEndFrame;
    }

    public static MarkerDragCompletedEventArgs ForPoint(PointMarker marker, int oldFrame, int newFrame)
        => new(marker, oldFrame, -1, newFrame, -1);

    public static MarkerDragCompletedEventArgs ForRange(RangeMarker marker,
        int oldStart, int oldEnd, int newStart, int newEnd)
        => new(marker, oldStart, oldEnd, newStart, newEnd);
}
