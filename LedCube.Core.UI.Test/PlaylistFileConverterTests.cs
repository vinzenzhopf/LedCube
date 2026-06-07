using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LedCube.Core.UI.Services.Library;
using LedCube.Core.UI.Services.Library.Model;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginBase;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using FilePlaylist = LedCube.Animation.FileFormat.Playlist.Model.Playlist;
using FilePlaylistEntry = LedCube.Animation.FileFormat.Playlist.Model.PlaylistEntry;
using FilePlaylistManifest = LedCube.Animation.FileFormat.Playlist.Model.PlaylistManifest;
using FileRepeatMode = LedCube.Animation.FileFormat.Playlist.Model.PlaylistRepeatMode;

namespace LedCube.Core.UI.Test;

public class PlaylistFileConverterTests
{
    // Marker types whose full names identify the generators under test.
    private sealed class MultiGen { }
    private sealed class FileGen { }

    private static readonly FrameGeneratorInfo MultiInfo = new("Multi", "multi", new[]
    {
        new AnimationConfigDescriptor("Count", "Count", AnimationConfigType.Int, 1),
        new AnimationConfigDescriptor("Speed", "Speed", AnimationConfigType.Float, 1.0f),
        new AnimationConfigDescriptor("On", "On", AnimationConfigType.Bool, false),
        new AnimationConfigDescriptor("Mode", "Mode", AnimationConfigType.Enum, "A", EnumValues: new[] { "A", "B" }),
    });

    private static readonly FrameGeneratorInfo FileInfo = new("File", "file", new[]
    {
        new AnimationConfigDescriptor("FilePath", "File", AnimationConfigType.FilePath),
    });

    private static PlaylistFileConverter Build(string animationsPath = "")
    {
        var pluginManager = new StubPluginManager(new[]
        {
            new FrameGeneratorEntry(MultiInfo, typeof(MultiGen).GetTypeInfo()),
            new FrameGeneratorEntry(FileInfo, typeof(FileGen).GetTypeInfo()),
        });
        var library = new StubLibraryService { AnimationsPath = animationsPath };
        return new PlaylistFileConverter(NullLogger<PlaylistFileConverter>.Instance, pluginManager, library);
    }

    private static PlaylistEntry MultiEntry() => new(MultiInfo, typeof(MultiGen).GetTypeInfo());
    private static PlaylistEntry FileEntry() => new(FileInfo, typeof(FileGen).GetTypeInfo());

    [Fact]
    public void ToModel_MapsTypeNameInstanceRepeatAndFrameTime()
    {
        var converter = Build();
        var entry = MultiEntry();
        entry.InstanceName = "My Multi";
        entry.RepeatCount = 3;
        entry.FrameTimeOverride = TimeSpan.FromMicroseconds(25000);

        var model = converter.ToModel(new[] { entry }, PlaylistRepeatMode.FairRandomPlay, new PlaylistMetadata("Show"), null);

        Assert.Equal(typeof(MultiGen).GetTypeInfo().FullName, model.Entries[0].TypeName);
        Assert.Equal("My Multi", model.Entries[0].InstanceName);
        Assert.Equal(3, model.Entries[0].RepeatCount);
        Assert.Equal(25000u, model.Entries[0].FrameTimeUsOverride);
        Assert.Equal(FileRepeatMode.FairRandomPlay, model.Manifest.RepeatMode);
        Assert.Equal("Show", model.Manifest.Name);
    }

    [Fact]
    public void ToModel_NormalizesConfigToModelPrimitives()
    {
        var converter = Build();
        var entry = MultiEntry();
        entry.Config["Count"] = 42;
        entry.Config["Speed"] = 1.5f;
        entry.Config["On"] = true;
        entry.Config["Mode"] = "B";

        var config = converter.ToModel(new[] { entry }, PlaylistRepeatMode.LoopWholePlaylist, new PlaylistMetadata("S"), null)
            .Entries[0].Config;

        Assert.Equal(42L, config["Count"]);
        Assert.Equal(1.5, config["Speed"]);
        Assert.Equal(true, config["On"]);
        Assert.Equal("B", config["Mode"]);
    }

    [Fact]
    public void ToUiPlaylist_CoercesConfigToDescriptorTypes()
    {
        var converter = Build();
        var source = new FilePlaylistEntry(
            typeName: typeof(MultiGen).GetTypeInfo().FullName!,
            config: new Dictionary<string, object?>
            {
                ["Count"] = 42L,
                ["Speed"] = 1.5,
                ["On"] = true,
                ["Mode"] = "B",
            });
        var model = new FilePlaylist(new FilePlaylistManifest { Name = "S" }, new[] { source });

        var config = converter.ToUiPlaylist(model, null).Entries[0].Config;

        Assert.IsType<int>(config["Count"]);
        Assert.Equal(42, config["Count"]);
        Assert.IsType<float>(config["Speed"]);
        Assert.Equal(1.5f, config["Speed"]);
        Assert.Equal(true, config["On"]);
        Assert.Equal("B", config["Mode"]);
    }

    [Fact]
    public void ToUiPlaylist_UnknownType_IsReportedAndSkipped()
    {
        var converter = Build();
        var source = new FilePlaylistEntry(typeName: "Some.Unknown.Generator");
        var model = new FilePlaylist(new FilePlaylistManifest { Name = "S" }, new[] { source });

        var result = converter.ToUiPlaylist(model, null);

        Assert.Empty(result.Entries);
        Assert.Equal(new[] { "Some.Unknown.Generator" }, result.UnresolvedTypeNames);
    }

    [Fact]
    public void RepeatModeAndMetadata_RoundTrip()
    {
        var converter = Build();
        var metadata = new PlaylistMetadata("Evening", "vinzenz", "desc",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        var model = converter.ToModel(new[] { MultiEntry() }, PlaylistRepeatMode.TrueRandomPlay, metadata, null);
        var result = converter.ToUiPlaylist(model, null);

        Assert.Equal(PlaylistRepeatMode.TrueRandomPlay, result.RepeatMode);
        Assert.Equal(metadata, result.Metadata);
    }

    [Fact]
    public void FilePath_InsideLibrary_StoredRelative_ThenResolvedBack()
    {
        var animationsDir = Path.Combine(Path.GetTempPath(), "lc-conv-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(animationsDir);
        try
        {
            var filePath = Path.Combine(animationsDir, "sunrise.lcanimraw");
            File.WriteAllBytes(filePath, new byte[] { 1 });

            var converter = Build(animationsDir);
            var entry = FileEntry();
            entry.Config["FilePath"] = filePath;

            var model = converter.ToModel(new[] { entry }, PlaylistRepeatMode.StopAtEnd, new PlaylistMetadata("S"), null);
            // Stored as a bare, forward-slash, library-relative filename.
            Assert.Equal("sunrise.lcanimraw", model.Entries[0].Config["FilePath"]);

            var resolved = converter.ToUiPlaylist(model, null).Entries[0].Config["FilePath"];
            Assert.Equal(filePath, resolved);
        }
        finally
        {
            Directory.Delete(animationsDir, recursive: true);
        }
    }

    [Fact]
    public void FilePath_OutsideLibrary_StoredAbsolute()
    {
        var converter = Build(Path.Combine(Path.GetTempPath(), "lc-lib-" + Guid.NewGuid().ToString("N")));
        var outside = Path.Combine(Path.GetTempPath(), "elsewhere", "x.lcanimraw");
        var entry = FileEntry();
        entry.Config["FilePath"] = outside;

        var model = converter.ToModel(new[] { entry }, PlaylistRepeatMode.StopAtEnd, new PlaylistMetadata("S"), null);

        Assert.Equal(outside.Replace('\\', '/'), model.Entries[0].Config["FilePath"]);
    }

    private sealed class StubPluginManager(IReadOnlyList<FrameGeneratorEntry> entries) : IPluginManager
    {
        public IEnumerable<FrameGeneratorEntry> AllFrameGeneratorInfos() => entries;
        public IFrameGenerator GetFrameGenerator(FrameGeneratorEntry entry) => throw new NotSupportedException();
    }

    private sealed class StubLibraryService : ILibraryService
    {
        public string LibraryPath { get; set; } = string.Empty;
        public string AnimationsPath { get; set; } = string.Empty;
        public string PlaylistsPath { get; set; } = string.Empty;
        public string ProjectsPath { get; set; } = string.Empty;
        public bool WatchDirectory { get; set; }

        public IReadOnlyCollection<LibraryAnimationEntry> AnimationEntries => Array.Empty<LibraryAnimationEntry>();
        public IReadOnlyCollection<LibraryPlaylistEntry> PlaylistEntries => Array.Empty<LibraryPlaylistEntry>();
        public IReadOnlyCollection<LibraryProjectEntry> ProjectEntries => Array.Empty<LibraryProjectEntry>();

        public event EventHandler<LibraryChangeEventArgs<LibraryAnimationEntry>>? AnimationsChanged;
        public event EventHandler<LibraryChangeEventArgs<LibraryPlaylistEntry>>? PlaylistsChanged;
        public event EventHandler<LibraryChangeEventArgs<LibraryProjectEntry>>? ProjectsChanged;

        // Suppress unused-event warnings; the converter never raises these.
        public void Unused() => _ = (AnimationsChanged, PlaylistsChanged, ProjectsChanged);
    }
}
