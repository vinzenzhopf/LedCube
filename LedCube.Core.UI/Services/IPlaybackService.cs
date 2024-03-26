using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.PluginHost;

namespace LedCube.Core.UI.Services;

public interface IPlaybackService
{
    public PlaybackState PlaybackState { get; }
    public AnimationViewModel? Animation { get; }
    public void UpdateFrameGenerator(FrameGeneratorEntry entry, AnimationViewModel animation);
    public void StartPlayback();
    public void ContinuePlayback();
    public void StopPlayback();
    public void PausePlayback();
}