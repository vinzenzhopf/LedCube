using System.IO;
using LedCube.Animation.FileFormat.Playlist.Io;
using LedCube.Animation.FileFormat.Playlist.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;

namespace LedCube.Animation.FileFormat.Test.Playlist;

public class ReaderApiTests
{
    private static PlaylistManifest ReadManifest(string manifestJson)
    {
        using var ms = RawZip.FromEntries(RawZip.Text("manifest.json", manifestJson));
        return LcPlstReader.ReadManifest(ms);
    }

    [Fact]
    public void ReadManifest_ReturnsMetadataWithoutEntries()
    {
        var bytes = PlaylistBuilder.WriteToBytes(
            PlaylistBuilder.Playlist(PlaylistBuilder.Manifest(name: "My show"), PlaylistBuilder.PluginEntry()));

        using var ms = new MemoryStream(bytes);
        var manifest = LcPlstReader.ReadManifest(ms);

        Assert.Equal("My show", manifest.Name);
    }

    [Theory]
    [InlineData("StopAtEnd", PlaylistRepeatMode.StopAtEnd)]
    [InlineData("LoopWholePlaylist", PlaylistRepeatMode.LoopWholePlaylist)]
    [InlineData("RepeatCurrentEntry", PlaylistRepeatMode.RepeatCurrentEntry)]
    [InlineData("FairRandomPlay", PlaylistRepeatMode.FairRandomPlay)]
    [InlineData("TrueRandomPlay", PlaylistRepeatMode.TrueRandomPlay)]
    public void RepeatMode_KnownValues_Parsed(string value, PlaylistRepeatMode expected)
    {
        var json = $$"""{ "formatVersion": 1, "name": "X", "repeatMode": "{{value}}", "entries": [] }""";
        Assert.Equal(expected, ReadManifest(json).RepeatMode);
    }

    [Fact]
    public void RepeatMode_Absent_DefaultsToLoopWholePlaylist()
    {
        var json = """{ "formatVersion": 1, "name": "X", "entries": [] }""";
        Assert.Equal(PlaylistRepeatMode.LoopWholePlaylist, ReadManifest(json).RepeatMode);
    }

    [Fact]
    public void RepeatMode_Unknown_FallsBackToDefault()
    {
        var json = """{ "formatVersion": 1, "name": "X", "repeatMode": "Bogus", "entries": [] }""";
        Assert.Equal(PlaylistRepeatMode.LoopWholePlaylist, ReadManifest(json).RepeatMode);
    }

    [Fact]
    public void Config_NullValue_Preserved()
    {
        var json = """
        { "formatVersion": 1, "name": "X",
          "entries": [ { "typeName": "T", "config": { "Opt": null } } ] }
        """;
        using var ms = RawZip.FromEntries(RawZip.Text("manifest.json", json));
        var entry = LcPlstReader.Read(ms).Entries[0];

        Assert.True(entry.Config.ContainsKey("Opt"));
        Assert.Null(entry.Config["Opt"]);
    }

    [Fact]
    public void Reader_PreservesUnknownManifestField_OnRoundTrip()
    {
        var json = """
        { "formatVersion": 1, "name": "X", "futureField": { "a": 1 }, "entries": [] }
        """;
        using var ms = RawZip.FromEntries(RawZip.Text("manifest.json", json));
        var playlist = LcPlstReader.Read(ms);

        var rewritten = PlaylistBuilder.WriteToBytes(playlist);
        using var doc = System.Text.Json.JsonDocument.Parse(RawZip.ReadEntry(rewritten, "manifest.json"));
        Assert.True(doc.RootElement.TryGetProperty("futureField", out _));
    }
}
