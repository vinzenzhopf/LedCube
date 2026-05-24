using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace LedCube.Core.UI.TimelineControl;

/// <summary>
/// SkiaSharp-based timeline control. TimelineState is the single source of truth;
/// AvaloniaProperties are a thin sync layer on top.
/// </summary>
public class TimelineControl : Control
{
    // ──────────────────────────────────────────────────────────────────────────
    // Routed Events
    // ──────────────────────────────────────────────────────────────────────────

    public static readonly RoutedEvent<PlayheadChangedEventArgs> PlayheadChangedEvent =
        RoutedEvent.Register<TimelineControl, PlayheadChangedEventArgs>(
            nameof(PlayheadChanged), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<TimelineControl, SelectionChangedEventArgs>(
            nameof(SelectionChanged), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<MarkerDragStartedEventArgs> MarkerDragStartedEvent =
        RoutedEvent.Register<TimelineControl, MarkerDragStartedEventArgs>(
            nameof(MarkerDragStarted), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<MarkerDraggingEventArgs> MarkerDraggingEvent =
        RoutedEvent.Register<TimelineControl, MarkerDraggingEventArgs>(
            nameof(MarkerDragging), RoutingStrategies.Bubble);

    public static readonly RoutedEvent<MarkerDragCompletedEventArgs> MarkerDragCompletedEvent =
        RoutedEvent.Register<TimelineControl, MarkerDragCompletedEventArgs>(
            nameof(MarkerDragCompleted), RoutingStrategies.Bubble);

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

    // ──────────────────────────────────────────────────────────────────────────
    // Avalonia Properties
    // ──────────────────────────────────────────────────────────────────────────

    public static readonly StyledProperty<TimelineMode> ModeProperty =
        AvaloniaProperty.Register<TimelineControl, TimelineMode>(nameof(Mode), TimelineMode.Edit);

    public static readonly StyledProperty<int> TotalFramesProperty =
        AvaloniaProperty.Register<TimelineControl, int>(nameof(TotalFrames), 100);

    public static readonly StyledProperty<TimeSpan?> FrameTimeProperty =
        AvaloniaProperty.Register<TimelineControl, TimeSpan?>(nameof(FrameTime), null);

    public static readonly StyledProperty<int> CurrentFrameProperty =
        AvaloniaProperty.Register<TimelineControl, int>(nameof(CurrentFrame), 0);

    public static readonly StyledProperty<int?> SelectionStartProperty =
        AvaloniaProperty.Register<TimelineControl, int?>(nameof(SelectionStart), null);

    public static readonly StyledProperty<int?> SelectionEndProperty =
        AvaloniaProperty.Register<TimelineControl, int?>(nameof(SelectionEnd), null);

    public static readonly StyledProperty<int?> LoopStartProperty =
        AvaloniaProperty.Register<TimelineControl, int?>(nameof(LoopStart), null);

    public static readonly StyledProperty<int?> LoopEndProperty =
        AvaloniaProperty.Register<TimelineControl, int?>(nameof(LoopEnd), null);

    public static readonly StyledProperty<bool> LoopEnabledProperty =
        AvaloniaProperty.Register<TimelineControl, bool>(nameof(LoopEnabled), false);

    public static readonly StyledProperty<ObservableCollection<MarkerBase>> MarkersProperty =
        AvaloniaProperty.Register<TimelineControl, ObservableCollection<MarkerBase>>(nameof(Markers));

    public TimelineMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public int TotalFrames
    {
        get => GetValue(TotalFramesProperty);
        set => SetValue(TotalFramesProperty, value);
    }

    public TimeSpan? FrameTime
    {
        get => GetValue(FrameTimeProperty);
        set => SetValue(FrameTimeProperty, value);
    }

    public int CurrentFrame
    {
        get => GetValue(CurrentFrameProperty);
        set => SetValue(CurrentFrameProperty, value);
    }

    public int? SelectionStart
    {
        get => GetValue(SelectionStartProperty);
        set => SetValue(SelectionStartProperty, value);
    }

    public int? SelectionEnd
    {
        get => GetValue(SelectionEndProperty);
        set => SetValue(SelectionEndProperty, value);
    }

    public int? LoopStart
    {
        get => GetValue(LoopStartProperty);
        set => SetValue(LoopStartProperty, value);
    }

    public int? LoopEnd
    {
        get => GetValue(LoopEndProperty);
        set => SetValue(LoopEndProperty, value);
    }

    public bool LoopEnabled
    {
        get => GetValue(LoopEnabledProperty);
        set => SetValue(LoopEnabledProperty, value);
    }

    public ObservableCollection<MarkerBase> Markers
    {
        get => GetValue(MarkersProperty);
        set => SetValue(MarkersProperty, value);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Property changed
    // ──────────────────────────────────────────────────────────────────────────

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (_isSyncingState) return;

        if (change.Property == ModeProperty)
        {
            _isSyncingState = true;
            _state.Mode = change.GetNewValue<TimelineMode>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == TotalFramesProperty)
        {
            _isSyncingState = true;
            _state.TotalFrames = change.GetNewValue<int>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == FrameTimeProperty)
        {
            _isSyncingState = true;
            _state.FrameTime = change.GetNewValue<TimeSpan?>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == CurrentFrameProperty)
        {
            _isSyncingState = true;
            _state.CurrentFrame = change.GetNewValue<int>();
            _isSyncingState = false;
            AutoScrollToPlayheadIfLive();
            InvalidateVisual();
        }
        else if (change.Property == SelectionStartProperty)
        {
            _isSyncingState = true;
            _state.SelectionStart = change.GetNewValue<int?>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == SelectionEndProperty)
        {
            _isSyncingState = true;
            _state.SelectionEnd = change.GetNewValue<int?>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == LoopStartProperty)
        {
            _isSyncingState = true;
            _state.LoopStart = change.GetNewValue<int?>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == LoopEndProperty)
        {
            _isSyncingState = true;
            _state.LoopEnd = change.GetNewValue<int?>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == LoopEnabledProperty)
        {
            _isSyncingState = true;
            _state.LoopEnabled = change.GetNewValue<bool>();
            _isSyncingState = false;
            InvalidateVisual();
        }
        else if (change.Property == MarkersProperty)
        {
            if (change.OldValue is ObservableCollection<MarkerBase> old)
                old.CollectionChanged -= OnMarkersCollectionChanged;

            if (change.NewValue is ObservableCollection<MarkerBase> newCol)
            {
                newCol.CollectionChanged += OnMarkersCollectionChanged;
                _state.Markers.Clear();
                foreach (var m in newCol)
                    _state.Markers.Add(m);
            }

            InvalidateVisual();
        }
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
        InvalidateVisual();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Instance state
    // ──────────────────────────────────────────────────────────────────────────

    private readonly TimelineState _state = new();
    private RenderResources? _resources;
    private DragOperation? _activeDrag;
    private bool _isSyncingState;
    private IPointer? _capturedPointer;

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

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Layout
    // ──────────────────────────────────────────────────────────────────────────

    private static readonly double MinControlHeight = TimelineRenderer.RulerHeight + TimelineRenderer.DefaultTrackHeight;

    protected override Size MeasureOverride(Size availableSize)
    {
        double w = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        double h = double.IsInfinity(availableSize.Height) ? MinControlHeight : Math.Max(MinControlHeight, availableSize.Height);
        return new Size(w, h);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double h = Math.Max(MinControlHeight, finalSize.Height);
        return new Size(finalSize.Width, h);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _resources = new RenderResources();
        InvalidateVisual();
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _resources?.Dispose();
        _resources = null;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // State → property sync
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
                var oldFrame = GetValue(CurrentFrameProperty);
                SetValue(CurrentFrameProperty, _state.CurrentFrame);
                if (oldFrame != _state.CurrentFrame)
                {
                    RaiseEvent(new PlayheadChangedEventArgs(oldFrame, _state.CurrentFrame) { RoutedEvent = PlayheadChangedEvent });
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
        InvalidateVisual();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Render
    // ──────────────────────────────────────────────────────────────────────────

    public override void Render(DrawingContext context)
    {
        if (_resources is null) return;
        context.Custom(new SkiaDrawOp(new Rect(Bounds.Size), _state, _activeDrag, _resources));
    }

    private sealed class SkiaDrawOp : ICustomDrawOperation
    {
        private readonly Rect _bounds;
        private readonly TimelineState _state;
        private readonly DragOperation? _drag;
        private readonly RenderResources _resources;

        public SkiaDrawOp(Rect bounds, TimelineState state, DragOperation? drag, RenderResources resources)
        {
            _bounds = bounds;
            _state = state;
            _drag = drag;
            _resources = resources;
        }

        public Rect Bounds => _bounds;
        public bool Equals(ICustomDrawOperation? other) => false;
        public bool HitTest(Point p) => _bounds.Contains(p);
        public void Dispose() { }

        public void Render(ImmediateDrawingContext context)
        {
            var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            using var lease = leaseFeature?.Lease();
            if (lease?.SkCanvas is not SKCanvas canvas) return;

            // The leased SkCanvas is the window's shared surface — DO NOT call
            // canvas.Clear() (would wipe the entire window's rendering).
            // Isolate state so nothing leaks out of our bounds either.
            var savePoint = canvas.Save();
            try
            {
                canvas.ClipRect(SKRect.Create(
                    (float)_bounds.X, (float)_bounds.Y,
                    (float)_bounds.Width, (float)_bounds.Height));
                var layout = _state.BuildLayout(_bounds.Width, _bounds.Height);
                TimelineRenderer.Draw(canvas, layout, _state, _drag, _resources);
            }
            finally
            {
                canvas.RestoreToCount(savePoint);
            }
        }
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
    // Pointer — scroll & zoom (all modes)
    // ──────────────────────────────────────────────────────────────────────────

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            // Zoom centred on cursor
            var layout = _state.BuildLayout(Bounds.Width, Bounds.Height);
            double mouseX = e.GetPosition(this).X;
            int frameUnderCursor = layout.PixelToFrame(mouseX);

            double zoomDelta = e.Delta.Y > 0 ? 1.2 : 1.0 / 1.2;
            double newZoom = Math.Clamp(_state.ZoomScale * zoomDelta, 1.0, 100.0);

            // Compute new scroll so that frameUnderCursor stays at mouseX
            double newScroll = frameUnderCursor * newZoom - mouseX;

            _state.ZoomScale = newZoom;
            _state.ScrollOffsetPx = Math.Max(0, newScroll);
        }
        else
        {
            // Horizontal scroll
            double delta = e.Delta.Y > 0 ? -30.0 : 30.0;
            _state.ScrollOffsetPx = Math.Max(0, _state.ScrollOffsetPx + delta);
        }

        InvalidateVisual();
        e.Handled = true;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Pointer — drag interaction (Edit mode only)
    // ──────────────────────────────────────────────────────────────────────────

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        Focus();

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        if (_state.Mode != TimelineMode.Edit) return;

        var pos = e.GetPosition(this);
        var layout = _state.BuildLayout(Bounds.Width, Bounds.Height);
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
                _capturedPointer = e.Pointer;
                e.Pointer.Capture(this);
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
                _capturedPointer = e.Pointer;
                e.Pointer.Capture(this);
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
                    _capturedPointer = e.Pointer;
                    e.Pointer.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(marker) { RoutedEvent = MarkerDragStartedEvent });
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
                    _capturedPointer = e.Pointer;
                    e.Pointer.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(marker) { RoutedEvent = MarkerDragStartedEvent });
                    e.Handled = true;
                    return;
                }

                if (Math.Abs(pos.X - endPx) <= 5.0)
                {
                    _activeDrag = new DragOperation { Target = DragTarget.MarkerRangeEnd, Marker = marker, GhostFrame = range.EndFrame };
                    _dragStartFrame = range.StartFrame;
                    _dragStartEndFrame = range.EndFrame;
                    _capturedPointer = e.Pointer;
                    e.Pointer.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(marker) { RoutedEvent = MarkerDragStartedEvent });
                    e.Handled = true;
                    return;
                }

                // Body drag: inside the range
                if (pos.X > startPx && pos.X < endPx)
                {
                    _activeDrag = new DragOperation { Target = DragTarget.MarkerRangeBody, Marker = marker, GhostFrame = frame };
                    _dragStartFrame = range.StartFrame;
                    _dragStartEndFrame = range.EndFrame;
                    _capturedPointer = e.Pointer;
                    e.Pointer.Capture(this);
                    RaiseEvent(new MarkerDragStartedEventArgs(marker) { RoutedEvent = MarkerDragStartedEvent });
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
                RaiseEvent(new PlayheadChangedEventArgs(oldFrame, frame) { RoutedEvent = PlayheadChangedEvent });

            _activeDrag = new DragOperation { Target = DragTarget.Playhead, GhostFrame = frame };
            _dragStartFrame = frame;
            _capturedPointer = e.Pointer;
            e.Pointer.Capture(this);
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
            RaiseEvent(new SelectionChangedEventArgs(oldSelStart, oldSelEnd, frame, frame) { RoutedEvent = SelectionChangedEvent });

        _activeDrag = new DragOperation { Target = DragTarget.SelectionEnd, GhostFrame = frame };
        _dragStartFrame = frame;
        _capturedPointer = e.Pointer;
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_activeDrag is null) return;

        var pos = e.GetPosition(this);
        var layout = _state.BuildLayout(Bounds.Width, Bounds.Height);
        int frame = layout.PixelToFrame(pos.X);

        _activeDrag.GhostFrame = frame;

        switch (_activeDrag.Target)
        {
            case DragTarget.Playhead:
                int oldFrame = _state.CurrentFrame;
                _state.CurrentFrame = frame;
                if (oldFrame != frame)
                    RaiseEvent(new PlayheadChangedEventArgs(oldFrame, frame) { RoutedEvent = PlayheadChangedEvent });
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
                        RaiseEvent(new SelectionChangedEventArgs(oldStart, oldEnd, _state.SelectionStart, _state.SelectionEnd) { RoutedEvent = SelectionChangedEvent });
                }
                break;

            case DragTarget.MarkerPoint:
            case DragTarget.MarkerRangeStart:
            case DragTarget.MarkerRangeEnd:
            case DragTarget.MarkerRangeBody:
                if (_activeDrag.Marker is not null)
                    RaiseEvent(new MarkerDraggingEventArgs(_activeDrag.Marker, frame) { RoutedEvent = MarkerDraggingEvent });
                break;
        }

        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_activeDrag is null) return;
        if (e.InitialPressMouseButton != MouseButton.Left) return;

        var pos = e.GetPosition(this);
        var layout = _state.BuildLayout(Bounds.Width, Bounds.Height);
        int frame = layout.PixelToFrame(pos.X);

        CommitDrag(frame);

        _activeDrag = null;
        _selectionDragAnchor = null;
        _capturedPointer?.Capture(null);
        _capturedPointer = null;
        InvalidateVisual();
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
                var pointArgs = MarkerDragCompletedEventArgs.ForPoint(point, oldFrame, frame);
                pointArgs.RoutedEvent = MarkerDragCompletedEvent;
                RaiseEvent(pointArgs);
                break;
            }

            case DragTarget.MarkerRangeStart when _activeDrag.Marker is RangeMarker range:
            {
                int newStart = Math.Min(frame, range.EndFrame);
                range.StartFrame = newStart;
                var rangeStartArgs = MarkerDragCompletedEventArgs.ForRange(range,
                    _dragStartFrame, _dragStartEndFrame, range.StartFrame, range.EndFrame);
                rangeStartArgs.RoutedEvent = MarkerDragCompletedEvent;
                RaiseEvent(rangeStartArgs);
                break;
            }

            case DragTarget.MarkerRangeEnd when _activeDrag.Marker is RangeMarker range:
            {
                int newEnd = Math.Max(frame, range.StartFrame);
                range.EndFrame = newEnd;
                var rangeEndArgs = MarkerDragCompletedEventArgs.ForRange(range,
                    _dragStartFrame, _dragStartEndFrame, range.StartFrame, range.EndFrame);
                rangeEndArgs.RoutedEvent = MarkerDragCompletedEvent;
                RaiseEvent(rangeEndArgs);
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
                var rangeBodyArgs = MarkerDragCompletedEventArgs.ForRange(range,
                    _dragStartFrame, _dragStartEndFrame, newStart, newEnd);
                rangeBodyArgs.RoutedEvent = MarkerDragCompletedEvent;
                RaiseEvent(rangeBodyArgs);
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

        var layout = _state.BuildLayout(Bounds.Width, Bounds.Height);
        double playheadPx = layout.FrameToPixel(_state.CurrentFrame);

        if (playheadPx < 0 || playheadPx > layout.ViewportWidth)
        {
            // Scroll so playhead is at the left edge
            _state.ScrollOffsetPx = _state.CurrentFrame * _state.ZoomScale;
        }
    }
}
