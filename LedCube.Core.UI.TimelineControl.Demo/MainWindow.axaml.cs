using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkiaSharp;

namespace LedCube.Core.UI.TimelineControl.Demo;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        // Add some example markers.
        Timeline.Markers.Add(new PointMarker
        {
            Frame = 10,
            Label = "Start",
            Color = SKColors.LimeGreen,
            IsDraggable = true,
            ClampBehavior = ClampBehavior.Clamp
        });
        Timeline.Markers.Add(new RangeMarker
        {
            StartFrame = 50,
            EndFrame = 90,
            Label = "Action",
            Color = new SKColor(255, 165, 0),
            IsDraggable = true,
            ClampBehavior = ClampBehavior.Clamp
        });
        Timeline.Markers.Add(new PointMarker
        {
            Frame = 150,
            Label = "End",
            Color = SKColors.OrangeRed,
            IsDraggable = true,
            ClampBehavior = ClampBehavior.Clamp
        });
    }

    private void AddPointMarker_Click(object? sender, RoutedEventArgs e)
    {
        Timeline.Markers.Add(new PointMarker
        {
            Frame = _vm.CurrentFrame,
            Label = $"M{Timeline.Markers.Count + 1}",
            Color = SKColors.DeepSkyBlue,
            IsDraggable = true,
            ClampBehavior = ClampBehavior.Clamp
        });
    }

    private void AddRangeMarker_Click(object? sender, RoutedEventArgs e)
    {
        var start = Timeline.SelectionStart ?? _vm.CurrentFrame;
        var end = Timeline.SelectionEnd ?? Math.Min(_vm.CurrentFrame + 20, _vm.TotalFrames - 1);
        if (start > end) (start, end) = (end, start);
        Timeline.Markers.Add(new RangeMarker
        {
            StartFrame = start,
            EndFrame = end,
            Label = $"R{Timeline.Markers.Count + 1}",
            Color = new SKColor(180, 100, 220),
            IsDraggable = true,
            ClampBehavior = ClampBehavior.Clamp
        });
    }

    private void ClearMarkers_Click(object? sender, RoutedEventArgs e) =>
        Timeline.Markers.Clear();

    private void Timeline_PlayheadChanged(object? sender, PlayheadChangedEventArgs e) =>
        StatusText.Text = $"Playhead: frame {e.NewFrame} (was {e.OldFrame})";

    private void Timeline_SelectionChanged(object? sender, SelectionChangedEventArgs e) =>
        StatusText.Text = e.NewStart.HasValue
            ? $"Selection: {e.NewStart} – {e.NewEnd}"
            : "Selection cleared";

    private void Timeline_MarkerDragCompleted(object? sender, MarkerDragCompletedEventArgs e) =>
        StatusText.Text = $"Marker moved: {e.OldFrame} → {e.NewFrame}  ({e.Marker.Label})";
}
