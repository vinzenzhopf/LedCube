using System;
using System.Threading;
using System.Threading.Tasks;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.PluginHost;

namespace LedCube.Core.UI.Services.Playback;

public interface IPlaybackService
{
    public PlaybackState PlaybackState { get; }
    public AnimationViewModel? Animation { get; }
    public int CurrentFrame { get; }
    public TimeSpan CurrentTime { get; }
    public Task UpdateFrameGeneratorAsync(FrameGeneratorEntry entry, AnimationViewModel animation, CancellationToken token);
    public void StartPlayback();
    public void ContinuePlayback();
    public void StopPlayback();
    public void PausePlayback();
    public void SeekToFrame(int frame);
}