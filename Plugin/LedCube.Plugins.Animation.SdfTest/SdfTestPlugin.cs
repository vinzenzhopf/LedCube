using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.SdfTest;

public class SdfTestPlugin : IPlugin
{
    string IPlugin.Name => "SdfTest Animation";
    string IPlugin.Description => "SdfTest Animation";
    
}