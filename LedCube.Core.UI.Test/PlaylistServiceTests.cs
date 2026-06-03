using System.Collections.Generic;
using System.Reflection;
using Avalonia.Headless.XUnit;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;
using LedCube.Core.UI.Test.Fakes;
using LedCube.PluginBase;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LedCube.Core.UI.Test;

public class PlaylistServiceTests
{
    private static PlaylistEntry MakeEntry(string name)
        => new(new FrameGeneratorInfo(name, name), typeof(PlaybackControlViewModel).GetTypeInfo());

    private static (PlaylistService playlist, FakePlaybackService playback) Build(params string[] names)
    {
        var playback = new FakePlaybackService();
        var playlist = new PlaylistService(NullLogger<PlaylistService>.Instance, new FakePluginManager(), playback);
        foreach (var name in names)
            playlist.Add(MakeEntry(name));
        return (playlist, playback);
    }

    private static void Finish(PlaylistService playlist) => playlist.Receive(new PlaybackFinishedMessage());

    [AvaloniaFact]
    public void PlaybackFinishedMessage_FromMessenger_TriggersAutoAdvance()
    {
        // Proves the WeakReferenceMessenger registration actually wires PlaybackService's
        // PlaybackFinishedMessage to PlaylistService.Receive (the real app's auto-advance path).
        var (playlist, playback) = Build("A", "B");
        playlist.RepeatMode = PlaylistRepeatMode.LoopWholePlaylist;
        playback.CurrentEntry = playlist.Entries[0];

        WeakReferenceMessenger.Default.Send(new PlaybackFinishedMessage());

        Assert.Same(playlist.Entries[1], playback.CurrentEntry);
    }

    [AvaloniaFact]
    public void StopAtEnd_StopsPlayback_AtLastEntry()
    {
        var (playlist, playback) = Build("A", "B");
        playlist.RepeatMode = PlaylistRepeatMode.StopAtEnd;
        playback.CurrentEntry = playlist.Entries[1]; // last

        Finish(playlist);

        Assert.True(playback.StopPlaybackCalled);
    }

    [AvaloniaFact]
    public void StopAtEnd_AdvancesToNext_WhenNotLast()
    {
        var (playlist, playback) = Build("A", "B");
        playlist.RepeatMode = PlaylistRepeatMode.StopAtEnd;
        playback.CurrentEntry = playlist.Entries[0];

        Finish(playlist);

        Assert.False(playback.StopPlaybackCalled);
        Assert.Same(playlist.Entries[1], playback.CurrentEntry);
        Assert.True(playback.StartPlaybackCalled);
    }

    [AvaloniaFact]
    public void LoopWholePlaylist_WrapsToFirst_FromLast()
    {
        var (playlist, playback) = Build("A", "B");
        playlist.RepeatMode = PlaylistRepeatMode.LoopWholePlaylist;
        playback.CurrentEntry = playlist.Entries[1]; // last

        Finish(playlist);

        Assert.False(playback.StopPlaybackCalled);
        Assert.Same(playlist.Entries[0], playback.CurrentEntry);
    }

    [AvaloniaFact]
    public void RepeatCurrentEntry_ReloadsSameEntry()
    {
        var (playlist, playback) = Build("A", "B");
        playlist.RepeatMode = PlaylistRepeatMode.RepeatCurrentEntry;
        var current = playlist.Entries[0];
        playback.CurrentEntry = current;

        Finish(playlist);

        Assert.Same(current, playback.CurrentEntry);
        Assert.True(playback.StartPlaybackCalled);
    }

    [AvaloniaFact]
    public void AutoAdvance_DoesNotChangeSelection()
    {
        var (playlist, playback) = Build("A", "B");
        playlist.RepeatMode = PlaylistRepeatMode.LoopWholePlaylist;
        playback.CurrentEntry = playlist.Entries[0];

        Finish(playlist);

        Assert.Null(playlist.SelectedEntry);
    }

    [AvaloniaFact]
    public void FairRandomPlay_AlternatesWithoutImmediateRepeat()
    {
        // With two entries, the fair shuffle is deterministic: it must alternate.
        var (playlist, playback) = Build("A", "B");
        playlist.RepeatMode = PlaylistRepeatMode.FairRandomPlay;
        playback.CurrentEntry = playlist.Entries[0];

        var sequence = new List<PlaylistEntry>();
        for (var i = 0; i < 4; i++)
        {
            Finish(playlist);
            sequence.Add(playback.CurrentEntry!);
        }

        Assert.Equal(
            new[] { playlist.Entries[1], playlist.Entries[0], playlist.Entries[1], playlist.Entries[0] },
            sequence);
    }

    [AvaloniaFact]
    public void DefaultRepeatMode_IsLoopWholePlaylist()
    {
        var (playlist, _) = Build("A");
        Assert.Equal(PlaylistRepeatMode.LoopWholePlaylist, playlist.RepeatMode);
    }

    [AvaloniaFact]
    public void CycleRepeatMode_CyclesThroughAllValuesAndWraps()
    {
        var (playlist, _) = Build("A");
        playlist.RepeatMode = PlaylistRepeatMode.StopAtEnd;

        playlist.CycleRepeatMode();
        Assert.Equal(PlaylistRepeatMode.LoopWholePlaylist, playlist.RepeatMode);
        playlist.CycleRepeatMode();
        Assert.Equal(PlaylistRepeatMode.RepeatCurrentEntry, playlist.RepeatMode);
        playlist.CycleRepeatMode();
        Assert.Equal(PlaylistRepeatMode.FairRandomPlay, playlist.RepeatMode);
        playlist.CycleRepeatMode();
        Assert.Equal(PlaylistRepeatMode.TrueRandomPlay, playlist.RepeatMode);
        playlist.CycleRepeatMode();
        Assert.Equal(PlaylistRepeatMode.StopAtEnd, playlist.RepeatMode);
    }
}
