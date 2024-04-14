using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.TextWriter;

public class TextWriterPlugin : IPlugin
{
    public string Name => "TextWriter Animation";
    public string Description => "TextWriter Animation";
    
    public void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
    }
}