using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Streamer.UI.Controls.PlaybackControl;

namespace LedCube.Streamer.UI.Controls.AnimationList;

[ObservableObject]
public partial class AnimationListViewModel
{
    private IPlaybackService _playbackService;

    [ObservableProperty]
    private ObservableCollection<AnimationViewModel> _animations = new()
    {
        new AnimationViewModel()
        {
            Name = "Test1",
            Description = "Desc1",
            FrameCount = 350,
            FrameTime = TimeSpan.FromMilliseconds(5)
        },
        new AnimationViewModel()
        {
            Name = "Test2",
            Description = "Desc2",
            FrameCount = 10,
            FrameTime = TimeSpan.FromMilliseconds(200)
        },
        new AnimationViewModel()
        {
            Name = "Test3",
            Description = "Desc3",
            FrameCount = 5600,
            FrameTime = TimeSpan.FromMilliseconds(5)
        }
    };

    [ObservableProperty]
    private AnimationViewModel _selectedAnimation;
    
    public AnimationListViewModel(IPlaybackService playbackService)
    {
        _playbackService = playbackService;
    }

    //Design Time Constructor
    public AnimationListViewModel()
    {
    }
}