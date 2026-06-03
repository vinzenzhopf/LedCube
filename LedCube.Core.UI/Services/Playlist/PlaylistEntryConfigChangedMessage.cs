namespace LedCube.Core.UI.Services.Playlist;

/// <summary>Broadcast when a plugin config value of an entry was changed (e.g. a file path picked
/// in the PluginConfig panel), so the loaded generator can be reconfigured to reflect it.</summary>
public record PlaylistEntryConfigChangedMessage(PlaylistEntry Entry);
