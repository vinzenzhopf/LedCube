using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;

namespace LedCube.Core.UI.Test.Fakes;

/// <summary>
/// In-memory IPlaybackService that raises PropertyChanged on every setter (the
/// PlaybackControlViewModel subscribes to it) and records which control methods were called.
/// </summary>
public sealed class FakePlaybackService : IPlaybackService, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private PlaybackState _playbackState;
    private PlaylistEntry? _currentEntry;
    private TimeSpan _frameTime;
    private int? _frameCount;
    private int _currentFrame;
    private TimeSpan _currentTime;

    public PlaybackState PlaybackState { get => _playbackState; set => Set(ref _playbackState, value); }
    public PlaylistEntry? CurrentEntry { get => _currentEntry; set => Set(ref _currentEntry, value); }
    public TimeSpan FrameTime { get => _frameTime; set => Set(ref _frameTime, value); }
    public int? FrameCount { get => _frameCount; set => Set(ref _frameCount, value); }
    public int CurrentFrame { get => _currentFrame; set => Set(ref _currentFrame, value); }
    public TimeSpan CurrentTime { get => _currentTime; set => Set(ref _currentTime, value); }

    public bool StartPlaybackCalled { get; private set; }
    public bool ContinuePlaybackCalled { get; private set; }
    public bool StopPlaybackCalled { get; private set; }
    public bool PausePlaybackCalled { get; private set; }
    public int? SeekedFrame { get; private set; }
    public int StartPlaybackCount { get; private set; }

    public Task UpdateFrameGeneratorAsync(PlaylistEntry entry, CancellationToken token)
    {
        // Mimic the real service: loading an entry makes it the current one.
        CurrentEntry = entry;
        return Task.CompletedTask;
    }

    public void StartPlayback()
    {
        StartPlaybackCalled = true;
        StartPlaybackCount++;
        PlaybackState = PlaybackState.Playing;
    }

    public void ContinuePlayback() => ContinuePlaybackCalled = true;
    public void StopPlayback() => StopPlaybackCalled = true;
    public void PausePlayback() => PausePlaybackCalled = true;
    public void SeekToFrame(int frame) => SeekedFrame = frame;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
