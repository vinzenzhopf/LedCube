using System.Collections.Generic;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Playlist.Model;

namespace LedCube.Animation.FileFormat.Playlist.Io;

/// <summary>
/// Enforces the <c>.lcplst</c> spec constraints. The reader validates what it parses and the
/// writer guarantees the same constraints before serializing.
/// </summary>
internal static class PlaylistValidator
{
    public static void Validate(PlaylistManifest manifest, IReadOnlyList<PlaylistEntry> entries)
    {
        if (string.IsNullOrEmpty(manifest.Name))
        {
            throw new InvalidFileFormatException("Manifest field 'name' is required and must be non-empty.");
        }

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (string.IsNullOrEmpty(entry.TypeName))
            {
                throw new InvalidFileFormatException($"Entry {i} is missing the required 'typeName' field.");
            }

            if (entry.RepeatCount < 0)
            {
                throw new InvalidFileFormatException(
                    $"Entry {i} ('{entry.InstanceName ?? entry.TypeName}') has a negative repeatCount ({entry.RepeatCount}).");
            }

            if (entry.FrameTimeUsOverride is 0)
            {
                throw new InvalidFileFormatException(
                    $"Entry {i} ('{entry.InstanceName ?? entry.TypeName}') has frameTimeUsOverride 0; it must be > 0 or absent.");
            }
        }
    }
}
