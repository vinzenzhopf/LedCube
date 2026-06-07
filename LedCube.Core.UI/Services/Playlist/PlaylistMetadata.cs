using System;

namespace LedCube.Core.UI.Services.Playlist;

/// <summary>
/// Display/authoring metadata of a playlist, mirroring the <c>.lcplst</c> manifest fields that
/// are not per-entry. Kept separate from the entry collection so the UI can edit it independently.
/// </summary>
public sealed record PlaylistMetadata(
    string Name,
    string? Author = null,
    string? Description = null,
    DateTimeOffset? CreatedUtc = null)
{
    /// <summary>A blank, unnamed playlist's metadata.</summary>
    public static PlaylistMetadata Empty { get; } = new(string.Empty);
}
