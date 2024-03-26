using System;

namespace LedCube.PluginBase;

public record AnimationContext(
    TimeSpan TargetFrameTime,
    long CurrentTicks
);