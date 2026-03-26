using System.Threading;
using System.Threading.Tasks;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.PluginHost;

namespace LedCube.Core.UI.Services;

public interface IPlaybackService
{
    public PlaybackState PlaybackState { get; }
    public AnimationViewModel? Animation { get; }
    public Task UpdateFrameGeneratorAsync(FrameGeneratorEntry entry, AnimationViewModel animation, CancellationToken token);
    public void StartPlayback();
    public void ContinuePlayback();
    public void StopPlayback();
    public void PausePlayback();
}