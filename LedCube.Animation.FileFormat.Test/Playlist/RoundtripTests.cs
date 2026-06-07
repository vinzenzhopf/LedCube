using System;
using System.Collections.Generic;
using LedCube.Animation.FileFormat.Playlist.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;

namespace LedCube.Animation.FileFormat.Test.Playlist;

public class RoundtripTests
{
    [Fact]
    public void Roundtrip_MinimalPlaylist_SingleEntry()
    {
        var playlist = PlaylistBuilder.Playlist(null, PlaylistBuilder.PluginEntry());

        var read = PlaylistBuilder.RoundTrip(playlist);

        Assert.Equal(playlist, read);
    }

    [Fact]
    public void Roundtrip_EmptyEntries_IsValid()
    {
        var playlist = PlaylistBuilder.Playlist(PlaylistBuilder.Manifest());

        var read = PlaylistBuilder.RoundTrip(playlist);

        Assert.Empty(read.Entries);
        Assert.Equal(playlist, read);
    }

    [Fact]
    public void Roundtrip_AllManifestFields_Present()
    {
        var manifest = new PlaylistManifest
        {
            Name = "Evening show",
            Author = "vinzenz",
            Description = "Sunset to deep-night sequence.",
            CreatedUtc = new DateTimeOffset(2026, 5, 31, 14, 0, 0, TimeSpan.Zero),
            RepeatMode = PlaylistRepeatMode.FairRandomPlay,
        };
        var playlist = PlaylistBuilder.Playlist(manifest, PlaylistBuilder.FileEntry(), PlaylistBuilder.PluginEntry());

        var read = PlaylistBuilder.RoundTrip(playlist);

        Assert.Equal(playlist, read);
    }

    [Fact]
    public void Roundtrip_EntryOrder_Preserved()
    {
        var playlist = PlaylistBuilder.Playlist(
            PlaylistBuilder.Manifest(),
            PlaylistBuilder.PluginEntry(typeName: "A", id: "a", instanceName: "A"),
            PlaylistBuilder.PluginEntry(typeName: "B", id: "b", instanceName: "B"),
            PlaylistBuilder.PluginEntry(typeName: "C", id: "c", instanceName: "C"));

        var read = PlaylistBuilder.RoundTrip(playlist);

        Assert.Collection(read.Entries,
            e => Assert.Equal("A", e.TypeName),
            e => Assert.Equal("B", e.TypeName),
            e => Assert.Equal("C", e.TypeName));
    }

    [Fact]
    public void Roundtrip_ConfigPrimitives_PreserveTypes()
    {
        var config = new Dictionary<string, object?>
        {
            ["Text"] = "hello",
            ["Count"] = 42L,
            ["Speed"] = 1.5,
            ["Enabled"] = true,
            ["Mode"] = "Octahedron",
        };
        var playlist = PlaylistBuilder.Playlist(
            PlaylistBuilder.Manifest(),
            PlaylistBuilder.PluginEntry(config: config));

        var read = PlaylistBuilder.RoundTrip(playlist).Entries[0].Config;

        Assert.Equal("hello", read["Text"]);
        Assert.Equal(42L, read["Count"]);
        Assert.Equal(1.5, read["Speed"]);
        Assert.Equal(true, read["Enabled"]);
        Assert.Equal("Octahedron", read["Mode"]);
    }

    [Fact]
    public void Roundtrip_FrameTimeOverrideAndRepeatCount_Preserved()
    {
        var entry = PlaylistBuilder.PluginEntry(repeatCount: 0, frameTimeUsOverride: 25000);
        var playlist = PlaylistBuilder.Playlist(PlaylistBuilder.Manifest(), entry);

        var read = PlaylistBuilder.RoundTrip(playlist).Entries[0];

        Assert.Equal(0, read.RepeatCount);
        Assert.Equal(25000u, read.FrameTimeUsOverride);
    }

    [Fact]
    public void Roundtrip_Thumbnail_Preserved()
    {
        var thumb = new byte[] { 0x89, 0x50, 0x4E, 0x47, 1, 2, 3 };
        var playlist = new PlaylistModel(
            PlaylistBuilder.Manifest(),
            new[] { PlaylistBuilder.PluginEntry() },
            thumbnail: thumb);

        var read = PlaylistBuilder.RoundTrip(playlist);

        Assert.Equal(thumb, read.Thumbnail);
    }

    [Fact]
    public void Roundtrip_UnknownZipEntry_Preserved()
    {
        var extras = new Dictionary<string, byte[]> { ["meta/notes.txt"] = new byte[] { 1, 2, 3, 4 } };
        var playlist = new PlaylistModel(
            PlaylistBuilder.Manifest(),
            new[] { PlaylistBuilder.PluginEntry() },
            extraEntries: extras);

        var read = PlaylistBuilder.RoundTrip(playlist);

        Assert.True(read.ExtraEntries.ContainsKey("meta/notes.txt"));
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, read.ExtraEntries["meta/notes.txt"]);
    }
}
