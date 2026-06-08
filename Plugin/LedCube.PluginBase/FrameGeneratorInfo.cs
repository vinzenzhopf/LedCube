using System;
using System.Collections.Generic;

namespace LedCube.PluginBase;

// Declarative timing metadata, evaluated by tooling/previews WITHOUT instantiating the generator.
// Resolution order used by consumers, for both frame time and frame count:
//   1. the static value (FrameTime / FrameCount) when the rate/length is fixed,
//   2. else the estimator (EstimateFrameTime / EstimateFrameCount) computed from the cube + config,
//   3. else null — genuinely dynamic (interactive/infinite); shown as unknown.
public sealed record FrameGeneratorInfo(
    string Name,
    string Description,
    IReadOnlyList<AnimationConfigDescriptor>? ConfigDescriptors = null,
    // Fixed frame period, when constant. Null => use EstimateFrameTime or treat as dynamic.
    TimeSpan? FrameTime = null,
    // Fixed total frame count, when statically known. Usually null (size/config dependent or dynamic).
    int? FrameCount = null,
    // Pure estimator for the frame period from cube + config; used when FrameTime is null.
    Func<CubeInfo, AnimationConfig, TimeSpan?>? EstimateFrameTime = null,
    // Pure estimator for the total frame count from cube + config; used when FrameCount is null.
    Func<CubeInfo, AnimationConfig, int?>? EstimateFrameCount = null);