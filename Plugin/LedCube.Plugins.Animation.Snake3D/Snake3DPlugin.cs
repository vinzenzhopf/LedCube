using CommunityToolkit.Mvvm.Messaging;
using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.Snake3D;

public class Snake3DPlugin : IPlugin
{
    string IPlugin.Name => "Snake3D Animation";
    string IPlugin.Description => "Snake3D Animation";
    
    void IPlugin.ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("LedCube.Plugins.Animation.Snake3D.json");
    }

    void IPlugin.ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<Snake3DConfiguration>()
            .BindConfiguration(Snake3DConfiguration.SectionName);
    }
}