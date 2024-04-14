using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LedCube.PluginHost;

public static class PluginHostExtensions
{
    public static void Initialize(this PluginHostContext pluginHostContext)
    {
        var pluginTypes = Directory
            .GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories)
            .Select(Assembly.LoadFrom)
            .Select(a => a.DefinedTypes
                .SingleOrDefault(x =>
                    typeof(IPlugin).IsAssignableFrom(x) &&
                    x is {IsInterface: false, IsAbstract: false}))
            .Where(x => x is not null)
            .Cast<TypeInfo>()
            .ToList();
        
        foreach (var pluginEntry in pluginTypes
                     .Select(InitializePlugin))
        {
            if (pluginEntry is not null)
            {
                pluginHostContext.Entries.Add(pluginEntry);        
            }
        }
        
    }

    private static PluginEntry? InitializePlugin(TypeInfo pluginType)
    {
        try
        {
            Log.Information("Loading Plugin {0} from Assembly {1}...", pluginType.Name, pluginType.Assembly.GetName());
            if (Activator.CreateInstance(pluginType)
                is not IPlugin pluginInstance)
            {
                Log.Error("Error creating instance of {0}. Skip loading Plugin.", pluginType.Name);
                return null;
            }
            return new PluginEntry(pluginType, pluginInstance);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error creating instance of {0}. Skip loading Plugin.", pluginType.Name);
            return null;
        }
    }

    public static void ConfigurePluginHost(this IConfigurationBuilder configurationBuilder, PluginHostContext pluginHostContext)
    {
        var entries = pluginHostContext.Entries.ToList();
        foreach (var pluginEntry in entries)
        {
            try
            {
                Log.Debug("Configure for Plugin {0}", pluginEntry.PluginType.Name);
                pluginEntry.PluginInstance.ConfigureAppConfiguration(configurationBuilder);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while running Configure on plugin {0}.", pluginEntry.PluginType.Name);
                pluginHostContext.Entries.Remove(pluginEntry);
            }
        }
    }
    
    public static void SetupPluginHost(this IServiceCollection services, PluginHostContext pluginHostContext)
    {
        foreach (var pluginEntry in pluginHostContext.Entries)
        {
            SetupPlugin(services, pluginEntry);
        }
        services.AddSingleton<IPluginHostContext>(pluginHostContext);
        services.AddSingleton<IPluginManager, PluginManager>();
    }

    private static void SetupPlugin(IServiceCollection services, PluginEntry pluginEntry)
    {
        try
        {
            Log.Debug("ConfigureServices for Plugin {0}", pluginEntry.PluginType.Name);
            pluginEntry.PluginInstance.ConfigureServices(services);
            
            var frameGeneratorType = pluginEntry.PluginType.Assembly
                .DefinedTypes
                .FirstOrDefault(x =>
                    typeof(IFrameGenerator).IsAssignableFrom(x) &&
                    x is {IsInterface: false, IsAbstract: false});
            
            if (frameGeneratorType is null) return;
            
            Log.Debug("Loading FrameGenerator {0} from Plugin {1}", frameGeneratorType.Name, pluginEntry.PluginType.Name);
            pluginEntry.FrameGeneratorType = frameGeneratorType;
            services.AddTransient(frameGeneratorType);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while running ConfigureServices on plugin {0}.", pluginEntry.PluginType.Name);
        }
    }
}