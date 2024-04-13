using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.GameOfLife;

public class GameOfLifePlugin : IPlugin
{
    public string Name => "Game of Life";
    public string Description => "Conway's Game of Life in 3D";
    
    public void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("LedCube.Plugins.Animation.GameOfLife.json");
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<GameOfLifeConfiguration>()
            .BindConfiguration(GameOfLifeConfiguration.SectionName);
    }
}