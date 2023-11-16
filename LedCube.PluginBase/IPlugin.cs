using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.Plugin.Base;

public interface IPlugin
{
    string Name { get; }
    string Description { get; }

    public void ConfigureAppConfiguration(IConfigurationBuilder configurationBuilder);
    public void ConfigureServices(IServiceCollection serviceCollection);
}
