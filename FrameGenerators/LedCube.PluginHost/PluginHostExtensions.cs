using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LedCube.PluginHost;

public static class PluginHostExtensions
{
    public static void ConfigurePluginHost(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.Configure<PluginOptions>(
            configuration.GetSection(PluginOptions.Key));
        services.AddSingleton<IHostServiceProxy, HostServiceProxy>();
        services.AddSingleton<IPluginContext, PluginContext>();
        services.AddSingleton<IServiceProxy, ServiceProxy>();
        services.AddHostedService<PluginInitializer>();
    }
}