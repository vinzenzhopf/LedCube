using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LedCube.PluginBase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LedCube.PluginHost;

public class PluginManager(ILogger<PluginManager> logger, IPluginHostContext context, IServiceProvider serviceProvider)
    : IPluginManager
{
    public IEnumerable<FrameGeneratorEntry> AllFrameGeneratorInfos()
    {
        var list = new List<FrameGeneratorEntry>();
        foreach (var typeInfo in context.EntriesImmutable
                     .Select(x => x.FrameGeneratorType)
                     .Where(x => x is not null)
                     .Cast<TypeInfo>())
        {
            try
            {
                var info = (FrameGeneratorInfo) typeInfo.GetProperty("Info")?.GetValue(null, null)!;
                list.Add(new FrameGeneratorEntry(info, typeInfo));
            }
            catch (Exception e)
            {
                logger.LogError(e, "FrameGenerator {FrameGeneratorName} does not have property {FrameGeneratorPropertyInfo}",
                    typeInfo.Name, nameof(IFrameGenerator.Info));
            }
        }
        return list;
    }

    public IFrameGenerator GetFrameGenerator(FrameGeneratorEntry entry)
    {
        return (IFrameGenerator)serviceProvider.GetRequiredService(entry.TypeInfo);
    }
}


public record FrameGeneratorEntry(FrameGeneratorInfo Info, TypeInfo TypeInfo);

public interface IPluginManager
{
    IEnumerable<FrameGeneratorEntry> AllFrameGeneratorInfos();

    IFrameGenerator GetFrameGenerator(FrameGeneratorEntry entry);
}

