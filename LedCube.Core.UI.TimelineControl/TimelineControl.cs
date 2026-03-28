using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace LedCube.Core.UI.TimelineControl;

/// <summary>
/// SkiaSharp-based timeline control. TimelineState is the single source of truth;
/// DependencyProperties are a thin sync layer on top.
/// </summary>
public class TimelineControl : System.Windows.Controls.Control
{
    // ──────────────────────────────────────────────────────────────────────────
    // Static ctor / routed events / dependency properties
    // ──────────────────────────────────────────────────────────────────────────

    static TimelineControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(TimelineControl),
            new FrameworkPropertyMetadata(typeof(TimelineControl)));
    }

    #region Routed Events

    public static readonly RoutedEvent PlayheadChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(PlayheadChanged), RoutingStrategy.Bubble, typeof(EventHandler<PlayheadChangedEventArgs>), typeof(TimelineControl));

    public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(EventHandler<SelectionChangedEventArgs>), typeof(TimelineControl));

    public static readonly RoutedEvent MarkerDragStartedEvent = EventManager.RegisterRoutedEvent(
        nameof(MarkerDragStarted), RoutingStrategy.Bubble, typeof(EventHandler<MarkerDragStartedEventArgs>), typeof(TimelineControl));

    public static readonly RoutedEvent MarkerDraggingEvent = EventManager.RegisterRoutedEvent(
        nameof(MarkerDragging), RoutingStrategy.Bubble, typeof(EventHandler<MarkerDraggingEventArgs>), typeof(TimelineControl));

    public static readonly RoutedEvent MarkerDragCompletedEvent = EventManager.RegisterRoutedEvent(
        nameof(MarkerDragCompleted), RoutingStrategy.Bubble, typeof(EventHandler<MarkerDragCompletedEventArgs>), typeof(TimelineControl));

    public event EventHandler<PlayheadChangedEventArgs> PlayheadChanged
    {
        add => AddHandler(PlayheadChangedEvent, value);
        remove => RemoveHandler(PlayheadChangedEvent, value);
    }

    public event EventHandler<SelectionChangedEventArgs> SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public event EventHandler<MarkerDragStartedEventArgs> MarkerDragStarted
    {
        add => AddHandler(MarkerDragStartedEvent, value);
        remove => RemoveHandler(MarkerDragStartedEvent, value);
    }

    public event EventHandler<MarkerDraggingEventArgs> MarkerDragging
    {
        add => AddHandler(MarkerDraggingEvent, value);
        remove => RemoveHandler(MarkerDraggingEvent, value);
    }

    public event EventHandler<MarkerDragCompletedEventArgs> MarkerDragCompleted
    {
        add => AddHandler(MarkerDragCompletedEvent, value);
        remove => RemoveHandler(MarkerDragCompletedEvent, value);
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
        nameof(Mode), typeof(TimelineMode), typeof(TimelineControl),
        new PropertyMetadata(TimelineMode.Edit, OnModeChanged));

    public static readonly DependencyProperty TotalFramesProperty = DependencyProperty.Register(
        nameof(TotalFrames), typeof(int), typeof(TimelineControl),
        new PropertyMetadata(100, OnTotalFramesChanged));

    public static readonly DependencyProperty FrameTimeProperty = DependencyProperty.Register(
        nameof(FrameTime), typeof(TimeSpan?), typeof(TimelineControl),
        new PropertyMetadata(null, OnFrameTimeChanged));

    public static readonly DependencyProperty CurrentFrameProperty = DependencyProperty.Register(
        nameof(CurrentFrame), typeof(int), typeof(TimelineControl),
        new PropertyMetadata(0, OnCurrentFrameChanged));

    public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
        nameof(SelectionStart), typeof(int?), typeof(TimelineControl),
        new PropertyMetadata(null, OnSelectionStartChanged));

    public static readonly DependencyProperty SelectionEndProperty = DependencyProperty.Register(
        nameof(SelectionEnd), typeof(int?), typeof(TimelineControl),
        new PropertyMetadata(null, OnSelectionEndChanged));

    public static readonly DependencyProperty LoopStartProperty = DependencyProperty.Register(
        nameof(LoopStart), typeof(int?), typeof(TimelineControl),
        new PropertyMetadata(null, OnLoopStartChanged));

    public static readonly DependencyProperty LoopEndProperty = DependencyProperty.Register(
        nameof(LoopEnd), typeof(int?), typeof(TimelineControl),
        new PropertyMetadata(null, OnLoopEndChanged));

    public static readonly DependencyProperty LoopEnabledProperty = DependencyProperty.Register(
        nameof(LoopEnabled), typeof(bool), typeof(TimelineControl),
        new PropertyMetadata(false, OnLoopEnabledChanged));

    public static readonly DependencyProperty MarkersProperty = DependencyProperty.Register(
        nameof(Markers), typeof(ObservableCollection<MarkerBase>), typeof(TimelineControl),
        new PropertyMetadata(null, OnMarkersChanged));

    public TimelineMode Mode
    {
        get => (TimelineMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public int TotalFrames
    {
        get => (int)GetValue(TotalFramesProperty);
        set => SetValue(TotalFramesProperty, value);
    }

    public TimeSpan? FrameTime
    {
        get => (TimeSpan?)GetValue(FrameTimeProperty);
        set => SetValue(FrameTimeProperty, value);
    }

    public int CurrentFrame
    {
        get => (int)GetValue(CurrentFrameProperty);
        set => SetValue(CurrentFrameProperty, value);
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

    public int? LoopStart
    {
        get => (int?)GetValue(LoopStartProperty);
        set => SetValue(LoopStartProperty, value);
    }

    public int? LoopEnd
    {
        get => (int?)GetValue(LoopEndProperty);
        set => SetValue(LoopEndProperty, value);
    }

    public bool LoopEnabled
    {
        get => (bool)GetValue(LoopEnabledProperty);
        set => SetValue(LoopEnabledProperty, value);
    }

    public ObservableCollection<MarkerBase> Markers
    {
        get => (ObservableCollection<MarkerBase>)GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    #endregion

    #region DP Callbacks

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.Mode = (TimelineMode)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnTotalFramesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.TotalFrames = (int)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnFrameTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.FrameTime = (TimeSpan?)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnCurrentFrameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.CurrentFrame = (int)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl.AutoScrollToPlayheadIfLive();
        ctrl._element?.InvalidateVisual();
    }

    private static void OnSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.SelectionStart = (int?)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnSelectionEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.SelectionEnd = (int?)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnLoopStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.LoopStart = (int?)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnLoopEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.LoopEnd = (int?)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnLoopEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (ctrl._isSyncingState) return;
        ctrl._isSyncingState = true;
        ctrl._state.LoopEnabled = (bool)e.NewValue;
        ctrl._isSyncingState = false;
        ctrl._element?.InvalidateVisual();
    }

    private static void OnMarkersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = (TimelineControl)d;
        if (e.OldValue is ObservableCollection<MarkerBase> old)
            old.CollectionChanged -= ctrl.OnMarkersCollectionChanged;

        if (e.NewValue is ObservableCollection<MarkerBase> newCol)
        {
            newCol.CollectionChanged += ctrl.OnMarkersCollectionChanged;
            // Sync state markers to match
            ctrl._state.Markers.Clear();
            foreach (var m in newCol)
                ctrl._state.Markers.Add(m);
        }

        ctrl._element?.InvalidateVisual();
    }

    private void OnMarkersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Mirror external collection into state's collection
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null)
                    foreach (MarkerBase m in e.NewItems)
                        _state.Markers.Add(m);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is not null)
                    foreach (MarkerBase m in e.OldItems)
                        _state.Markers.Remove(m);
                break;
            case NotifyCollectionChangedAction.Reset:
                _state.Markers.Clear();
                if (Markers is not null)
                    foreach (var m in Markers)
                        _state.Markers.Add(m);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems is not null)
                    foreach (MarkerBase m in e.OldItems)
                        _state.Markers.Remove(m);
                if (e.NewItems is not null)
                    foreach (MarkerBase m in e.NewItems)
                        _state.Markers.Add(m);
                break;
            case NotifyCollectionChangedAction.Move:
                // Order doesn't affect rendering meaningfully
                break;
        }
        _element?.InvalidateVisual();
    }

    #endregion

    // ──────────────────────────────────────────────────────────────────────────
    // Instance state
    // ──────────────────────────────────────────────────────────────────────────

    private readonly TimelineState _state = new();
    private SKElement? _element;
    private RenderResources? _resources;
    private DragOperation? _activeDrag;
    private bool _isSyncingState;

    // Saved state at drag start for event raising
    private int _dragStartFrame;
    private int _dragStartEndFrame; // for range markers

    // Selection state at start of a selection drag
    private int? _selectionDragAnchor;

    public TimelineControl()
    {
        Focusable = true;

        _state.PropertyChanged += OnStatePropertyChanged;

        // Initialize default markers collection
        var defaultMarkers = new ObservableCollection<MarkerBase>();
        SetValue(MarkersProperty, defaultMarkers);

        _element = new SKElement();
        _element.PaintSurface += OnPaintSurface;

        // Host the SKElement as a visual child via AddVisualChild
        AddVisualChild(_element);
        AddLogicalChild(_element);

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Visual children override (Control has no Panel, so we manage visuals)
    // ──────────────────────────────────────────────────────────────────────────

    protected override int VisualChildrenCount => 1;

    protected override System.Windows.Media.Visual GetVisualChild(int index)
    {
        if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
        return _element!;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Layout
    // ──────────────────────────────────────────────────────────────────────────

    private static readonly double ControlHeight = TimelineRenderer.RulerHeight + TimelineRenderer.TrackHeight;

    protected override Size MeasureOverride(Size availableSize)
    {
        var desired = new Size(
            double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
            ControlHeight);
        _element?.Measure(desired);
        return desired;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = new Rect(0, 0, finalSize.Width, ControlHeight);
        _element?.Arrange(rect);
        return new Size(finalSize.Width, ControlHeight);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _resources = new RenderResources();
        _element?.InvalidateVisual();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _resources?.Dispose();
        _resources = null;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State → DP sync
    // ──────────────────────────────────────────────────────────────────────────

    private void OnStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_isSyncingState) return;
        _isSyncingState = true;

        switch (e.PropertyName)
        {
            case nameof(TimelineState.Mode):
                SetValue(ModeProperty, _state.Mode);
                break;
            case nameof(TimelineState.TotalFrames):
                SetValue(TotalFramesProperty, _state.TotalFrames);
                break;
            case nameof(TimelineState.FrameTime):
                SetValue(FrameTimeProperty, _state.FrameTime);
                break;
            case nameof(TimelineState.CurrentFrame):
                var oldFrame = (int)GetValue(CurrentFrameProperty);
                SetValue(CurrentFrameProperty, _state.CurrentFrame);
                if (oldFrame != _state.CurrentFrame)
                {
                    RaiseEvent(new PlayheadChangedEventArgs(PlayheadChangedEvent, oldFrame, _state.CurrentFrame));
                    AutoScrollToPlayheadIfLive();
                }
                break;
            case nameof(TimelineState.SelectionStart):
                SetValue(SelectionStartProperty, _state.SelectionStart);
                break;
            case nameof(TimelineState.SelectionEnd):
                SetValue(SelectionEndProperty, _state.SelectionEnd);
                break;
            case nameof(TimelineState.LoopStart):
                SetValue(LoopStartProperty, _state.LoopStart);
                break;
            case nameof(TimelineState.LoopEnd):
                SetValue(LoopEndProperty, _state.LoopEnd);
                break;
            case nameof(TimelineState.LoopEnabled):
                SetValue(LoopEnabledProperty, _state.LoopEnabled);
                break;
        }

        _isSyncingState = false;
        _element?.InvalidateVisual();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Paint
    // ──────────────────────────────────────────────────────────────────────────

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        if (_resources is null) return;
        var canvas = e.Surface.Canvas;
        canvas.Clear();
        var layout = _state.BuildLayout(e.Info.Width);
        TimelineRenderer.Draw(canvas, layout, _state, _activeDrag, _resources);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Keyboard
    // ──────────────────────────────────────────────────────────────────────────

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_state.Mode != TimelineMode.Edit) return;

        switch (e.Key)
        {
            case Key.OemComma:
                _state.CurrentFrame--;
                e.Handled = true;
                break;
            case Key.OemPeriod:
                _state.CurrentFrame++;
                e.Handled = true;
                break;
            case Key.Home:
                _state.CurrentFrame = 0;
                e.Handled = true;
                break;
            case Key.End:
                _state.CurrentFrame = _state.TotalFrames - 1;
                e.Handled = true;
                break;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mouse — scroll & zoom (all modes)
    // ──────────────────────────────────────────────────────────────────────────

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            // Zoom centred on cursor
            var layout = _state.BuildLayout(ActualWidth);
            double mouseX = e.GetPosition(this).X;
            int frameUnderCursor = layout.PixelToFrame(mouseX);

            double zoomDelta = e.Delta > 0 ? 1.2 : 1.0 / 1.2;
            double newZoom = Math.Clamp(_state.ZoomScale * zoomDelta, 1.0, 100.0);

            // Compute new scroll so that frameUnderCursor stays at mouseX
            double newScroll = frameUnderCursor * newZoom - mouseX;

            _state.ZoomScale = newZoom;
            _state.ScrollOffsetPx = Math.Max(0, newScroll);
        }
        else
        {
            // Horizontal scroll
            double delta = e.Delta > 0 ? -30.0 : 30.0;
            _state.ScrollOffsetPx = Math.Max(0, _state.ScrollOffsetPx + delta);
        }

        _element?.InvalidateVisual();
        e.Handled = true;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Mouse — drag interaction (Edit mode only)
    // ──────────────────────────────────────────────────────────────────────────

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Focus();

        if (_state.Mode != TimelineMode.Edit) return;

        var pos = e.GetPosition(this);
        var layout = _state.BuildLayout(ActualWidth);
        int frame = layout.PixelToFrame(pos.X);
        bool inRuler = pos.Y < TimelineRenderer.RulerHeight;

        // 1. Check loop handles
        if (_state.LoopStart.HasValue)
        {
            double loopInPx = layout.FrameToPixel(_state.LoopStart.Value);
            if (Math.Abs(pos.X - loopInPx) <= 5.0)
            {
                _activeDrag = new DragOperation { Target = DragTarget.LoopIn, GhostFrame = _state.LoopStart.Value };
                _dragStartFrame = _state.LoopStart.Value;
                Mouse.Capture(this);
                e.Handled = true;
                return;
            }
        }

        if (_state.LoopEnd.HasValue)
        {
            double loopOutPx = layout.FrameToPixel(_state.LoopEnd.Value);
            if (Math.Abs(pos.X - loopOutPx) <= 5.0)
            {
                _activeDrag = new DragOperation { Target = DragTarget.LoopOut, GhostFrame = _state.LoopEnd.Value };
                _dragStartFrame = _state.LoopEnd.Value;
                Mouse.Capture(this);
                e.Handled = true;
                return;
            }
        }

        // 2. Check draggable markers
        foreach (var marker in _state.Markers)
        {
            if (!marker.IsDraggable) continue;

            if (marker is PointMarker point)
            {
                double px = layout.FrameToPixel(point.Frame);
                if (Math.Abs(pos.X - px) <= 5.0)
                {
                    _activeDrag = new DragOperation { Target = DragTarget.MarkerPoint, Marker = marker, GhostFrame = point.Frame };
                    _dragStartFrame = point.Frame;
                    _dragStartEndFrame = -1;
                    Mouse.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(MarkerDragStartedEvent, marker));
                    e.Handled = true;
                    return;
                }
            }
            else if (marker is RangeMarker range)
            {
                double startPx = layout.FrameToPixel(range.StartFrame);
                double endPx = layout.FrameToPixel(range.EndFrame);

                if (Math.Abs(pos.X - startPx) <= 5.0)
                {
                    _activeDrag = new DragOperation { Target = DragTarget.MarkerRangeStart, Marker = marker, GhostFrame = range.StartFrame };
                    _dragStartFrame = range.StartFrame;
                    _dragStartEndFrame = range.EndFrame;
                    Mouse.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(MarkerDragStartedEvent, marker));
                    e.Handled = true;
                    return;
                }

                if (Math.Abs(pos.X - endPx) <= 5.0)
                {
                    _activeDrag = new DragOperation { Target = DragTarget.MarkerRangeEnd, Marker = marker, GhostFrame = range.EndFrame };
                    _dragStartFrame = range.StartFrame;
                    _dragStartEndFrame = range.EndFrame;
                    Mouse.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(MarkerDragStartedEvent, marker));
                    e.Handled = true;
                    return;
                }

                // Body drag: inside the range
                if (pos.X > startPx && pos.X < endPx)
                {
                    _activeDrag = new DragOperation { Target = DragTarget.MarkerRangeBody, Marker = marker, GhostFrame = frame };
                    _dragStartFrame = range.StartFrame;
                    _dragStartEndFrame = range.EndFrame;
                    Mouse.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(MarkerDragStartedEvent, marker));
                    e.Handled = true;
                    return;
                }
            }
        }

        // 3. Ruler zone → move playhead, clear selection
        if (inRuler)
        {
            int oldFrame = _state.CurrentFrame;
            _state.CurrentFrame = frame;
            _state.SelectionStart = null;
            _state.SelectionEnd = null;
            _selectionDragAnchor = null;
            if (oldFrame != frame)
                RaiseEvent(new PlayheadChangedEventArgs(PlayheadChangedEvent, oldFrame, frame));

            _activeDrag = new DragOperation { Target = DragTarget.Playhead, GhostFrame = frame };
            _dragStartFrame = frame;
            Mouse.Capture(this);
            e.Handled = true;
            return;
        }

        // 4. Track zone → start selection drag
        var oldSelStart = _state.SelectionStart;
        var oldSelEnd = _state.SelectionEnd;
        _state.SelectionStart = frame;
        _state.SelectionEnd = frame;
        _selectionDragAnchor = frame;

        if (oldSelStart != frame || oldSelEnd != frame)
            RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, oldSelStart, oldSelEnd, frame, frame));

        _activeDrag = new DragOperation { Target = DragTarget.SelectionEnd, GhostFrame = frame };
        _dragStartFrame = frame;
        Mouse.Capture(this);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_activeDrag is null) return;

        var pos = e.GetPosition(this);
        var layout = _state.BuildLayout(ActualWidth);
        int frame = layout.PixelToFrame(pos.X);

        _activeDrag.GhostFrame = frame;

        // For live feedback during drags, update state without committing
        switch (_activeDrag.Target)
        {
            case DragTarget.Playhead:
                // Update playhead live during ruler drag
                int oldFrame = _state.CurrentFrame;
                _state.CurrentFrame = frame;
                if (oldFrame != frame)
                    RaiseEvent(new PlayheadChangedEventArgs(PlayheadChangedEvent, oldFrame, frame));
                break;

            case DragTarget.SelectionEnd:
                if (_selectionDragAnchor.HasValue)
                {
                    var oldStart = _state.SelectionStart;
                    var oldEnd = _state.SelectionEnd;
                    int anchor = _selectionDragAnchor.Value;
                    _state.SelectionStart = Math.Min(anchor, frame);
                    _state.SelectionEnd = Math.Max(anchor, frame);
                    if (oldStart != _state.SelectionStart || oldEnd != _state.SelectionEnd)
                        RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, oldStart, oldEnd, _state.SelectionStart, _state.SelectionEnd));
                }
                break;

            case DragTarget.MarkerPoint:
            case DragTarget.MarkerRangeStart:
            case DragTarget.MarkerRangeEnd:
            case DragTarget.MarkerRangeBody:
                if (_activeDrag.Marker is not null)
                    RaiseEvent(new MarkerDraggingEventArgs(MarkerDraggingEvent, _activeDrag.Marker, frame));
                break;
        }

        _element?.InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (_activeDrag is null) return;

        var pos = e.GetPosition(this);
        var layout = _state.BuildLayout(ActualWidth);
        int frame = layout.PixelToFrame(pos.X);

        CommitDrag(frame);

        _activeDrag = null;
        _selectionDragAnchor = null;
        Mouse.Capture(null);
        _element?.InvalidateVisual();
        e.Handled = true;
    }

    private void CommitDrag(int frame)
    {
        if (_activeDrag is null) return;

        switch (_activeDrag.Target)
        {
            case DragTarget.LoopIn:
                _state.LoopStart = Math.Min(frame, _state.LoopEnd ?? frame);
                break;

            case DragTarget.LoopOut:
                _state.LoopEnd = Math.Max(frame, _state.LoopStart ?? frame);
                break;

            case DragTarget.MarkerPoint when _activeDrag.Marker is PointMarker point:
            {
                int oldFrame = _dragStartFrame;
                point.Frame = frame;
                RaiseEvent(MarkerDragCompletedEventArgs.ForPoint(MarkerDragCompletedEvent, point, oldFrame, frame));
                break;
            }

            case DragTarget.MarkerRangeStart when _activeDrag.Marker is RangeMarker range:
            {
                int newStart = Math.Min(frame, range.EndFrame);
                range.StartFrame = newStart;
                RaiseEvent(MarkerDragCompletedEventArgs.ForRange(MarkerDragCompletedEvent, range,
                    _dragStartFrame, _dragStartEndFrame, range.StartFrame, range.EndFrame));
                break;
            }

            case DragTarget.MarkerRangeEnd when _activeDrag.Marker is RangeMarker range:
            {
                int newEnd = Math.Max(frame, range.StartFrame);
                range.EndFrame = newEnd;
                RaiseEvent(MarkerDragCompletedEventArgs.ForRange(MarkerDragCompletedEvent, range,
                    _dragStartFrame, _dragStartEndFrame, range.StartFrame, range.EndFrame));
                break;
            }

            case DragTarget.MarkerRangeBody when _activeDrag.Marker is RangeMarker range:
            {
                int width = _dragStartEndFrame - _dragStartFrame;
                int ghostStart = _activeDrag.GhostFrame - (width / 2);
                int newStart = Math.Max(0, ghostStart);
                int newEnd = Math.Min(_state.TotalFrames - 1, newStart + width);
                range.StartFrame = newStart;
                range.EndFrame = newEnd;
                RaiseEvent(MarkerDragCompletedEventArgs.ForRange(MarkerDragCompletedEvent, range,
                    _dragStartFrame, _dragStartEndFrame, newStart, newEnd));
                break;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Live mode auto-scroll
    // ──────────────────────────────────────────────────────────────────────────

    private void AutoScrollToPlayheadIfLive()
    {
        if (_state.Mode != TimelineMode.Live) return;

        var layout = _state.BuildLayout(ActualWidth);
        double playheadPx = layout.FrameToPixel(_state.CurrentFrame);

        if (playheadPx < 0 || playheadPx > layout.ViewportWidth)
        {
            // Scroll so playhead is at the left edge
            _state.ScrollOffsetPx = _state.CurrentFrame * _state.ZoomScale;
        }
    }
}
