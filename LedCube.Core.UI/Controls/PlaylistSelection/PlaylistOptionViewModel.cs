namespace LedCube.Core.UI.Controls.PlaylistSelection;

/// <summary>An entry in the playlist-selection dropdown: a discovered file, or the unsaved playlist.</summary>
public sealed class PlaylistOptionViewModel(string? filePath, string displayName)
{
    /// <summary>Full path to the <c>.lcplst</c> file, or null for the in-memory unsaved playlist.</summary>
    public string? FilePath { get; } = filePath;

    public string DisplayName { get; } = displayName;

    public bool IsUnsaved => FilePath is null;
}
