using System.Collections.ObjectModel;

namespace LedCube.Core.UI.Services.Playlist;

public interface IPlaylistService
{
    ReadOnlyObservableCollection<PlaylistEntry> Entries { get; }
    PlaylistEntry? SelectedEntry { get; }
    void Add(PlaylistEntry entry);
    void Insert(int index, PlaylistEntry entry);
    void Remove(PlaylistEntry entry);
    void Move(int fromIndex, int toIndex);
    void Select(PlaylistEntry? entry);
    void SelectNext();
    void SelectPrevious();
}
