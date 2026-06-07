using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LedCube.Animation.FileFormat.Playlist.Model;

/// <summary>
/// The deserialized <c>manifest.json</c> metadata of a playlist, excluding the entry list
/// (exposed via <see cref="Playlist.Entries"/>). Always the current format version; no
/// <c>formatVersion</c> is exposed to callers.
/// </summary>
public sealed record PlaylistManifest
{
    /// <summary>Display name. Not unique, not used for lookup.</summary>
    public required string Name { get; init; }

    /// <summary>Optional free-text author.</summary>
    public string? Author { get; init; }

    /// <summary>Optional free-text description.</summary>
    public string? Description { get; init; }

    /// <summary>Optional UTC creation timestamp.</summary>
    public DateTimeOffset? CreatedUtc { get; init; }

    /// <summary>Auto-advance policy applied when an entry finishes. Defaults to LoopWholePlaylist.</summary>
    public PlaylistRepeatMode RepeatMode { get; init; } = PlaylistRepeatMode.LoopWholePlaylist;

    /// <summary>
    /// Unknown manifest fields preserved verbatim for round-tripping newer files through this
    /// reader. Null when the manifest had no unrecognized fields.
    /// </summary>
    public IReadOnlyDictionary<string, JsonElement>? ExtraFields { get; init; }

    public bool Equals(PlaylistManifest? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name
               && Author == other.Author
               && Description == other.Description
               && Nullable.Equals(CreatedUtc, other.CreatedUtc)
               && RepeatMode == other.RepeatMode
               && ExtraFieldsEqual(ExtraFields, other.ExtraFields);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(Author);
        hash.Add(Description);
        hash.Add(CreatedUtc);
        hash.Add(RepeatMode);
        hash.Add(ExtraFields?.Count ?? -1);
        return hash.ToHashCode();
    }

    private static bool ExtraFieldsEqual(
        IReadOnlyDictionary<string, JsonElement>? a,
        IReadOnlyDictionary<string, JsonElement>? b)
    {
        if (a is null || b is null) return a is null && b is null;
        if (a.Count != b.Count) return false;
        foreach (var (key, valueA) in a)
        {
            if (!b.TryGetValue(key, out var valueB)) return false;
            if (valueA.GetRawText() != valueB.GetRawText()) return false;
        }

        return true;
    }
}
