using System.Collections.Generic;
using System.Text.Json;
using LedCube.Animation.FileFormat.Playlist.Io;
using LedCube.Animation.FileFormat.Playlist.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;

namespace LedCube.Animation.FileFormat.Test.Playlist;

public class WriterTests
{
    [Fact]
    public void Writer_AlwaysWritesCurrentVersion()
    {
        var bytes = PlaylistBuilder.WriteToBytes(
            PlaylistBuilder.Playlist(PlaylistBuilder.Manifest(), PlaylistBuilder.PluginEntry()));

        using var doc = JsonDocument.Parse(RawZip.ReadEntry(bytes, "manifest.json"));
        Assert.Equal(LcPlstFormat.CurrentVersion, doc.RootElement.GetProperty("formatVersion").GetInt32());
    }

    [Fact]
    public void Writer_GeneratesIdWhenAbsent()
    {
        var entry = PlaylistBuilder.PluginEntry(id: null);
        var bytes = PlaylistBuilder.WriteToBytes(PlaylistBuilder.Playlist(PlaylistBuilder.Manifest(), entry));

        var read = PlaylistBuilder.ReadFromBytes(bytes).Entries[0];
        Assert.False(string.IsNullOrEmpty(read.Id));
    }

    [Fact]
    public void Writer_KeepsExplicitId()
    {
        var entry = PlaylistBuilder.PluginEntry(id: "stable-id");
        var read = PlaylistBuilder.RoundTrip(PlaylistBuilder.Playlist(PlaylistBuilder.Manifest(), entry));

        Assert.Equal("stable-id", read.Entries[0].Id);
    }

    [Fact]
    public void Writer_OmitsEmptyConfig()
    {
        var bytes = PlaylistBuilder.WriteToBytes(
            PlaylistBuilder.Playlist(PlaylistBuilder.Manifest(), PlaylistBuilder.PluginEntry(config: null)));

        using var doc = JsonDocument.Parse(RawZip.ReadEntry(bytes, "manifest.json"));
        var entry = doc.RootElement.GetProperty("entries")[0];
        Assert.False(entry.TryGetProperty("config", out _));
    }

    [Fact]
    public void Writer_WritesCamelCaseFields()
    {
        var bytes = PlaylistBuilder.WriteToBytes(
            PlaylistBuilder.Playlist(PlaylistBuilder.Manifest(), PlaylistBuilder.PluginEntry()));

        using var doc = JsonDocument.Parse(RawZip.ReadEntry(bytes, "manifest.json"));
        Assert.True(doc.RootElement.TryGetProperty("repeatMode", out _));
        Assert.True(doc.RootElement.GetProperty("entries")[0].TryGetProperty("typeName", out _));
    }

    [Fact]
    public void Writer_RejectsReservedExtraEntryName()
    {
        var extras = new Dictionary<string, byte[]> { ["thumbnail.png"] = new byte[] { 1 } };
        var playlist = new PlaylistModel(
            PlaylistBuilder.Manifest(),
            new[] { PlaylistBuilder.PluginEntry() },
            extraEntries: extras);

        Assert.Throws<LedCube.Animation.FileFormat.Common.Exceptions.InvalidFileFormatException>(
            () => PlaylistBuilder.WriteToBytes(playlist));
    }
}
