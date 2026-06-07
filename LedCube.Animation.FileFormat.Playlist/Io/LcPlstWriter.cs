using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Common.Io;
using LedCube.Animation.FileFormat.Playlist.Io.Versions.V1;
using LedCube.Animation.FileFormat.Playlist.Model;
using PlaylistModel = LedCube.Animation.FileFormat.Playlist.Model.Playlist;

namespace LedCube.Animation.FileFormat.Playlist.Io;

/// <summary>
/// Writes a <see cref="Playlist"/> to a <c>.lcplst</c> container, always at the current format
/// version. Entries without an <see cref="PlaylistEntry.Id"/> are assigned a fresh UUID so saved
/// playlists carry stable ids.
/// </summary>
public static class LcPlstWriter
{
    public static void Write(Stream stream, PlaylistModel playlist)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(playlist);

        // The writer MUST guarantee the same constraints the reader validates.
        PlaylistValidator.Validate(playlist.Manifest, playlist.Entries);

        var manifestBytes = FormatJson.SerializeToUtf8Bytes(BuildDto(playlist));

        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);

        ZipEntries.Write(archive, LcPlstFormat.ManifestEntry, manifestBytes, CompressionLevel.Optimal);

        if (playlist.Thumbnail is not null)
        {
            ZipEntries.Write(archive, LcPlstFormat.ThumbnailEntry, playlist.Thumbnail, CompressionLevel.Optimal);
        }

        foreach (var (name, data) in playlist.ExtraEntries)
        {
            if (LcPlstFormat.KnownEntries.Contains(name))
            {
                throw new InvalidFileFormatException(
                    $"Extra entry name '{name}' collides with a reserved container entry.");
            }

            ZipEntries.Write(archive, name, data, CompressionLevel.Optimal);
        }
    }

    private static ManifestV1Dto BuildDto(PlaylistModel playlist)
    {
        var manifest = playlist.Manifest;
        var entries = new List<EntryV1Dto>(playlist.Entries.Count);
        foreach (var entry in playlist.Entries)
        {
            entries.Add(new EntryV1Dto
            {
                Id = string.IsNullOrEmpty(entry.Id) ? Guid.NewGuid().ToString() : entry.Id,
                InstanceName = entry.InstanceName,
                TypeName = entry.TypeName,
                RepeatCount = entry.RepeatCount,
                FrameTimeUsOverride = entry.FrameTimeUsOverride,
                Config = entry.Config.Count > 0 ? new Dictionary<string, object?>(entry.Config) : null,
            });
        }

        return new ManifestV1Dto
        {
            FormatVersion = LcPlstFormat.CurrentVersion,
            Name = manifest.Name,
            Author = manifest.Author,
            Description = manifest.Description,
            CreatedUtc = manifest.CreatedUtc,
            RepeatMode = manifest.RepeatMode.ToString(),
            Entries = entries,
            Extra = manifest.ExtraFields is { Count: > 0 }
                ? new Dictionary<string, System.Text.Json.JsonElement>(manifest.ExtraFields)
                : null,
        };
    }
}
