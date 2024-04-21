using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.TextWriter;

public class TextWriterPlugin : IPlugin
{
    string IPlugin.Name => "TextWriter Animation";
    string IPlugin.Description => "TextWriter Animation";
    
}