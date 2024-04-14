using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.Snake3D;

public class Snake3DPlugin : IPlugin
{
    public string Name => "Snake3D Animation";
    public string Description => "Snake3D Animation";
    
    public void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}