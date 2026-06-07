namespace LedCube.Animation.FileFormat.Playlist.Model;

/// <summary>
/// Playlist-level auto-advance policy applied when an entry finishes. The format-layer mirror of
/// the UI's <c>PlaylistRepeatMode</c>; consumers map between the two.
/// </summary>
public enum PlaylistRepeatMode
{
    /// <summary>Play through the playlist once, then stop at the last entry.</summary>
    StopAtEnd,

    /// <summary>Advance sequentially and wrap from the last entry back to the first.</summary>
    LoopWholePlaylist,

    /// <summary>Keep replaying the currently playing entry.</summary>
    RepeatCurrentEntry,

    /// <summary>Shuffle that guarantees every entry plays once before any repeats (fair shuffle).</summary>
    FairRandomPlay,

    /// <summary>Pick a fully random entry each time (repeats possible).</summary>
    TrueRandomPlay,
}
