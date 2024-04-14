using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace LedCube.PluginHost;

public class PluginHostContext : IPluginHostContext
{
    public List<PluginEntry> Entries { get; set; } = [];
    
    public ImmutableList<IPluginEntry> EntriesImmutable => Entries.Cast<IPluginEntry>().ToImmutableList();
}