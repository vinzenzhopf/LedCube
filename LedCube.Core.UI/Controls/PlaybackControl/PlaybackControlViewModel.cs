using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.Core.UI.Services;
using AnimationViewModel = LedCube.Core.UI.Controls.AnimationInstanceList.AnimationViewModel;

namespace LedCube.Core.UI.Controls.PlaybackControl;

[ObservableObject]
public partial class PlaybackControlViewModel
{
    public IPlaybackService PlaybackService { get; }
    
    [ObservableProperty]
    private PlaybackState _playbackState;

    [ObservableProperty]
    private int _currentFrame;

    [ObservableProperty]
    private TimeSpan _currentTime = TimeSpan.Zero;

    public bool IsAnimationSelected => PlaybackService.Animation is not null;
    
    public PlaybackControlViewModel(IPlaybackService playbackService)
    { 
        PlaybackService = playbackService;
    }
    
    [RelayCommand]
    private Task PlayContinue(CancellationToken token)
    {
        if (PlaybackService.PlaybackState == PlaybackState.Stopped)
        {
            PlaybackService.StartPlayback();
        }else{
            PlaybackService.ContinuePlayback();
        } 
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private Task Stop(CancellationToken token)
    {
        PlaybackService.StopPlayback();
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private Task Restart(CancellationToken token)
    {
        if (PlaybackService.PlaybackState != PlaybackState.Stopped)
        {
            PlaybackService.StopPlayback();
        }
        PlaybackService.StartPlayback();
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private Task Pause(CancellationToken token)
    {
        PlaybackService.PausePlayback();
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private Task Forward(CancellationToken token)
    {
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private Task Backward(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}
