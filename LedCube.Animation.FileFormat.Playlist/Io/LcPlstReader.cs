using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Common.Io;
using LedCube.Animation.FileFormat.Playlist.Io.Versions.V1;
using LedCube.Animation.FileFormat.Playlist.Model;
using PlaylistModel = LedCube.Animation.FileFormat.Playlist.Model.Playlist;

namespace LedCube.Animation.FileFormat.Playlist.Io;

/// <summary>
/// Reads a <c>.lcplst</c> container from a stream into the current <see cref="Playlist"/> model.
/// No format version is exposed to callers; version dispatch is internal.
/// </summary>
public static class LcPlstReader
{
    public static PlaylistModel Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);

        var (manifest, entries) = ReadManifestAndEntries(archive);

        var thumbnail = ZipEntries.ReadOptional(archive, LcPlstFormat.ThumbnailEntry);
        var extraEntries = ReadExtraEntries(archive);

        PlaylistValidator.Validate(manifest, entries);

        return new PlaylistModel(
            manifest,
            new ReadOnlyCollection<PlaylistEntry>(entries),
            thumbnail,
            extraEntries);
    }

    /// <summary>
    /// Reads only the manifest metadata (name, author, repeat mode, ...) without materializing the
    /// entry list as live data. Cheap; useful when browsing a library of playlists.
    /// </summary>
    public static PlaylistManifest ReadManifest(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        var (manifest, _) = ReadManifestAndEntries(archive);
        return manifest;
    }

    private static (PlaylistManifest Manifest, List<PlaylistEntry> Entries) ReadManifestAndEntries(ZipArchive archive)
    {
        var manifestBytes = ZipEntries.ReadRequired(archive, LcPlstFormat.ManifestEntry);
        var version = FormatJson.PeekFormatVersion(manifestBytes);

        return version switch
        {
            1 => ReadV1.Map(FormatJson.Deserialize<ManifestV1Dto>(manifestBytes)),
            _ => throw new UnsupportedFormatVersionException(version, LcPlstFormat.CurrentVersion),
        };
    }

    private static IReadOnlyDictionary<string, byte[]>? ReadExtraEntries(ZipArchive archive)
    {
        Dictionary<string, byte[]>? extras = null;
        foreach (var entry in archive.Entries)
        {
            var name = entry.FullName;
            if (LcPlstFormat.KnownEntries.Contains(name) || name.EndsWith('/'))
            {
                continue;
            }

            extras ??= new Dictionary<string, byte[]>(StringComparer.Ordinal);
            extras[name] = ZipEntries.ReadAll(entry);
        }

        return extras;
    }
}
