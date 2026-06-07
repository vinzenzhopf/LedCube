using System;
using System.Collections.Generic;
using System.Text.Json;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Playlist.Model;

namespace LedCube.Animation.FileFormat.Playlist.Io.Versions.V1;

/// <summary>
/// Maps the frozen <see cref="ManifestV1Dto"/> onto the current in-memory model. Because v1 is the
/// current version there is no migration hop yet; when v2 lands a <c>MigrateV1ToV2</c> pure
/// function slots in between this mapper and the current model.
/// </summary>
internal static class ReadV1
{
    public static (PlaylistManifest Manifest, List<PlaylistEntry> Entries) Map(ManifestV1Dto dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
        {
            throw new InvalidFileFormatException("Manifest field 'name' is required and must be non-empty.");
        }

        var entries = new List<PlaylistEntry>(dto.Entries?.Count ?? 0);
        if (dto.Entries is not null)
        {
            for (var i = 0; i < dto.Entries.Count; i++)
            {
                entries.Add(MapEntry(dto.Entries[i], i));
            }
        }

        var manifest = new PlaylistManifest
        {
            Name = dto.Name,
            Author = dto.Author,
            Description = dto.Description,
            CreatedUtc = dto.CreatedUtc,
            RepeatMode = ParseRepeatMode(dto.RepeatMode),
            ExtraFields = dto.Extra is { Count: > 0 } ? dto.Extra : null,
        };

        return (manifest, entries);
    }

    private static PlaylistEntry MapEntry(EntryV1Dto dto, int index)
    {
        if (string.IsNullOrEmpty(dto.TypeName))
        {
            throw new InvalidFileFormatException($"Entry {index} is missing the required 'typeName' field.");
        }

        IReadOnlyDictionary<string, object?>? config = null;
        if (dto.Config is { Count: > 0 })
        {
            var normalized = new Dictionary<string, object?>(dto.Config.Count);
            foreach (var (key, value) in dto.Config)
            {
                normalized[key] = Normalize(value);
            }

            config = normalized;
        }

        return new PlaylistEntry(
            typeName: dto.TypeName,
            id: dto.Id,
            instanceName: dto.InstanceName,
            repeatCount: dto.RepeatCount,
            frameTimeUsOverride: dto.FrameTimeUsOverride,
            config: config);
    }

    /// <summary>Unknown or absent values fall back to the documented default (LoopWholePlaylist).</summary>
    private static PlaylistRepeatMode ParseRepeatMode(string? value) =>
        Enum.TryParse<PlaylistRepeatMode>(value, ignoreCase: false, out var mode)
            ? mode
            : PlaylistRepeatMode.LoopWholePlaylist;

    /// <summary>Collapses a deserialized JSON value into a CLR primitive (string / long / double / bool / null).</summary>
    private static object? Normalize(object? raw) => raw switch
    {
        JsonElement e => e.ValueKind switch
        {
            JsonValueKind.String => e.GetString(),
            JsonValueKind.Number => e.TryGetInt64(out var l) ? (object)l : e.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            // Objects/arrays are not expected in config; preserve their raw JSON text.
            _ => e.GetRawText(),
        },
        _ => raw,
    };
}
