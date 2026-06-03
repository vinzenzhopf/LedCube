using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.UI.Controls.PlaylistControl;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;

namespace LedCube.Core.UI.Controls.PlaybackControl;

public partial class PlaybackControlViewModel : ObservableObject
{
    private readonly IPlaybackService _playbackService;
    private readonly IPlaylistService _playlistService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAnimationSelected))]
    [NotifyCanExecuteChangedFor(nameof(PlayContinueCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [NotifyCanExecuteChangedFor(nameof(RestartCommand))]
    [NotifyCanExecuteChangedFor(nameof(PauseCommand))]
    private AnimationViewModel? _animation;

    [ObservableProperty]
    private PlaybackState _playbackState;

    [ObservableProperty]
    private int _currentFrame;

    [ObservableProperty]
    private TimeSpan _currentTime = TimeSpan.Zero;

    public bool IsAnimationSelected => Animation is not null;

    public PlaybackControlViewModel(IPlaybackService playbackService, IPlaylistService playlistService)
    {
        _playbackService = playbackService;
        _playlistService = playlistService;
        _animation = BuildAnimationViewModel(playbackService.CurrentEntry);
        _playbackState = playbackService.PlaybackState;
        _currentFrame = playbackService.CurrentFrame;
        _currentTime = playbackService.CurrentTime;
        ((INotifyPropertyChanged)playbackService).PropertyChanged += OnServicePropertyChanged;
        ((INotifyCollectionChanged)playlistService.Entries).CollectionChanged += OnEntriesChanged;
    }

    private static AnimationViewModel? BuildAnimationViewModel(PlaylistEntry? entry)
    {
        if (entry is null) return null;
        return new AnimationViewModel
        {
            Name = entry.Info.Name,
            Description = entry.Info.Description,
            TypeInfo = entry.TypeInfo,
            GeneratorInfo = entry.Info,
        };
    }

    private void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IPlaybackService.CurrentEntry):
                Animation = BuildAnimationViewModel(_playbackService.CurrentEntry);
                if (Animation is not null)
                {
                    Animation.FrameTime = _playbackService.FrameTime;
                    Animation.FrameCount = _playbackService.FrameCount ?? 0;
                }
                break;
            case nameof(IPlaybackService.FrameTime):
                if (Animation is not null)
                    Animation.FrameTime = _playbackService.FrameTime;
                break;
            case nameof(IPlaybackService.FrameCount):
                if (Animation is not null)
                    Animation.FrameCount = _playbackService.FrameCount ?? 0;
                break;
            case nameof(IPlaybackService.PlaybackState):
                PlaybackState = _playbackService.PlaybackState;
                break;
            case nameof(IPlaybackService.CurrentFrame):
                CurrentFrame = _playbackService.CurrentFrame;
                break;
            case nameof(IPlaybackService.CurrentTime):
                CurrentTime = _playbackService.CurrentTime;
                break;
        }
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ForwardCommand.NotifyCanExecuteChanged();
        BackwardCommand.NotifyCanExecuteChanged();
    }

    private bool HasAnimation() => Animation is not null;
    private bool CanNavigatePlaylist() => _playlistService.Entries.Count > 1;

    public void SeekToFrame(int frame) => _playbackService.SeekToFrame(frame);

    [RelayCommand(CanExecute = nameof(HasAnimation))]
    private Task PlayContinue(CancellationToken token)
    {
        if (_playbackService.PlaybackState == PlaybackState.Stopped)
            _playbackService.StartPlayback();
        else
            _playbackService.ContinuePlayback();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(HasAnimation))]
    private Task Stop(CancellationToken token)
    {
        _playbackService.StopPlayback();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(HasAnimation))]
    private Task Restart(CancellationToken token)
    {
        if (_playbackService.PlaybackState != PlaybackState.Stopped)
            _playbackService.StopPlayback();
        _playbackService.StartPlayback();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(HasAnimation))]
    private Task Pause(CancellationToken token)
    {
        _playbackService.PausePlayback();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePlaylist))]
    private Task Forward(CancellationToken token)
    {
        _playlistService.SelectNext();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePlaylist))]
    private Task Backward(CancellationToken token)
    {
        _playlistService.SelectPrevious();
        return Task.CompletedTask;
    }
}
