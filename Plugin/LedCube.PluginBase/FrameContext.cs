using System;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.PluginBase;

public record FrameContext(
    TimeSpan TargetFrameTime,
    TimeSpan LastFrameTime,
    double ElapsedTimeUs,
    ICubeDataBuffer Buffer
);