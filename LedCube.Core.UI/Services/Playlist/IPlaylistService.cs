using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LedCube.Core.UI.Services.Playlist;

public interface IPlaylistService : INotifyPropertyChanged
{
    ReadOnlyObservableCollection<PlaylistEntry> Entries { get; }
    PlaylistEntry? SelectedEntry { get; }

    /// <summary>How the playlist behaves when an entry finishes (auto-advance policy).</summary>
    PlaylistRepeatMode RepeatMode { get; set; }

    /// <summary>Advances <see cref="RepeatMode"/> to the next value (wraps), for a cycle-through UI button.</summary>
    void CycleRepeatMode();
    void Add(PlaylistEntry entry);
    void Insert(int index, PlaylistEntry entry);
    void Remove(PlaylistEntry entry);

    /// <summary>Removes all entries (raises a single Reset). Used when loading a playlist file.</summary>
    void Clear();
    void Move(int fromIndex, int toIndex);
    void Select(PlaylistEntry? entry);

    /// <summary>Loads and plays the entry after the currently playing one (wraps around). Does not change selection.</summary>
    void PlayNext();

    /// <summary>Loads and plays the entry before the currently playing one (wraps around). Does not change selection.</summary>
    void PlayPrevious();
}
