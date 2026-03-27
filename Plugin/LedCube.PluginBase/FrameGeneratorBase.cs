using System;
using System.Threading;
using System.Threading.Tasks;

namespace LedCube.PluginBase;

public abstract class FrameGeneratorBase : IFrameGenerator
{
    public static FrameGeneratorInfo Info => new("", "");

    public abstract TimeSpan? FrameTime { get; }

    public virtual Task InitializeAsync(CancellationToken token) => Task.CompletedTask;

    public virtual DrawingResult DrawFrame(FrameContext frameContext) => DrawingResult.Continue;

    public virtual void Start(AnimationContext animationContext) { }
    public virtual void End(AnimationContext animationContext) { }
    public virtual void Pause(AnimationContext animationContext) { }
    public virtual void Continue(AnimationContext animationContext) { }
    public virtual void ChangeTime(AnimationContext animationContext) { }
}
