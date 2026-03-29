using System.Collections.Generic;

namespace LedCube.PluginBase;

public sealed record FrameGeneratorInfo(
    string Name,
    string Description,
    IReadOnlyList<AnimationConfigDescriptor>? ConfigDescriptors = null);