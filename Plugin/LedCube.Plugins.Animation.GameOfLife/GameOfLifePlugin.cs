using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.GameOfLife;

public class GameOfLifePlugin : IPlugin
{
    string IPlugin.Name => "Game of Life";
    string IPlugin.Description => "Conway's Game of Life in 3D";
    
    void IPlugin.ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("LedCube.Plugins.Animation.GameOfLife.json");
    }

    void IPlugin.ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<GameOfLifeConfiguration>()
            .BindConfiguration(GameOfLifeConfiguration.SectionName);
    }
}