using CommunityToolkit.Mvvm.Messaging;
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
        configurationBuilder.AddJsonFile("LedCube.Plugins.Animation.Snake3D.json");
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<Snake3DConfiguration>()
            .BindConfiguration(Snake3DConfiguration.SectionName);
    }
}