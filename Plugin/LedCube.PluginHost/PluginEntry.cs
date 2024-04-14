using System.Reflection;
using LedCube.PluginBase;

namespace LedCube.PluginHost;

public class PluginEntry(TypeInfo pluginType, IPlugin pluginInstance) : IPluginEntry
{
    public TypeInfo PluginType { get; } = pluginType;
    public IPlugin PluginInstance { get; } = pluginInstance;
    public TypeInfo? FrameGeneratorType { get; set; }
}

public interface IPluginEntry
{
    TypeInfo PluginType { get; } 
    IPlugin PluginInstance { get; }
    TypeInfo? FrameGeneratorType { get; }
}