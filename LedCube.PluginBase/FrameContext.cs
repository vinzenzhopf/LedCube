using System;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Plugin.Base;

public record FrameContext(
    TimeSpan TargetFrameTime,
    TimeSpan LastFrameTime,
    long CurrentTicks,
    ICubeDataBuffer Buffer
);