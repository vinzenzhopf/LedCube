using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LedCube.PluginHost;

public class PluginHostContext : IPluginHostContext
{
    public List<PluginEntry> Entries { get; init; } = [];
    
    public ImmutableList<IPluginEntry> EntriesImmutable => Entries.Cast<IPluginEntry>().ToImmutableList();
}