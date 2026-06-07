using System.Collections.Generic;
using System.IO;
using System.Text;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Playlist.Io;
using LedCube.Animation.FileFormat.Playlist.Model;
using LedCube.Animation.FileFormat.Test.Fixtures;

namespace LedCube.Animation.FileFormat.Test.Playlist;

public class ValidationTests
{
    private static PlaylistModel Read(string manifestJson)
    {
        using var ms = RawZip.FromEntries(RawZip.Text("manifest.json", manifestJson));
        return LcPlstReader.Read(ms);
    }

    [Fact]
    public void Validation_MissingName_Throws()
    {
        var json = """{ "formatVersion": 1, "entries": [] }""";
        Assert.Throws<InvalidFileFormatException>(() => Read(json));
    }

    [Fact]
    public void Validation_EmptyName_Throws()
    {
        var json = """{ "formatVersion": 1, "name": "", "entries": [] }""";
        Assert.Throws<InvalidFileFormatException>(() => Read(json));
    }

    [Fact]
    public void Validation_EntryMissingTypeName_Throws()
    {
        var json = """
        { "formatVersion": 1, "name": "X", "entries": [ { "instanceName": "no type" } ] }
        """;
        Assert.Throws<InvalidFileFormatException>(() => Read(json));
    }

    [Fact]
    public void Validation_NegativeRepeatCount_Throws()
    {
        var json = """
        { "formatVersion": 1, "name": "X", "entries": [ { "typeName": "T", "repeatCount": -1 } ] }
        """;
        Assert.Throws<InvalidFileFormatException>(() => Read(json));
    }

    [Fact]
    public void Validation_ZeroFrameTimeOverride_Throws()
    {
        var json = """
        { "formatVersion": 1, "name": "X", "entries": [ { "typeName": "T", "frameTimeUsOverride": 0 } ] }
        """;
        Assert.Throws<InvalidFileFormatException>(() => Read(json));
    }

    [Fact]
    public void Validation_FutureFormatVersion_Throws()
    {
        var json = """{ "formatVersion": 999, "name": "X", "entries": [] }""";
        Assert.Throws<UnsupportedFormatVersionException>(() => Read(json));
    }

    [Fact]
    public void Validation_MissingManifestEntry_Throws()
    {
        using var ms = RawZip.FromEntries(RawZip.Text("not-manifest.json", "{}"));
        Assert.Throws<MissingEntryException>(() => LcPlstReader.Read(ms));
    }

    [Fact]
    public void Validation_MissingFormatVersion_Throws()
    {
        var json = """{ "name": "X", "entries": [] }""";
        Assert.Throws<InvalidFileFormatException>(() => Read(json));
    }
}
