using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace LedCube.Core.UI.Services.Playlist;

/// <summary>
/// Tracks the playlist "document": which <c>.lcplst</c> file the live <see cref="IPlaylistService"/>
/// is bound to, its metadata, and whether it has unsaved changes. Bridges file load/save (via
/// <see cref="IPlaylistFileConverter"/>) to the live playlist.
/// </summary>
public interface IPlaylistSessionService : INotifyPropertyChanged
{
    /// <summary>Metadata of the current playlist (name/author/description/created).</summary>
    PlaylistMetadata Metadata { get; }

    /// <summary>The file the current playlist is bound to, or null for an unsaved playlist.</summary>
    string? SourceFilePath { get; }

    /// <summary>True when the live playlist has changes not yet written to <see cref="SourceFilePath"/>.</summary>
    bool IsDirty { get; }

    /// <summary>Replaces the current metadata and marks the playlist dirty.</summary>
    void UpdateMetadata(PlaylistMetadata metadata);

    /// <summary>Reads a <c>.lcplst</c> file and applies it to the live playlist (entries, repeat mode, metadata).</summary>
    Task LoadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>Writes the live playlist back to <see cref="SourceFilePath"/>. Throws if there is none.</summary>
    Task SaveAsync(CancellationToken cancellationToken = default);

    /// <summary>Writes the live playlist to <paramref name="filePath"/> and binds the session to it.</summary>
    Task SaveToAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>Detaches from the current file (keeps entries), making it an unsaved playlist.</summary>
    void DetachFromFile();
}
