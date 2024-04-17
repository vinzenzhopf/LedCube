using System;
using System.Threading;
using System.Threading.Tasks;

namespace LedCube.PluginBase;

public abstract class FrameGeneratorBase : IFrameGenerator
{
    public static FrameGeneratorInfo Info { get; }
    public abstract TimeSpan? FrameTime { get; }
    public virtual Task InitializeAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public virtual void DrawFrame(FrameContext frameContext)
    {
        throw new NotImplementedException();
    }

    public virtual void Start(AnimationContext animationContext)
    {
    }

    public virtual void End(AnimationContext animationContext)
    {
    }

    public virtual void Pause(AnimationContext animationContext)
    {
    }

    public virtual void Continue(AnimationContext animationContext)
    {
    }

    public virtual void ChangeTime(AnimationContext animationContext)
    {
    }
    
    public virtual void Dispose()
    {
    }
}