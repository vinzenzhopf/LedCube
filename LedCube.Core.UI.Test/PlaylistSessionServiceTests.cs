using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using LedCube.Core.UI.Services.Library;
using LedCube.Core.UI.Services.Library.Model;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;
using LedCube.Core.UI.Test.Fakes;
using LedCube.PluginBase;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LedCube.Core.UI.Test;

public class PlaylistSessionServiceTests : IDisposable
{
    private sealed class Gen { }

    private static readonly FrameGeneratorInfo Info = new("Marker", "marker");

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "lc-session-" + Guid.NewGuid().ToString("N"));

    private (PlaylistSessionService session, PlaylistService playlist) Build()
    {
        Directory.CreateDirectory(_dir);
        var pluginManager = new StubPluginManager(new[] { new FrameGeneratorEntry(Info, typeof(Gen).GetTypeInfo()) });
        var playlist = new PlaylistService(NullLogger<PlaylistService>.Instance, pluginManager, new FakePlaybackService());
        var converter = new PlaylistFileConverter(NullLogger<PlaylistFileConverter>.Instance, pluginManager, new StubLibraryService());
        var session = new PlaylistSessionService(NullLogger<PlaylistSessionService>.Instance, playlist, converter);
        return (session, playlist);
    }

    private static PlaylistEntry Entry(string name)
        => new(Info, typeof(Gen).GetTypeInfo()) { InstanceName = name };

    [Fact]
    public async Task SaveTo_ThenLoad_RoundTripsEntriesRepeatAndMetadata()
    {
        var (session, playlist) = Build();
        playlist.Add(Entry("A"));
        playlist.Add(Entry("B"));
        playlist.RepeatMode = PlaylistRepeatMode.FairRandomPlay;
        session.UpdateMetadata(new PlaylistMetadata("My Show", "me", "desc"));

        var path = Path.Combine(_dir, "show.lcplst");
        await session.SaveToAsync(path);

        // Mutate, then load the file back.
        playlist.Add(Entry("C"));
        await session.LoadAsync(path);

        Assert.Equal(2, playlist.Entries.Count);
        Assert.Equal("A", playlist.Entries[0].InstanceName);
        Assert.Equal(PlaylistRepeatMode.FairRandomPlay, playlist.RepeatMode);
        Assert.Equal("My Show", session.Metadata.Name);
        Assert.Equal("me", session.Metadata.Author);
        Assert.Equal(path, session.SourceFilePath);
        Assert.False(session.IsDirty);
    }

    [Fact]
    public async Task AddingEntry_MarksDirty_SaveClearsIt()
    {
        var (session, playlist) = Build();
        Assert.False(session.IsDirty);

        playlist.Add(Entry("A"));
        Assert.True(session.IsDirty);

        await session.SaveToAsync(Path.Combine(_dir, "p.lcplst"));
        Assert.False(session.IsDirty);
    }

    [Fact]
    public void ChangingRepeatMode_MarksDirty()
    {
        var (session, playlist) = Build();
        playlist.RepeatMode = PlaylistRepeatMode.TrueRandomPlay;
        Assert.True(session.IsDirty);
    }

    [Fact]
    public void UpdateMetadata_MarksDirty()
    {
        var (session, _) = Build();
        session.UpdateMetadata(new PlaylistMetadata("X"));
        Assert.True(session.IsDirty);
    }

    [Fact]
    public async Task Load_DoesNotMarkDirty()
    {
        var (session, playlist) = Build();
        playlist.Add(Entry("A"));
        var path = Path.Combine(_dir, "p.lcplst");
        await session.SaveToAsync(path);

        await session.LoadAsync(path);

        Assert.False(session.IsDirty);
    }

    [Fact]
    public async Task SaveAsync_WithoutSource_Throws()
    {
        var (session, _) = Build();
        await Assert.ThrowsAsync<InvalidOperationException>(() => session.SaveAsync());
    }

    [Fact]
    public async Task DetachFromFile_ClearsSourceAndMarksDirty()
    {
        var (session, playlist) = Build();
        playlist.Add(Entry("A"));
        var path = Path.Combine(_dir, "p.lcplst");
        await session.SaveToAsync(path);
        Assert.Equal(path, session.SourceFilePath);

        session.DetachFromFile();

        Assert.Null(session.SourceFilePath);
        Assert.True(session.IsDirty);
    }

    [Fact]
    public async Task SaveTo_BlankName_DefaultsToFileName()
    {
        var (session, playlist) = Build();
        playlist.Add(Entry("A"));

        var path = Path.Combine(_dir, "evening.lcplst");
        await session.SaveToAsync(path);

        Assert.Equal("evening", session.Metadata.Name);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }

    private sealed class StubPluginManager(IReadOnlyList<FrameGeneratorEntry> entries) : IPluginManager
    {
        public IEnumerable<FrameGeneratorEntry> AllFrameGeneratorInfos() => entries;
        public IFrameGenerator GetFrameGenerator(FrameGeneratorEntry entry) => throw new NotSupportedException();
    }

    private sealed class StubLibraryService : ILibraryService
    {
        public string LibraryPath => string.Empty;
        public string AnimationsPath => string.Empty;
        public string PlaylistsPath => string.Empty;
        public string ProjectsPath => string.Empty;
        public bool WatchDirectory => false;

        public IReadOnlyCollection<LibraryAnimationEntry> AnimationEntries => Array.Empty<LibraryAnimationEntry>();
        public IReadOnlyCollection<LibraryPlaylistEntry> PlaylistEntries => Array.Empty<LibraryPlaylistEntry>();
        public IReadOnlyCollection<LibraryProjectEntry> ProjectEntries => Array.Empty<LibraryProjectEntry>();

        public event EventHandler<LibraryChangeEventArgs<LibraryAnimationEntry>>? AnimationsChanged;
        public event EventHandler<LibraryChangeEventArgs<LibraryPlaylistEntry>>? PlaylistsChanged;
        public event EventHandler<LibraryChangeEventArgs<LibraryProjectEntry>>? ProjectsChanged;

        public void Unused() => _ = (AnimationsChanged, PlaylistsChanged, ProjectsChanged);
    }
}
