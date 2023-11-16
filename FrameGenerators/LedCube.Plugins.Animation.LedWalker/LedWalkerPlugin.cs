using LedCube.Plugin.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.LedWalker;

public class LedWalkerPlugin : IPlugin
{
    public string Name => "Led Walker Animation";
    public string Description => "Walks one led through the cube.";
    
    public void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}