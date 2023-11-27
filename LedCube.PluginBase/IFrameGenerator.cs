using System;

namespace LedCube.Plugin.Base;

public interface IFrameGenerator : IDisposable
{
    public TimeSpan? FrameTime { get; }

    void Initialize(GeneratorCubeConfiguration config);

    void AnimationStart(AnimationContext animationContext);

    void DrawFrame(FrameContext frameContext);

    void AnimationEnd(AnimationContext animationContext);
}