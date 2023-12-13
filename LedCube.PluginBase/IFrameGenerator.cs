using System;

namespace LedCube.PluginBase;

public interface IFrameGenerator : IDisposable
{
    string Name { get; }

    string Description { get; }

    TimeSpan? FrameTime { get; }

    void Initialize(GeneratorCubeConfiguration config);

    void AnimationStart(AnimationContext animationContext);

    void DrawFrame(FrameContext frameContext);

    void AnimationEnd(AnimationContext animationContext);
}