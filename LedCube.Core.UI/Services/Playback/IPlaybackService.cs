using System;
using System.Threading;
using System.Threading.Tasks;
using LedCube.Core.UI.Services.Playlist;

namespace LedCube.Core.UI.Services.Playback;

public interface IPlaybackService
{
    public PlaybackState PlaybackState { get; }
    public PlaylistEntry? CurrentEntry { get; }
    public TimeSpan FrameTime { get; }
    public int CurrentFrame { get; }
    public TimeSpan CurrentTime { get; }
    public Task UpdateFrameGeneratorAsync(PlaylistEntry entry, CancellationToken token);
    public void StartPlayback();
    public void ContinuePlayback();
    public void StopPlayback();
    public void PausePlayback();
    public void SeekToFrame(int frame);
}
