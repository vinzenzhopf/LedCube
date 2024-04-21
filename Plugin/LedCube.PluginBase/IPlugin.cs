using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.PluginBase;

/// <summary>
/// Entry Interface that each Plugin has to implement to be loaded.
/// </summary>
public interface IPlugin
{
    string Name { get; }
    string Description { get; }

    void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder);
    void ConfigureServices(IServiceCollection serviceCollection);
}
