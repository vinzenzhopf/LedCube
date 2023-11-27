using System;

namespace LedCube.Plugin.Base;

public record AnimationContext(
    TimeSpan TargetFrameTime,
    long CurrentTicks
);