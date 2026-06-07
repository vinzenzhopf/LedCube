using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LedCube.Core.UI.Services.Playlist;

namespace LedCube.Core.UI.Test.Fakes;

/// <summary>
/// In-memory IPlaylistService backed by an ObservableCollection so the
/// PlaybackControlViewModel can observe entry changes and records navigation calls.
/// </summary>
public sealed class FakePlaylistService : IPlaylistService
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ObservableCollection<PlaylistEntry> _entries = new();
    private PlaylistEntry? _selectedEntry;
    private PlaylistRepeatMode _repeatMode = PlaylistRepeatMode.LoopWholePlaylist;

    public FakePlaylistService() => Entries = new ReadOnlyObservableCollection<PlaylistEntry>(_entries);

    public ReadOnlyObservableCollection<PlaylistEntry> Entries { get; }
    public PlaylistEntry? SelectedEntry { get => _selectedEntry; private set => Set(ref _selectedEntry, value); }
    public PlaylistRepeatMode RepeatMode { get => _repeatMode; set => Set(ref _repeatMode, value); }

    public bool PlayNextCalled { get; private set; }
    public bool PlayPreviousCalled { get; private set; }

    public void Add(PlaylistEntry entry) => _entries.Add(entry);
    public void Insert(int index, PlaylistEntry entry) => _entries.Insert(index, entry);
    public void Remove(PlaylistEntry entry) => _entries.Remove(entry);
    public void Clear() => _entries.Clear();
    public void Move(int fromIndex, int toIndex) => _entries.Move(fromIndex, toIndex);
    public void Select(PlaylistEntry? entry) => SelectedEntry = entry;
    public void PlayNext() => PlayNextCalled = true;
    public void PlayPrevious() => PlayPreviousCalled = true;

    public void CycleRepeatMode()
    {
        var values = (PlaylistRepeatMode[])System.Enum.GetValues(typeof(PlaylistRepeatMode));
        RepeatMode = values[(System.Array.IndexOf(values, RepeatMode) + 1) % values.Length];
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
