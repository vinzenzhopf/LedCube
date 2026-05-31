namespace LedCube.Core.UI.Services.Playlist;

/// <summary>Broadcast when an entry's editable fields were changed in-place
/// (e.g. via the PluginConfig panel), so view models cached elsewhere can refresh.</summary>
public record PlaylistEntryEditedMessage(PlaylistEntry Entry);
