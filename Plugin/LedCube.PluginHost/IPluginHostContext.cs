using System.Collections.Immutable;

namespace LedCube.PluginHost;

public interface IPluginHostContext
{
    ImmutableList<IPluginEntry> EntriesImmutable { get; }
}