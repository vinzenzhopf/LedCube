using System;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.PluginBase;

public record AnimationContext(
    TimeSpan TargetFrameTime,
    double ElapsedTimeUs,
    ICubeData CubeData
);