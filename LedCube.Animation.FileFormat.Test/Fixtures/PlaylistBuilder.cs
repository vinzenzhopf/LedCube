using System.Collections.Generic;
using System.IO;
using LedCube.Animation.FileFormat.Playlist.Io;
using LedCube.Animation.FileFormat.Playlist.Model;

namespace LedCube.Animation.FileFormat.Test.Fixtures;

/// <summary>Small builders and round-trip helpers for <c>.lcplst</c> model tests.</summary>
internal static class PlaylistBuilder
{
    public static PlaylistManifest Manifest(
        string name = "Test show",
        PlaylistRepeatMode repeatMode = PlaylistRepeatMode.LoopWholePlaylist) => new()
    {
        Name = name,
        RepeatMode = repeatMode,
    };

    public static PlaylistEntry PluginEntry(
        string typeName = "LedCube.Plugins.Animation.Snake3D.Snake3DAnimation",
        string? id = "id-snake",
        string? instanceName = "Snake",
        int repeatCount = 1,
        uint? frameTimeUsOverride = null,
        IReadOnlyDictionary<string, object?>? config = null) =>
        new(typeName, id, instanceName, repeatCount, frameTimeUsOverride, config);

    public static PlaylistEntry FileEntry(string filePath = "sunrise.lcanimraw", string? id = "id-file") =>
        new(
            typeName: "LedCube.Plugins.Animation.FileAnimation.FileAnimationGenerator",
            id: id,
            instanceName: "Sunrise",
            repeatCount: 1,
            frameTimeUsOverride: null,
            config: new Dictionary<string, object?> { ["FilePath"] = filePath });

    public static PlaylistModel Playlist(
        PlaylistManifest? manifest = null,
        params PlaylistEntry[] entries) =>
        new(manifest ?? Manifest(), entries);

    public static byte[] WriteToBytes(PlaylistModel playlist)
    {
        using var ms = new MemoryStream();
        LcPlstWriter.Write(ms, playlist);
        return ms.ToArray();
    }

    public static PlaylistModel ReadFromBytes(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        return LcPlstReader.Read(ms);
    }

    public static PlaylistModel RoundTrip(PlaylistModel playlist) => ReadFromBytes(WriteToBytes(playlist));
}
