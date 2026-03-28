using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;

namespace LedCube.Core.UI.TimelineControl.Demo;

[ObservableObject]
public partial class MainWindow : Window
{
    [ObservableProperty] private TimelineMode _mode = TimelineMode.Edit;
    [ObservableProperty] private int _totalFrames = 200;
    [ObservableProperty] private int _currentFrame = 0;
    [ObservableProperty] private bool _loopEnabled = false;
    [ObservableProperty] private int _loopStart = 20;
    [ObservableProperty] private int _loopEnd = 80;
    [ObservableProperty] private bool _showFrameTime = false;
    [ObservableProperty] private int _modeIndex = 0;

    public TimeSpan? FrameTime => ShowFrameTime ? TimeSpan.FromMilliseconds(33.3) : null;

    public MainWindow()
    {
        InitializeComponent();

        // Add some example markers
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

    partial void OnModeIndexChanged(int value) =>
        Mode = value == 0 ? TimelineMode.Edit : TimelineMode.Live;

    partial void OnShowFrameTimeChanged(bool value) =>
        OnPropertyChanged(nameof(FrameTime));

    private void AddPointMarker_Click(object sender, RoutedEventArgs e)
    {
        Timeline.Markers.Add(new PointMarker
        {
            Frame = CurrentFrame,
            Label = $"M{Timeline.Markers.Count + 1}",
            Color = SKColors.DeepSkyBlue,
            IsDraggable = true,
            ClampBehavior = ClampBehavior.Clamp
        });
    }

    private void AddRangeMarker_Click(object sender, RoutedEventArgs e)
    {
        var start = Timeline.SelectionStart ?? CurrentFrame;
        var end = Timeline.SelectionEnd ?? Math.Min(CurrentFrame + 20, TotalFrames - 1);
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

    private void ClearMarkers_Click(object sender, RoutedEventArgs e) =>
        Timeline.Markers.Clear();

    private void Timeline_PlayheadChanged(object sender, PlayheadChangedEventArgs e) =>
        StatusText.Text = $"Playhead: frame {e.NewFrame} (was {e.OldFrame})";

    private void Timeline_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        StatusText.Text = e.NewStart.HasValue
            ? $"Selection: {e.NewStart} – {e.NewEnd}"
            : "Selection cleared";

    private void Timeline_MarkerDragCompleted(object sender, MarkerDragCompletedEventArgs e) =>
        StatusText.Text = $"Marker moved: {e.OldFrame} → {e.NewFrame}  ({e.Marker.Label})";
}
