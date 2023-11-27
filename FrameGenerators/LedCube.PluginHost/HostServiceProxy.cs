using Microsoft.Extensions.DependencyInjection;

namespace LedCube.PluginHost;

public class HostServiceProxy: IHostServiceProxy
{
    private readonly IServiceProvider _serviceProvider;

    public HostServiceProxy(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public TType GetHostService<TType>() where TType : class
    {
        using var scope = _serviceProvider.CreateScope();
        //TODO Limit Access to the base service provider
        // var assemblyName = typeof(TType).Assembly.GetName().Name;
        // if (assemblyName != "tbd")
        // {
        //     throw new ArgumentException("You may only access types belonging to the tbd assembly.");
        // }
        return scope.ServiceProvider.GetRequiredService<TType>();
    }
}
