using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.SdfTest;

public class SdfTestPlugin : IPlugin
{
    public string Name => "SdfTest Animation";
    public string Description => "SdfTest Animation";
    
    public void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}