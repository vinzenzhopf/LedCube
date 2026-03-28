using System.Windows;

namespace LedCube.Core.UI.TimelineControl;

public class PlayheadChangedEventArgs : RoutedEventArgs
{
    public int OldFrame { get; }
    public int NewFrame { get; }

    public PlayheadChangedEventArgs(RoutedEvent routedEvent, int oldFrame, int newFrame)
        : base(routedEvent)
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

    public SelectionChangedEventArgs(RoutedEvent routedEvent, int? oldStart, int? oldEnd, int? newStart, int? newEnd)
        : base(routedEvent)
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

    public MarkerDragStartedEventArgs(RoutedEvent routedEvent, MarkerBase marker)
        : base(routedEvent)
    {
        Marker = marker;
    }
}

public class MarkerDraggingEventArgs : RoutedEventArgs
{
    public MarkerBase Marker { get; }
    public int CurrentFrame { get; }

    public MarkerDraggingEventArgs(RoutedEvent routedEvent, MarkerBase marker, int currentFrame)
        : base(routedEvent)
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

    public MarkerDragCompletedEventArgs(RoutedEvent routedEvent, MarkerBase marker,
        int oldStartFrame, int oldEndFrame, int newStartFrame, int newEndFrame)
        : base(routedEvent)
    {
        Marker = marker;
        OldStartFrame = oldStartFrame;
        OldEndFrame = oldEndFrame;
        NewStartFrame = newStartFrame;
        NewEndFrame = newEndFrame;
    }

    public static MarkerDragCompletedEventArgs ForPoint(RoutedEvent routedEvent, PointMarker marker, int oldFrame, int newFrame)
        => new(routedEvent, marker, oldFrame, -1, newFrame, -1);

    public static MarkerDragCompletedEventArgs ForRange(RoutedEvent routedEvent, RangeMarker marker,
        int oldStart, int oldEnd, int newStart, int newEnd)
        => new(routedEvent, marker, oldStart, oldEnd, newStart, newEnd);
}
