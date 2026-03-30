using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Services.Playback;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Services.Playlist;

public partial class PlaylistService : ObservableObject, IPlaylistService, IRecipient<PlaybackFinishedMessage>
{
    private readonly ILogger<PlaylistService> _logger;
    private readonly IPluginManager _pluginManager;
    private readonly IPlaybackService _playbackService;

    private readonly ObservableCollection<PlaylistEntry> _entries = [];
    public ReadOnlyObservableCollection<PlaylistEntry> Entries { get; }

    [ObservableProperty]
    private PlaylistEntry? _selectedEntry;

    public PlaylistService(ILogger<PlaylistService> logger, IPluginManager pluginManager, IPlaybackService playbackService)
    {
        _logger = logger;
        _pluginManager = pluginManager;
        _playbackService = playbackService;
        Entries = new ReadOnlyObservableCollection<PlaylistEntry>(_entries);
        _entries.CollectionChanged += OnEntriesChanged;
        WeakReferenceMessenger.Default.Register(this);
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

    public void SelectNext()
    {
        if (_entries.Count == 0) return;
        var idx = SelectedEntry is null ? -1 : _entries.IndexOf(SelectedEntry);
        SelectedEntry = _entries[(idx + 1) % _entries.Count];
    }

    public void SelectPrevious()
    {
        if (_entries.Count == 0) return;
        var idx = SelectedEntry is null ? 0 : _entries.IndexOf(SelectedEntry);
        SelectedEntry = _entries[(idx - 1 + _entries.Count) % _entries.Count];
    }

    public void Receive(PlaybackFinishedMessage message)
    {
        if (_entries.Count == 0) return;
        _logger.LogInformation("Playback finished, advancing to next entry.");

        // Advance relative to the currently loaded entry, not the UI selection.
        var currentEntry = _playbackService.CurrentEntry;
        var idx = currentEntry is null ? -1 : _entries.IndexOf(currentEntry);
        var next = _entries[(idx + 1) % _entries.Count];

        // Update selection so config display follows.
        SelectedEntry = next;

        _ = LoadAndPlayAsync(next);
    }

    private async Task LoadAndPlayAsync(PlaylistEntry entry)
    {
        await _playbackService.UpdateFrameGeneratorAsync(entry, CancellationToken.None).ConfigureAwait(false);
        _playbackService.StartPlayback();
    }
}
