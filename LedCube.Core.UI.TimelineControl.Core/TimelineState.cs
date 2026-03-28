using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedCube.Core.UI.TimelineControl;

/// <summary>
/// All mutable runtime state for the timeline. Observed by the host for change detection.
/// </summary>
public class TimelineState : INotifyPropertyChanged
{
    private TimelineMode _mode = TimelineMode.Edit;
    private int _totalFrames = 100;
    private TimeSpan? _frameTime;
    private int _currentFrame;
    private int? _selectionStart;
    private int? _selectionEnd;
    private int? _loopStart;
    private int? _loopEnd;
    private bool _loopEnabled;
    private double _zoomScale = 10.0;
    private double _scrollOffsetPx;

    public TimelineMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value) return;
            _mode = value;
            if (value == TimelineMode.Live)
            {
                SelectionStart = null;
                SelectionEnd = null;
            }
            OnPropertyChanged();
        }
    }

    public int TotalFrames
    {
        get => _totalFrames;
        set
        {
            if (_totalFrames == value) return;
            _totalFrames = value;
            ApplyFrameLimit(value);
            OnPropertyChanged();
        }
    }

    public TimeSpan? FrameTime
    {
        get => _frameTime;
        set { if (_frameTime == value) return; _frameTime = value; OnPropertyChanged(); }
    }

    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            var clamped = Math.Clamp(value, 0, Math.Max(0, _totalFrames - 1));
            if (_currentFrame == clamped) return;
            _currentFrame = clamped;
            OnPropertyChanged();
        }
    }

    public int? SelectionStart
    {
        get => _selectionStart;
        set { if (_selectionStart == value) return; _selectionStart = value; OnPropertyChanged(); }
    }

    public int? SelectionEnd
    {
        get => _selectionEnd;
        set { if (_selectionEnd == value) return; _selectionEnd = value; OnPropertyChanged(); }
    }

    public int? LoopStart
    {
        get => _loopStart;
        set { if (_loopStart == value) return; _loopStart = value; OnPropertyChanged(); }
    }

    public int? LoopEnd
    {
        get => _loopEnd;
        set { if (_loopEnd == value) return; _loopEnd = value; OnPropertyChanged(); }
    }

    public bool LoopEnabled
    {
        get => _loopEnabled;
        set { if (_loopEnabled == value) return; _loopEnabled = value; OnPropertyChanged(); }
    }

    public double ZoomScale
    {
        get => _zoomScale;
        set
        {
            var clamped = Math.Max(0.5, value);
            if (_zoomScale == clamped) return;
            _zoomScale = clamped;
            OnPropertyChanged();
        }
    }

    public double ScrollOffsetPx
    {
        get => _scrollOffsetPx;
        set
        {
            var clamped = Math.Max(0, value);
            if (_scrollOffsetPx == clamped) return;
            _scrollOffsetPx = clamped;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<MarkerBase> Markers { get; } = new();

    public TimelineLayout BuildLayout(double viewportWidth, double viewportHeight = 0) => new()
    {
        ZoomScale = _zoomScale,
        ScrollOffsetPx = _scrollOffsetPx,
        TotalFrames = _totalFrames,
        ViewportWidth = viewportWidth,
        ViewportHeight = viewportHeight
    };

    private void ApplyFrameLimit(int newTotalFrames)
    {
        CurrentFrame = Math.Clamp(_currentFrame, 0, Math.Max(0, newTotalFrames - 1));

        var toRemove = Markers.Where(m => !m.ApplyFrameLimit(newTotalFrames)).ToList();
        foreach (var m in toRemove)
            Markers.Remove(m);

        if (_selectionStart >= newTotalFrames) SelectionStart = null;
        if (_selectionEnd >= newTotalFrames) SelectionEnd = null;
        if (_loopStart >= newTotalFrames) LoopStart = null;
        if (_loopEnd >= newTotalFrames) LoopEnd = Math.Max(0, newTotalFrames - 1);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
