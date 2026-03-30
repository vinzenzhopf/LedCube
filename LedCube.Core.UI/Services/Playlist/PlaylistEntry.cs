using System;
using System.Reflection;
using LedCube.PluginBase;

namespace LedCube.Core.UI.Services.Playlist;

public class PlaylistEntry
{
    public FrameGeneratorInfo Info { get; }
    public TypeInfo TypeInfo { get; }
    public string InstanceName { get; set; } = string.Empty;
    public AnimationConfig Config { get; }
    public int RepeatCount { get; set; } = 1;
    public TimeSpan? FrameTimeOverride { get; set; }

    public PlaylistEntry(FrameGeneratorInfo info, TypeInfo typeInfo)
    {
        Info = info;
        TypeInfo = typeInfo;
        Config = info.ConfigDescriptors is not null
            ? AnimationConfig.FromDescriptors(info.ConfigDescriptors)
            : new AnimationConfig();
    }

    public PlaylistEntry(PlaylistEntry other)
    {
        Info = other.Info;
        TypeInfo = other.TypeInfo;
        InstanceName = other.InstanceName;
        Config = new AnimationConfig(other.Config);
        RepeatCount = other.RepeatCount;
        FrameTimeOverride = other.FrameTimeOverride;
    }
}
