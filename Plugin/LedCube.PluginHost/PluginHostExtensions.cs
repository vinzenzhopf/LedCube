﻿using System.Collections.Immutable;
using System.Reflection;
using LedCube.PluginBase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedCube.PluginHost;

public static class PluginHostExtensions
{
    public static void SetupPluginHost(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PluginOptions>(
            configuration.GetSection(PluginOptions.Key));

        var assemblies = Directory
            .GetFiles(System.AppDomain.CurrentDomain.BaseDirectory, "*.dll", SearchOption.AllDirectories)
            .Select(Assembly.LoadFrom)
            .ToList();

        var frameGeneratorTypes = assemblies
            .SelectMany(a => a.DefinedTypes
                .Where(x =>
                    typeof(IFrameGenerator).IsAssignableFrom(x) &&
                    !x.IsInterface &&
                    !x.IsAbstract))
            .ToList();
        
        var pluginHostContext = new PluginHostContext(frameGeneratorTypes.ToImmutableList());
        
        foreach (var type in frameGeneratorTypes)
        {
            services.AddTransient(type);
        }

        services.AddSingleton(pluginHostContext);
        services.AddSingleton<IPluginManager, PluginManager>();

        // services.Scan(scan => scan
        //     .FromAssemblies(assemblies)
        //     .AddClasses(classes => classes.AssignableTo<IPlugin>(), true)
        //     .AsImplementedInterfaces()
        //     .WithSingletonLifetime());
        //
        // services.Scan(scan => scan
        //     .FromAssemblies(assemblies)
        //     .AddClasses(classes => classes.AssignableTo<IFrameGenerator>(), true)
        //     .AsImplementedInterfaces()
        //     .WithTransientLifetime());
        //
        // services.AddSingleton<IHostServiceProxy, HostServiceProxy>();
        // services.AddSingleton<IPluginContext, PluginContext>();
        // services.AddSingleton<IServiceProxy, ServiceProxy>();
        // services.AddHostedService<PluginInitializer>();
    }
    
    
}

public record PluginHostContext(ImmutableList<TypeInfo> FrameGeneratorTypes);