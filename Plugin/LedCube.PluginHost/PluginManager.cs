using System;
using System.Collections.Generic;
using System.Reflection;
using LedCube.PluginBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LedCube.PluginHost;

public class PluginManager : IPluginManager
{
    private readonly ILogger<PluginManager> _logger;
    private readonly PluginHostContext _context;
    private readonly IServiceProvider _serviceProvider; 
    
    public PluginManager(ILogger<PluginManager> logger, PluginHostContext context, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _context = context;
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<FrameGeneratorEntry> AllFrameGeneratorInfos()
    {
        var list = new List<FrameGeneratorEntry>();
        foreach (var typeInfo in _context.FrameGeneratorTypes)
        {
            try
            {
                var info = (FrameGeneratorInfo) typeInfo.GetProperty("Info")?.GetValue(null, null)!;
                list.Add(new FrameGeneratorEntry(info, typeInfo));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "FrameGenerator {FrameGeneratorName} does not have property {FrameGeneratorPropertyInfo}",
                    typeInfo.Name, nameof(IFrameGenerator.Info));
            }
        }
        return list;
    }

    public IFrameGenerator GetFrameGenerator(FrameGeneratorEntry entry)
    {
        return (IFrameGenerator)_serviceProvider.GetRequiredService(entry.TypeInfo);
    }
}


public record FrameGeneratorEntry(FrameGeneratorInfo Info, TypeInfo TypeInfo);

public interface IPluginManager
{
    IEnumerable<FrameGeneratorEntry> AllFrameGeneratorInfos();

    IFrameGenerator GetFrameGenerator(FrameGeneratorEntry entry);
}

