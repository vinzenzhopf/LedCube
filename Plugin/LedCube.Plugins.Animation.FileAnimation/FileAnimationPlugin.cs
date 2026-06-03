using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugins.Animation.FileAnimation;

public class FileAnimationPlugin : IPlugin
{
    string IPlugin.Name => "File Animation";
    string IPlugin.Description => "Plays baked .lcanimraw animation files";

    void IPlugin.ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("LedCube.Plugins.Animation.FileAnimation.json", optional: true);
    }

    void IPlugin.ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddOptions<FileAnimationConfiguration>()
            .BindConfiguration(FileAnimationConfiguration.SectionName);
    }
}
