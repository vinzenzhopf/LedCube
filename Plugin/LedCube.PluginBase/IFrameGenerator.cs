using System;

namespace LedCube.PluginBase;

public interface IFrameGenerator : IDisposable
{
    static abstract FrameGeneratorInfo Info { get; }

    TimeSpan? FrameTime { get; }

    void Initialize(GeneratorCubeConfiguration config);

    void DrawFrame(FrameContext frameContext);

    void Start(AnimationContext animationContext);
    
    void End(AnimationContext animationContext);
    
    void Pause(AnimationContext animationContext);
    
    void Continue(AnimationContext animationContext);
    
    void ChangeTime(AnimationContext animationContext);
}