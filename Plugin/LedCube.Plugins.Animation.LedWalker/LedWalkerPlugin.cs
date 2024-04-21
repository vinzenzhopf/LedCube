using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.LedWalker;

public class LedWalkerPlugin : IPlugin
{
    string IPlugin.Name => "Led Walker Animation";
    string IPlugin.Description => "Walks one led through the cube.";
    
}