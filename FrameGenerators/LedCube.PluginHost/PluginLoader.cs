using System.Data;
using System.Reflection;
using System.Runtime.Loader;
using LedCube.Plugin.Base;
using Microsoft.Extensions.Logging;

namespace LedCube.PluginHost;

public class PluginLoader : AssemblyLoadContext
{
    private readonly ILogger<PluginLoader> _logger;
    public PluginLoader(ILogger<PluginLoader> logger) : base(true)
    {
        _logger = logger;
    }

    public (Assembly Assembly, IPlugin Plugin)? LoadPluginAssemblies(string assemblyDirectory)
    {
        (Assembly Assembly, IPlugin Plugin)? assemblyGroup = null;
        foreach (var assemblyPath in Directory.GetFiles(assemblyDirectory, "*.dll"))
        {
            // if PluginBase is present, skip loading
            if (Path.GetFileName(assemblyPath).StartsWith("LedCube.PluginBase"))
            {
                _logger.LogWarning("LedCube.PluginBase assembly detected, please remove from plugin folder. " +
                                                    $"Skipping load of: {assemblyPath}" +
                                                    " to ensure plug-in can load.");
                continue;
            }

            var assembly = LoadFromAssemblyPath(assemblyPath);
            try
            {
                var types = assembly.GetTypes();
                var pluginCount = types.Count(t => typeof(IPlugin).IsAssignableFrom(t));
                if (pluginCount > 1)
                {
                    throw new ConstraintException("Cannot load assembly with more than 1 plugin.");
                }

                // if assembly has no plugins, continue, it's a needed dependency
                if (pluginCount == 0)
                {
                    _logger.LogDebug("Loaded plugin dependency: {assemblyName}", assembly.GetName().Name);
                    continue;
                }

                var pluginType = types.Single(t => typeof(IPlugin).IsAssignableFrom(t));
                var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                if (plugin != null)
                {
                    _logger.LogInformation("Loaded plugin: {name} - {description}", plugin.Name, plugin.Description);
                    assemblyGroup = (assembly, plugin);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                _logger.LogWarning("Exception thrown loading types for assembly: {AssemblyName}", assembly.GetName().Name);
            }
        }
        return assemblyGroup;
    }
}