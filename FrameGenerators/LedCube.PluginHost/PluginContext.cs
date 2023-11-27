using System.Collections.Concurrent;
using System.Reflection;
using LedCube.Plugin.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace LedCube.PluginHost;

public interface IPluginContext
{
    public void UnloadAll();
    public void LoadAssemblies();
    public TType GetService<TType>() where TType : class;
    public IEnumerable<TType> GetServices<TType>() where TType : class;
}

public class PluginContext : IPluginContext
{
    //Inspired from
    //https://github.com/GOATS2K/Coral/tree/main/src/Coral.PluginHost
    
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PluginContext> _logger;
    private readonly PluginOptions _pluginOptions;
    
    private readonly ConcurrentDictionary<LoadedPlugin, ServiceProvider> _loadedPlugins = new();
    
    public PluginContext(ILogger<PluginContext> logger, IServiceProvider serviceProvider, IOptions<PluginOptions> pluginOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _pluginOptions = pluginOptions.Value;
    }
    
    public TType GetService<TType>() where TType : class
    {
        var targetPlugin = _loadedPlugins.Keys.FirstOrDefault(k => k.Assembly.GetExportedTypes().Any(x => x == typeof(TType)));
        ArgumentNullException.ThrowIfNull(targetPlugin);
        var serviceProvider = _loadedPlugins[targetPlugin];
        return serviceProvider.GetRequiredService<TType>();
    }

    public IEnumerable<TType> GetServices<TType>() where TType : class
    {
        return _loadedPlugins.Keys
            .Where(k => k.Assembly.GetExportedTypes().Any(x => x == typeof(TType)))
            .Select(p => _loadedPlugins[p].GetRequiredService<TType>());
    }
    
    

    public void LoadAssemblies()
    {
        // load plugin via PluginLoader
        if (string.IsNullOrWhiteSpace(_pluginOptions.Path))
        {
            _logger.LogError("Plugin Loader: Plugin directory is empty. Skip plugin loading...");
            return;
        }
        if (!Directory.Exists(_pluginOptions.Path))
        {
            _logger.LogError("Plugin Loader: Plugin directory does not exist. Skip plugin loading...");
            return;
        }
        var assemblyDirectories = Directory.GetDirectories(_pluginOptions.Path);
        foreach (var assemblyDirectoryToLoad in assemblyDirectories)
        {
            var pluginLoader = new PluginLoader(_serviceProvider.GetService<ILogger<PluginLoader>>()!);
            var loadedPlugin = pluginLoader.LoadPluginAssemblies(assemblyDirectoryToLoad);
            if (!loadedPlugin.HasValue)
            {
                continue;
            }
            
            var storedPlugin = new LoadedPlugin(
                loadedPlugin.Value.Assembly,
                loadedPlugin.Value.Plugin,
                pluginLoader
            );

            //ConfigurePluginConfiguration
            var configFile = $"{loadedPlugin.Value.Assembly.GetName().Name}.json";
            
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(assemblyDirectoryToLoad);
            configurationBuilder.AddJsonFile(configFile, optional: true, reloadOnChange: true);
            loadedPlugin.Value.Plugin.ConfigureAppConfiguration(configurationBuilder);
            configurationBuilder.Build();
            
            //Configure Services
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(_serviceProvider.GetRequiredService<Serilog.ILogger>(), false);
            });
            serviceCollection.AddSingleton(_serviceProvider.GetRequiredService<ILoggerFactory>());
            loadedPlugin.Value.Plugin.ConfigureServices(serviceCollection);
            
            // allow plugins to access host services via proxy
            // it is important to note that the ServiceProxy in the plugin service collection
            // would normally contain a reference to its own service provider
            // so here we are telling the service collection to create the proxy
            // using the service provider injected in this class
            serviceCollection.AddScoped<IHostServiceProxy, HostServiceProxy>(_ => new HostServiceProxy(_serviceProvider));
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            
            //TODO Register/Load IHostedService and other background stuff. 
            
            _loadedPlugins.TryAdd(storedPlugin, serviceProvider);
            // finally, register event handlers
            // RegisterEventHandlersOnPlugin(serviceProvider);
        }
    }
    

    public void UnloadAll()
    {
        foreach (var (plugin, serviceProvider) in _loadedPlugins)
        {
            // UnregisterEventHandlersOnPlugin(serviceProvider);
            UnloadPlugin(plugin);
        }
    }

    private void UnloadPlugin(LoadedPlugin plugin)
    {
        _logger.LogInformation("Unloading plugin: {PluginName}", plugin.Plugin.Name);

        _loadedPlugins.Remove(plugin, out _);
        plugin.Loader.Unload();
    }

    
    public record LoadedPlugin(
        Assembly Assembly,
        IPlugin Plugin,
        PluginLoader Loader
    );
}