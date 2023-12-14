using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.UI.Controls.AnimationList;

namespace LedCube.Core.UI.Controls.PlaybackControl;

[ObservableObject]
public partial class PlaybackControlViewModel
{
    private IPlaybackService _playbackService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnimationSelected))]
    private AnimationViewModel? _activeAnimation = new AnimationViewModel()
    {
        Name = "Test1",
        Description = "Desc1",
        FrameCount = 350,
        FrameTime = TimeSpan.FromMilliseconds(5)
    };

    [ObservableProperty]
    private PlaybackState _playbackState;

    [ObservableProperty]
    private int _currentFrame;

    [ObservableProperty]
    private TimeSpan _currentTime = TimeSpan.Zero;

    public bool IsAnimationSelected => ActiveAnimation is not null;
    
    public PlaybackControlViewModel(/*IPlaybackService playbackService*/)
    {
        // _playbackService = playbackService;
    }
}

public enum PlaybackState
{
    Stopped,
    Playing,
    Paused
}
