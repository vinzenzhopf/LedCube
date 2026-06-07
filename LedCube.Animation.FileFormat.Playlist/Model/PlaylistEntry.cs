using System;
using System.Collections.Generic;
using System.Linq;

namespace LedCube.Animation.FileFormat.Playlist.Model;

/// <summary>
/// A single playlist entry: a plugin <c>IFrameGenerator</c> instance plus its config. File
/// animations are not a separate kind — they are the FileAnimation generator with its file
/// referenced through <see cref="Config"/>. The format-layer counterpart of the UI's
/// <c>PlaylistEntry</c>; pure data with no plugin resolution.
/// </summary>
public sealed class PlaylistEntry : IEquatable<PlaylistEntry>
{
    private static readonly IReadOnlyDictionary<string, object?> EmptyConfig =
        new Dictionary<string, object?>();

    /// <summary>Opaque, stable id (UUID recommended) for external referencing. May be null.</summary>
    public string? Id { get; }

    /// <summary>Display label. Not unique, not used for lookup. May be null.</summary>
    public string? InstanceName { get; }

    /// <summary>Full type name of the <c>IFrameGenerator</c>, resolved via the plugin manager.</summary>
    public string TypeName { get; }

    /// <summary>Per-entry repeats before auto-advance: 0 = infinite, 1 = once, N = N times.</summary>
    public int RepeatCount { get; }

    /// <summary>Frame period override in microseconds; null uses the entry's native frame time.</summary>
    public uint? FrameTimeUsOverride { get; }

    /// <summary>
    /// The generator's config as normalized primitives (string / long / double / bool / null),
    /// keyed by config-descriptor key. Coerced to descriptor types by the consumer.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Config { get; }

    public PlaylistEntry(
        string typeName,
        string? id = null,
        string? instanceName = null,
        int repeatCount = 1,
        uint? frameTimeUsOverride = null,
        IReadOnlyDictionary<string, object?>? config = null)
    {
        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        Id = id;
        InstanceName = instanceName;
        RepeatCount = repeatCount;
        FrameTimeUsOverride = frameTimeUsOverride;
        Config = config ?? EmptyConfig;
    }

    public bool Equals(PlaylistEntry? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id
               && InstanceName == other.InstanceName
               && TypeName == other.TypeName
               && RepeatCount == other.RepeatCount
               && FrameTimeUsOverride == other.FrameTimeUsOverride
               && ConfigEquals(Config, other.Config);
    }

    public override bool Equals(object? obj) => Equals(obj as PlaylistEntry);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Id);
        hash.Add(InstanceName);
        hash.Add(TypeName);
        hash.Add(RepeatCount);
        hash.Add(FrameTimeUsOverride);
        hash.Add(Config.Count);
        return hash.ToHashCode();
    }

    private static bool ConfigEquals(
        IReadOnlyDictionary<string, object?> a,
        IReadOnlyDictionary<string, object?> b)
    {
        if (a.Count != b.Count) return false;
        foreach (var (key, valueA) in a)
        {
            if (!b.TryGetValue(key, out var valueB)) return false;
            if (!Equals(valueA, valueB)) return false;
        }

        return true;
    }
}
