using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI.TimelineControl.Demo;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private TimelineMode _mode = TimelineMode.Edit;
    [ObservableProperty] private int _totalFrames = 200;
    [ObservableProperty] private int _currentFrame;
    [ObservableProperty] private bool _loopEnabled;
    [ObservableProperty] private int _loopStart = 20;
    [ObservableProperty] private int _loopEnd = 80;
    [ObservableProperty] private bool _showFrameTime;
    [ObservableProperty] private int _modeIndex;

    public TimeSpan? FrameTime => ShowFrameTime ? TimeSpan.FromMilliseconds(33.3) : null;

    partial void OnModeIndexChanged(int value) =>
        Mode = value == 0 ? TimelineMode.Edit : TimelineMode.Live;

    partial void OnShowFrameTimeChanged(bool value) =>
        OnPropertyChanged(nameof(FrameTime));
}
