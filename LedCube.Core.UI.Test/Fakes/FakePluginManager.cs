using System;
using System.Collections.Generic;
using System.Linq;
using LedCube.PluginBase;
using LedCube.PluginHost;

namespace LedCube.Core.UI.Test.Fakes;

/// <summary>
/// Minimal IPluginManager — PlaylistService stores it but does not use it for the
/// playlist-navigation logic under test, so empty implementations suffice.
/// </summary>
public sealed class FakePluginManager : IPluginManager
{
    public IEnumerable<FrameGeneratorEntry> AllFrameGeneratorInfos() => Enumerable.Empty<FrameGeneratorEntry>();
    public IFrameGenerator GetFrameGenerator(FrameGeneratorEntry entry) => throw new NotSupportedException();
}
