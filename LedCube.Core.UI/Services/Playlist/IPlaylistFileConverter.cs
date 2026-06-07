using System.Collections.Generic;
using FilePlaylist = LedCube.Animation.FileFormat.Playlist.Model.Playlist;

namespace LedCube.Core.UI.Services.Playlist;

/// <summary>
/// Result of mapping a loaded <c>.lcplst</c> model onto live UI playlist state. Entries whose
/// plugin type could not be resolved are reported in <see cref="UnresolvedTypeNames"/> and omitted
/// from <see cref="Entries"/>.
/// </summary>
public sealed record PlaylistLoadResult(
    IReadOnlyList<PlaylistEntry> Entries,
    PlaylistRepeatMode RepeatMode,
    PlaylistMetadata Metadata,
    IReadOnlyList<string> UnresolvedTypeNames);

/// <summary>
/// Bridges the pure <c>.lcplst</c> format model and the live UI playlist (resolving plugin types
/// via the plugin manager, coercing config to descriptor types, and resolving file references
/// against the library). Lives in the UI layer because the format library is plugin-agnostic.
/// </summary>
public interface IPlaylistFileConverter
{
    /// <summary>
    /// Maps a loaded format model onto live UI entries + metadata. <paramref name="playlistDirectory"/>
    /// is the folder the <c>.lcplst</c> was read from (used as a fallback for relative file refs);
    /// may be null for in-memory sources.
    /// </summary>
    PlaylistLoadResult ToUiPlaylist(FilePlaylist model, string? playlistDirectory);

    /// <summary>
    /// Builds a format model from live UI state, ready to write. File references are stored relative
    /// to the library (or <paramref name="playlistDirectory"/>) when possible, else as absolute paths.
    /// </summary>
    FilePlaylist ToModel(
        IReadOnlyList<PlaylistEntry> entries,
        PlaylistRepeatMode repeatMode,
        PlaylistMetadata metadata,
        string? playlistDirectory);
}
