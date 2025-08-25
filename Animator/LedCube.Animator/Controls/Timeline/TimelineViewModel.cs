using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Animator.Controls.Timeline;

public partial class TimelineViewModel : ObservableObject
{
    // Timeline bounds and scale
    [ObservableProperty]
    private int frameCount = 240; // default frames

    [ObservableProperty]
    private double scale = 1.0; // inner control interprets UnitWidth = Scale * 10

    // Selection (in frames)
    [ObservableProperty]
    private int? selectedStart;

    [ObservableProperty]
    private int? selectedEnd;

    // Cursor (current frame)
    [ObservableProperty]
    private int cursorPosition;

    public bool HasRange => SelectedStart.HasValue && SelectedEnd.HasValue;
    public bool HasSelection => SelectedStart.HasValue || SelectedEnd.HasValue;
    public int StartValue => 0;
    public int EndValue => FrameCount;

    partial void OnFrameCountChanged(int value)
    {
        if (SelectedStart >= value) SelectedStart = null;
        if (SelectedEnd >= value) SelectedEnd = null;
        if (CursorPosition >= value) CursorPosition = value > 0 ? value - 1 : 0;
    }

    public void SetSingleSelection(int frame)
    {
        if (frame < 0) frame = 0;
        if (frame >= FrameCount) frame = FrameCount - 1;
        SelectedStart = frame;
        SelectedEnd = null;
        CursorPosition = frame;
    }

    public void SetRangeSelection(int a, int b)
    {
        if (FrameCount <= 0) return;
        var start = int.Max(0, int.Min(a, b));
        var end = int.Min(FrameCount - 1, int.Max(a, b));
        SelectedStart = start;
        SelectedEnd = end;
        CursorPosition = start;
    }

    public void ClearSelection()
    {
        SelectedStart = null;
        SelectedEnd = null;
    }
}