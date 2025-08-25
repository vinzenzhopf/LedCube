using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LedCube.Core.UI.TimelineControl;

public partial class Timeline : UserControl
{
    // Public API / Dependency Properties
    public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
        nameof(Scale), typeof(double), typeof(Timeline),
        new PropertyMetadata(1.0, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty StartValueProperty = DependencyProperty.Register(
        nameof(StartValue), typeof(int), typeof(Timeline),
        new PropertyMetadata(0, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty EndValueProperty = DependencyProperty.Register(
        nameof(EndValue), typeof(int), typeof(Timeline),
        new PropertyMetadata(100, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty CursorPositionProperty = DependencyProperty.Register(
        nameof(CursorPosition), typeof(int), typeof(Timeline),
        new PropertyMetadata(0, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty TickMarksMajorProperty = DependencyProperty.Register(
        nameof(TickMarksMajor), typeof(int), typeof(Timeline),
        new PropertyMetadata(10, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty TickMarksMinorProperty = DependencyProperty.Register(
        nameof(TickMarksMinor), typeof(int), typeof(Timeline),
        new PropertyMetadata(1, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(Timeline),
        new FrameworkPropertyMetadata(false));

    // Optional second marker for range selection
    public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
        nameof(SelectionStart), typeof(int?), typeof(Timeline),
        new FrameworkPropertyMetadata(null, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
        nameof(SelectionEnd), typeof(int?), typeof(Timeline),
        new FrameworkPropertyMetadata(null, OnAnyVisualPropertyChanged));

    public static readonly DependencyProperty FramesPerSecondProperty = DependencyProperty.Register(
        nameof(FramesPerSecond), typeof(double), typeof(Timeline),
        new FrameworkPropertyMetadata(30.0, OnAnyVisualPropertyChanged));


    // Visual parts
    private ScrollViewer? _scroll;
    private Canvas? _canvas;
    private Canvas? _legend;
    private Canvas? _markers;
    private Canvas? _overlay; // captures input

    // constants
    private const double LegendHeight = 20;
    private const double TrackHeight = 60;

    // cache for interaction
    private bool _isDragging;
    private bool _isRangeDrag;

    public Timeline()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    // Public CLR wrappers
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public int StartValue
    {
        get => (int)GetValue(StartValueProperty);
        set => SetValue(StartValueProperty, value);
    }

    public int EndValue
    {
        get => (int)GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    public int CursorPosition
    {
        get => (int)GetValue(CursorPositionProperty);
        set => SetValue(CursorPositionProperty, value);
    }

    public int TickMarksMajor
    {
        get => (int)GetValue(TickMarksMajorProperty);
        set => SetValue(TickMarksMajorProperty, value);
    }

    public int TickMarksMinor
    {
        get => (int)GetValue(TickMarksMinorProperty);
        set => SetValue(TickMarksMinorProperty, value);
    }

    public double FramesPerSecond
    {
        get => (double)GetValue(FramesPerSecondProperty);
        set => SetValue(FramesPerSecondProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public int? SelectionStart
    {
        get => (int?)GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }

    public int? SelectionEnd
    {
        get => (int?)GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    // Derived values
    private double PixelsPerFrame => Math.Max(Scale * 10.0, 2.0);
    private double MinorTickSpacingPx => PixelsPerFrame * TickMarksMinor;
    private double MajorTickSpacingPx => PixelsPerFrame * TickMarksMajor;
    private int FrameCount => Math.Max(0, EndValue - StartValue + 1);

    private (int first, int last) GetVisibleFrameRange()
    {
        double totalWidth = (FrameCount) * PixelsPerFrame;
        double offset = _scroll?.HorizontalOffset ?? 0.0;
        double viewport = _scroll?.ViewportWidth ?? ActualWidth;
        if (viewport <= 0) viewport = ActualWidth > 0 ? ActualWidth : 400;
        double buffer = Math.Max(100, viewport * 0.5);
        double leftPx = Math.Max(0, offset - buffer);
        double rightPx = Math.Min(totalWidth, offset + viewport + buffer);
        int first = StartValue + (int)Math.Floor(leftPx / PixelsPerFrame);
        int last = StartValue + (int)Math.Ceiling(rightPx / PixelsPerFrame);
        first = Math.Clamp(first, StartValue, EndValue);
        last = Math.Clamp(last, StartValue, EndValue);
        if (first > last) first = last;
        return (first, last);
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return ts.ToString(@"hh\:mm\:ss\.fff");
    }

    private static void OnAnyVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Timeline tl)
        {
            tl.UpdateLayoutAndRedraw();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _scroll = PART_ScrollViewer;
        _canvas = PART_TimelineCanvas;
        _legend = PART_LegendCanvas;
        _markers = PART_MarkersCanvas;
        _overlay = PART_OverlayCanvas;

        if (_scroll != null)
        {
            _scroll.ScrollChanged += ScrollOnScrollChanged;
        }

        if (_overlay != null)
        {
            _overlay.MouseWheel += OverlayOnMouseWheel;
            _overlay.MouseLeftButtonDown += OverlayOnMouseLeftButtonDown;
            _overlay.MouseMove += OverlayOnMouseMove;
            _overlay.MouseLeftButtonUp += OverlayOnMouseLeftButtonUp;
            _overlay.MouseRightButtonDown += OverlayOnMouseRightButtonDown;
            _overlay.MouseLeave += OverlayOnMouseLeave;
            _overlay.KeyDown += OverlayOnKeyDown;
        }

        UpdateLayoutAndRedraw();
    }

    private void UpdateLayoutAndRedraw()
    {
        if (_canvas == null || _legend == null || _markers == null) return;
        var width = Math.Max(ActualWidth, FrameCount * PixelsPerFrame);
        _canvas.Width = width;
        _markers.Width = width;
        _overlay!.Width = width;
        // Legend should match viewport width (not content width), to stay in sync with scroll without heavy layout
        double legendWidth = (_scroll?.ViewportWidth ?? 0) > 0 ? _scroll!.ViewportWidth : ActualWidth;
        _legend.Width = legendWidth;
        _canvas.Height = TrackHeight;
        _markers.Height = TrackHeight;
        _overlay.Height = TrackHeight;

        RedrawTicks();
        RedrawLegend();
        RedrawMarkers();
    }

    private void RedrawTicks()
    {
        if (_canvas == null) return;
        _canvas.Children.Clear();
        var penMinor = new SolidColorBrush(Color.FromRgb(200, 200, 200));
        var penMajor = new SolidColorBrush(Color.FromRgb(150, 150, 150));

        // draw baseline
        var baseline = new Rectangle { Width = _canvas.Width, Height = 1, Fill = penMajor, VerticalAlignment = VerticalAlignment.Bottom };
        Canvas.SetTop(baseline, TrackHeight - 1);
        Canvas.SetLeft(baseline, 0);
        _canvas.Children.Add(baseline);

        // visible range
        var (first, last) = GetVisibleFrameRange();
        int minorStep = Math.Max(1, TickMarksMinor);
        // align start to minor step relative to absolute 0
        int start = first;
        int rem = start % minorStep;
        if (rem != 0) start += (minorStep - rem);

        for (int frame = start; frame <= last; frame += minorStep)
        {
            double x = (frame - StartValue) * PixelsPerFrame;
            bool isMajor = (frame % Math.Max(1, TickMarksMajor)) == 0;
            var height = isMajor ? 18 : 10;
            var brush = isMajor ? penMajor : penMinor;
            var rect = new Rectangle { Width = 1, Height = height, Fill = brush };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, TrackHeight - height);
            _canvas.Children.Add(rect);
        }
    }

    private void RedrawLegend()
    {
        if (_legend == null) return;
        _legend.Children.Clear();
        var textBrush = Brushes.Gray;

        // Visible range for major labels
        var (first, last) = GetVisibleFrameRange();
        int step = Math.Max(1, TickMarksMajor);
        int start = first - (first % step);
        if (start < first) start += step; // ensure >= first

        for (int frame = start; frame <= last; frame += step)
        {
            double x = (frame - StartValue) * PixelsPerFrame;
            double offset = _scroll?.HorizontalOffset ?? 0.0;
            var tb = new TextBlock
            {
                Text = frame.ToString(),
                Foreground = textBrush,
                FontSize = 10
            };
            _legend.Children.Add(tb);
            Canvas.SetLeft(tb, x - offset + 2);
            Canvas.SetTop(tb, 2);
        }
    }

    private void RedrawMarkers()
    {
        if (_markers == null) return;
        _markers.Children.Clear();

        void AddMarker(int frame, Brush brush)
        {
            double x = (frame - StartValue) * PixelsPerFrame;
            var line = new Rectangle { Width = 2, Height = TrackHeight, Fill = brush, Opacity = 0.9 };
            Canvas.SetLeft(line, x - 1);
            Canvas.SetTop(line, 0);
            _markers.Children.Add(line);
        }

        // range selection fill
        if (SelectionStart.HasValue && SelectionEnd.HasValue)
        {
            int a = Math.Min(SelectionStart.Value, SelectionEnd.Value);
            int b = Math.Max(SelectionStart.Value, SelectionEnd.Value);
            double x = (a - StartValue) * PixelsPerFrame;
            double w = (b - a) * PixelsPerFrame;
            var fill = new Rectangle { Width = w, Height = TrackHeight, Fill = new SolidColorBrush(Color.FromArgb(40, 30, 144, 255)) };
            Canvas.SetLeft(fill, x);
            Canvas.SetTop(fill, 0);
            _markers.Children.Add(fill);
        }

        // cursor marker (only if no active range selection)
        if (!(SelectionStart.HasValue || SelectionEnd.HasValue))
        {
            AddMarker(CursorPosition, Brushes.CornflowerBlue);
        }

        // secondary markers at selection edges
        if (SelectionStart.HasValue) AddMarker(SelectionStart.Value, Brushes.DodgerBlue);
        if (SelectionEnd.HasValue) AddMarker(SelectionEnd.Value, Brushes.DodgerBlue);
    }

    // Interaction & Input
    private void OverlayOnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_scroll == null) return;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            // zoom around mouse position
            var pos = e.GetPosition(_overlay);
            double oldPxPerFrame = PixelsPerFrame;
            Scale = Math.Clamp(Scale + (e.Delta > 0 ? 0.1 : -0.1), 0.1, 50);
            UpdateLayoutAndRedraw();
            // keep mouse-centered zoom
            double newPxPerFrame = PixelsPerFrame;
            double frameAtMouse = StartValue + pos.X / newPxPerFrame;
            double targetOffset = Math.Max(0, (frameAtMouse - StartValue) * newPxPerFrame - pos.X);
            _scroll.ScrollToHorizontalOffset(targetOffset);
        }
        else
        {
            // scroll
            double delta = e.Delta > 0 ? -60 : 60;
            _scroll.ScrollToHorizontalOffset(Math.Max(0, _scroll.HorizontalOffset + delta));
        }
    }

    private void OverlayOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsReadOnly) return;
        _overlay?.CaptureMouse();
        _overlay?.Focus();
        _isDragging = true;
        _isRangeDrag = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);
        if (!_isRangeDrag)
        {
            // Normal click should clear any existing range selection
            if (SelectionStart.HasValue || SelectionEnd.HasValue)
            {
                SelectionStart = null;
                SelectionEnd = null;
            }
        }
        UpdatePositionFromMouse(e.GetPosition(_overlay));
    }

    private void OverlayOnMouseMove(object sender, MouseEventArgs e)
    {
        if (_overlay == null) return;
        var pos = e.GetPosition(_overlay);

        // Update tooltip regardless of drag state
        int frameAtMouse = Math.Clamp(SnapPixelToFrame(pos.X), StartValue, EndValue);
        double seconds = frameAtMouse / Math.Max(0.0001, FramesPerSecond);
        _overlay.ToolTip = $"Frame {frameAtMouse} — {FormatTime(seconds)}";

        // Only update selection/markers while dragging
        if (_isDragging)
        {
            UpdatePositionFromMouse(pos);
        }
    }

    private void OverlayOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        _isDragging = false;
        _overlay?.ReleaseMouseCapture();
    }

    private void OverlayOnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // remove secondary markers
        SelectionStart = null;
        SelectionEnd = null;
        UpdateLayoutAndRedraw();
    }

    private void ScrollOnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // Redraw only ticks and legend on scroll/viewport changes
        RedrawTicks();
        RedrawLegend();
    }

    private void OverlayOnMouseLeave(object sender, MouseEventArgs e)
    {
        if (_overlay != null)
        {
            _overlay.ToolTip = null;
        }
    }

    private void UpdatePositionFromMouse(Point p)
    {
        if (_overlay == null) return;
        int frame = SnapPixelToFrame(p.X);
        frame = Math.Clamp(frame, StartValue, EndValue);

        if (_isRangeDrag)
        {
            // Start a range: first click sets SelectionStart and SelectionEnd to the current frame
            if (SelectionStart == null)
            {
                SelectionStart = frame;
                SelectionEnd = frame;
            }
            else
            {
                SelectionEnd = frame;
            }
        }
        else
        {
            CursorPosition = frame;
        }

        RedrawMarkers();
    }

    private int SnapPixelToFrame(double x)
    {
        // snap to nearest minor tick (i.e., each frame)
        double frameDouble = StartValue + x / PixelsPerFrame;
        int frame = (int)Math.Round(frameDouble);
        return frame;
    }

    private void OverlayOnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete || e.Key == Key.Escape)
        {
            SelectionStart = null;
            SelectionEnd = null;
            UpdateLayoutAndRedraw();
        }
    }
}