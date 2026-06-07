using System;
using System.Collections.Generic;
using System.Linq;

namespace LedCube.Animation.FileFormat.Playlist.Model;

/// <summary>
/// The in-memory model of a <c>.lcplst</c> playlist: pure data. Mapping entries to live plugin
/// instances (resolving <see cref="PlaylistEntry.TypeName"/>, file paths, ...) is a consumer
/// concern handled downstream where the plugin manager and library live.
/// </summary>
public sealed class Playlist : IEquatable<Playlist>
{
    private static readonly IReadOnlyDictionary<string, byte[]> EmptyExtraEntries =
        new Dictionary<string, byte[]>();

    /// <summary>Manifest metadata (name, author, repeat mode, ...).</summary>
    public PlaylistManifest Manifest { get; }

    /// <summary>Entries in play order (array order is authoritative). May be empty.</summary>
    public IReadOnlyList<PlaylistEntry> Entries { get; }

    /// <summary>Optional thumbnail image bytes (<c>thumbnail.png</c>), preserved verbatim.</summary>
    public byte[]? Thumbnail { get; }

    /// <summary>
    /// Unknown ZIP entries preserved verbatim so a reader that re-saves the file does not lose
    /// data. Keyed by full entry name.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]> ExtraEntries { get; }

    public Playlist(
        PlaylistManifest manifest,
        IReadOnlyList<PlaylistEntry> entries,
        byte[]? thumbnail = null,
        IReadOnlyDictionary<string, byte[]>? extraEntries = null)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        Entries = entries ?? throw new ArgumentNullException(nameof(entries));
        Thumbnail = thumbnail;
        ExtraEntries = extraEntries ?? EmptyExtraEntries;
    }

    public bool Equals(Playlist? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Manifest.Equals(other.Manifest)
               && Entries.SequenceEqual(other.Entries)
               && ThumbnailEquals(Thumbnail, other.Thumbnail)
               && ExtraEntriesEqual(ExtraEntries, other.ExtraEntries);
    }

    public override bool Equals(object? obj) => Equals(obj as Playlist);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Manifest);
        hash.Add(Entries.Count);
        hash.Add(Thumbnail?.Length ?? -1);
        hash.Add(ExtraEntries.Count);
        return hash.ToHashCode();
    }

    private static bool ThumbnailEquals(byte[]? a, byte[]? b)
    {
        if (a is null || b is null) return a is null && b is null;
        return a.AsSpan().SequenceEqual(b);
    }

    private static bool ExtraEntriesEqual(
        IReadOnlyDictionary<string, byte[]> a,
        IReadOnlyDictionary<string, byte[]> b)
    {
        if (a.Count != b.Count) return false;
        foreach (var (key, valueA) in a)
        {
            if (!b.TryGetValue(key, out var valueB) || !valueA.AsSpan().SequenceEqual(valueB))
                return false;
        }

        return true;
    }
}
