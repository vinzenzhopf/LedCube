using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Services.Playback;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Services.Playlist;

public partial class PlaylistService : ObservableObject, IPlaylistService,
    IRecipient<PlaybackFinishedMessage>,
    IRecipient<PlaylistEntryConfigChangedMessage>
{
    private readonly ILogger<PlaylistService> _logger;
    private readonly IPluginManager _pluginManager;
    private readonly IPlaybackService _playbackService;

    private readonly ObservableCollection<PlaylistEntry> _entries = [];
    public ReadOnlyObservableCollection<PlaylistEntry> Entries { get; }

    [ObservableProperty]
    private PlaylistEntry? _selectedEntry;

    [ObservableProperty]
    private PlaylistRepeatMode _repeatMode = PlaylistRepeatMode.LoopWholePlaylist;

    // Entries already played in the current FairRandomPlay cycle.
    private readonly HashSet<PlaylistEntry> _shuffleHistory = [];

    public PlaylistService(ILogger<PlaylistService> logger, IPluginManager pluginManager, IPlaybackService playbackService)
    {
        _logger = logger;
        _pluginManager = pluginManager;
        _playbackService = playbackService;
        Entries = new ReadOnlyObservableCollection<PlaylistEntry>(_entries);
        _entries.CollectionChanged += OnEntriesChanged;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    partial void OnSelectedEntryChanged(PlaylistEntry? value)
    {
        // Selection only updates config display — does NOT affect playback.
        WeakReferenceMessenger.Default.Send(new PlaylistSelectionChangedMessage(value));
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Remove || e.OldItems is null)
            return;
        foreach (PlaylistEntry removed in e.OldItems)
        {
            if (SelectedEntry == removed)
                SelectedEntry = null;
        }
    }

    public void Add(PlaylistEntry entry) => _entries.Add(entry);

    public void Insert(int index, PlaylistEntry entry) => _entries.Insert(index, entry);

    public void Remove(PlaylistEntry entry) => _entries.Remove(entry);

    public void Move(int fromIndex, int toIndex) => _entries.Move(fromIndex, toIndex);

    public void Select(PlaylistEntry? entry) => SelectedEntry = entry;

    public void CycleRepeatMode()
    {
        var values = (PlaylistRepeatMode[])Enum.GetValues(typeof(PlaylistRepeatMode));
        RepeatMode = values[(Array.IndexOf(values, RepeatMode) + 1) % values.Length];
    }

    // Manual playback navigation is anchored to the currently *playing* entry and never touches
    // the selection — selection only drives config display/editing. Manual skipping is always
    // sequential (wraps) regardless of RepeatMode; the repeat mode only governs auto-advance.
    public void PlayNext() => PlayRelative(+1);

    public void PlayPrevious() => PlayRelative(-1);

    private void PlayRelative(int direction)
    {
        if (_entries.Count == 0) return;
        var anchor = _playbackService.CurrentEntry ?? SelectedEntry;
        var idx = anchor is null ? -1 : _entries.IndexOf(anchor);
        var nextIdx = ((idx + direction) % _entries.Count + _entries.Count) % _entries.Count;
        LoadAndPlay(_entries[nextIdx]);
    }

    public void Receive(PlaybackFinishedMessage message)
    {
        if (_entries.Count == 0) return;

        var next = PickAutoAdvanceEntry();
        if (next is null)
        {
            _logger.LogInformation("Playlist reached the end ({Mode}); stopping.", RepeatMode);
            _playbackService.StopPlayback();
            return;
        }

        _logger.LogInformation("Playback finished, auto-advancing ({Mode}) to '{Next}'.",
            RepeatMode, next.InstanceName);
        LoadAndPlay(next);
    }

    /// <summary>Chooses the next entry for auto-advance per <see cref="RepeatMode"/>, or null to stop.</summary>
    private PlaylistEntry? PickAutoAdvanceEntry()
    {
        var current = _playbackService.CurrentEntry;
        var idx = current is null ? -1 : _entries.IndexOf(current);

        switch (RepeatMode)
        {
            case PlaylistRepeatMode.RepeatCurrentEntry:
                return current ?? _entries[0];

            case PlaylistRepeatMode.StopAtEnd:
                return idx >= 0 && idx < _entries.Count - 1 ? _entries[idx + 1] : null;

            case PlaylistRepeatMode.TrueRandomPlay:
                return _entries[Random.Shared.Next(_entries.Count)];

            case PlaylistRepeatMode.FairRandomPlay:
                return PickFairShuffleEntry(current);

            case PlaylistRepeatMode.LoopWholePlaylist:
            default:
                return _entries[(idx + 1) % _entries.Count];
        }
    }

    private PlaylistEntry PickFairShuffleEntry(PlaylistEntry? current)
    {
        if (current is not null)
            _shuffleHistory.Add(current);

        var remaining = _entries.Where(e => !_shuffleHistory.Contains(e)).ToList();
        if (remaining.Count == 0)
        {
            // Whole playlist played — start a new cycle, avoiding an immediate repeat of current.
            _shuffleHistory.Clear();
            remaining = _entries.Where(e => !ReferenceEquals(e, current)).ToList();
            if (remaining.Count == 0)
                remaining = [.. _entries];
        }

        var pick = remaining[Random.Shared.Next(remaining.Count)];
        _shuffleHistory.Add(pick);
        return pick;
    }

    public void Receive(PlaylistEntryConfigChangedMessage message)
    {
        // Reconfigure the loaded generator so config edits (e.g. a new file path) take effect.
        // Only when this entry is the loaded one and playback is stopped, so a running
        // animation is never interrupted out from under the user.
        if (!ReferenceEquals(message.Entry, _playbackService.CurrentEntry)) return;
        if (_playbackService.PlaybackState != PlaybackState.Stopped) return;

        _ = _playbackService.UpdateFrameGeneratorAsync(message.Entry, CancellationToken.None);
    }

    // Fire-and-forget load+play that logs faults instead of swallowing them (the previous
    // `_ = LoadAndPlayAsync(...)` discarded exceptions, hiding auto-advance failures).
    // Auto-advance is raised from the playback background thread, but loading an entry mutates
    // observable state bound to the view, so marshal onto the UI thread (where manual play runs).
    private void LoadAndPlay(PlaylistEntry entry)
    {
        if (Dispatcher.UIThread.CheckAccess())
            StartLoadAndPlay(entry);
        else
            Dispatcher.UIThread.Post(() => StartLoadAndPlay(entry));
    }

    private void StartLoadAndPlay(PlaylistEntry entry)
    {
        _ = LoadAndPlayAsync(entry).ContinueWith(
            t => _logger.LogError(t.Exception, "Failed to load and play '{Entry}'.", entry.InstanceName),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
    }

    // No ConfigureAwait(false): callers invoke this on the UI thread, and StartPlayback must resume
    // there too (it mutates observable state bound to the view). The file I/O inside the generator's
    // InitializeAsync runs on a thread pool thread, so the UI thread is not blocked while loading.
    private async Task LoadAndPlayAsync(PlaylistEntry entry)
    {
        await _playbackService.UpdateFrameGeneratorAsync(entry, CancellationToken.None);
        _playbackService.StartPlayback();
    }
}
