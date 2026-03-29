using System.Collections.Generic;

namespace LedCube.PluginBase;

public class AnimationConfig : Dictionary<string, object?>
{
    public AnimationConfig() { }

    public AnimationConfig(AnimationConfig other) : base(other) { }

    public T? Get<T>(string key) where T : struct
    {
        return TryGetValue(key, out var val) && val is T t ? t : null;
    }

    public string? GetString(string key)
    {
        return TryGetValue(key, out var val) ? val?.ToString() : null;
    }

    public static AnimationConfig FromDescriptors(IReadOnlyList<AnimationConfigDescriptor> descriptors)
    {
        var config = new AnimationConfig();
        foreach (var d in descriptors)
            config[d.Key] = d.DefaultValue;
        return config;
    }
}
