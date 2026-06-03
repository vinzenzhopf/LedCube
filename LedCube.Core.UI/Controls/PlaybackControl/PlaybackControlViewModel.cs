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
using Material.Icons;

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
        playlistService.PropertyChanged += OnPlaylistServicePropertyChanged;
    }

    public PlaylistRepeatMode RepeatMode => _playlistService.RepeatMode;

    public MaterialIconKind RepeatModeIcon => _playlistService.RepeatMode switch
    {
        PlaylistRepeatMode.StopAtEnd           => MaterialIconKind.RepeatOff,
        PlaylistRepeatMode.LoopWholePlaylist   => MaterialIconKind.Repeat,
        PlaylistRepeatMode.RepeatCurrentEntry  => MaterialIconKind.RepeatOnce,
        PlaylistRepeatMode.FairRandomPlay => MaterialIconKind.ShuffleVariant,
        PlaylistRepeatMode.TrueRandomPlay      => MaterialIconKind.Shuffle,
        _                                      => MaterialIconKind.RepeatOff,
    };

    public string RepeatModeTooltip => _playlistService.RepeatMode switch
    {
        PlaylistRepeatMode.StopAtEnd           => "Stop at end of playlist",
        PlaylistRepeatMode.LoopWholePlaylist   => "Loop whole playlist",
        PlaylistRepeatMode.RepeatCurrentEntry  => "Repeat current entry",
        PlaylistRepeatMode.FairRandomPlay => "Shuffle (play all before repeating)",
        PlaylistRepeatMode.TrueRandomPlay      => "Random",
        _                                      => string.Empty,
    };

    private void OnPlaylistServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IPlaylistService.RepeatMode)) return;
        OnPropertyChanged(nameof(RepeatMode));
        OnPropertyChanged(nameof(RepeatModeIcon));
        OnPropertyChanged(nameof(RepeatModeTooltip));
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
        _playlistService.PlayNext();
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(CanNavigatePlaylist))]
    private Task Backward(CancellationToken token)
    {
        _playlistService.PlayPrevious();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void CycleRepeatMode() => _playlistService.CycleRepeatMode();
}
