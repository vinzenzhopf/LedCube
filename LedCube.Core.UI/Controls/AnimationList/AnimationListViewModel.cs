using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.UI.Services.Library;
using LedCube.Core.UI.Services.Library.Model;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Controls.AnimationList;

public partial class AnimationListViewModel : ObservableObject, IDisposable
{
    private readonly ILogger _logger;
    private readonly ILibraryService _libraryService;
    private readonly IPluginManager _pluginManager;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaylistEntryFactory _playlistEntryFactory;

    // Master collection; the view exposed to the grid filters over this.
    private readonly ObservableCollection<AnimationListEntryViewModel> _entries = new();

    public DataGridCollectionView Animations { get; }

    [ObservableProperty]
    private AnimationListEntryViewModel? _selectedAnimation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchTextHasContent))]
    private string _searchText = string.Empty;

    public bool SearchTextHasContent => !string.IsNullOrWhiteSpace(SearchText);

    public AnimationSortField[] SortFields { get; } = Enum.GetValues<AnimationSortField>();

    [ObservableProperty]
    private AnimationSortField _selectedSortField = AnimationSortField.Name;

    [ObservableProperty]
    private bool _sortAscending = true;

    public AnimationListViewModel(ILogger<AnimationListViewModel> logger, ILibraryService libraryService,
        IPluginManager pluginManager, IPlaylistService playlistService, IPlaylistEntryFactory playlistEntryFactory)
    {
        _logger = logger;
        _libraryService = libraryService;
        _pluginManager = pluginManager;
        _playlistService = playlistService;
        _playlistEntryFactory = playlistEntryFactory;

        Animations = new DataGridCollectionView(_entries) { Filter = FilterEntry };
        ApplySort();

        LoadPluginAnimations();
        LoadLibraryAnimations(_libraryService.AnimationEntries);
        _libraryService.AnimationsChanged += OnAnimationsChanged;
    }

    private void LoadPluginAnimations()
    {
        foreach (var entry in _pluginManager.AllFrameGeneratorInfos())
            _entries.Add(new AnimationListEntryViewModel(entry));
    }

    private void LoadLibraryAnimations(IEnumerable<LibraryAnimationEntry> entries)
    {
        foreach (var entry in entries)
            UpsertFileEntry(entry);
    }

    // Discovery runs on background threads, so marshal collection mutations onto the UI thread.
    private void OnAnimationsChanged(object? sender, LibraryChangeEventArgs<LibraryAnimationEntry> e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e.Kind)
            {
                case LibraryChangeKind.Added:
                case LibraryChangeKind.Updated:
                    if (e.NewEntry is not null)
                        UpsertFileEntry(e.NewEntry);
                    break;
                case LibraryChangeKind.Removed:
                    RemoveFileEntry(e.Key);
                    break;
            }
        });
    }

    private void UpsertFileEntry(LibraryAnimationEntry entry)
    {
        var vm = new AnimationListEntryViewModel(entry);
        var existing = FindFileEntry(entry.FilePath);
        if (existing is null)
        {
            _entries.Add(vm);
            return;
        }

        _entries[_entries.IndexOf(existing)] = vm;
        if (ReferenceEquals(SelectedAnimation, existing))
            SelectedAnimation = vm;
    }

    private void RemoveFileEntry(string filePath)
    {
        var existing = FindFileEntry(filePath);
        if (existing is not null)
            _entries.Remove(existing);
    }

    private AnimationListEntryViewModel? FindFileEntry(string filePath) =>
        _entries.FirstOrDefault(e =>
            e.AnimationType == AnimationListEntryType.FileAnimation &&
            string.Equals(e.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

    private bool FilterEntry(object o)
    {
        if (o is not AnimationListEntryViewModel e)
            return false;
        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var query = SearchText.Trim();
        return Contains(e.Name, query)
               || Contains(e.Description, query)
               || Contains(e.Author, query)
               || Contains(e.FileName, query);

        static bool Contains(string? value, string query) =>
            value is not null && value.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    partial void OnSearchTextChanged(string value) => Animations.Refresh();

    [RelayCommand]
    private void ClearSearch() => SearchText = string.Empty;

    /// <summary>Appends the given entry to the playlist (double-click / drag-drop). File entries become
    /// FileAnimation entries with their path set; plugin entries use their generator.</summary>
    [RelayCommand]
    public void AddToPlaylist(AnimationListEntryViewModel? entry)
    {
        if (entry is null)
            return;

        var playlistEntry = _playlistEntryFactory.FromAnimationListEntry(entry);
        if (playlistEntry is null)
        {
            _logger.LogWarning("Could not create a playlist entry for '{Name}'.", entry.Name);
            return;
        }

        _playlistService.Add(playlistEntry);
    }

    partial void OnSelectedSortFieldChanged(AnimationSortField value) => ApplySort();
    partial void OnSortAscendingChanged(bool value) => ApplySort();

    [RelayCommand]
    private void ToggleSortDirection() => SortAscending = !SortAscending;

    private void ApplySort()
    {
        var path = SelectedSortField switch
        {
            AnimationSortField.Duration => nameof(AnimationListEntryViewModel.Duration),
            AnimationSortField.Fps => nameof(AnimationListEntryViewModel.Fps),
            AnimationSortField.Type => nameof(AnimationListEntryViewModel.AnimationType),
            _ => nameof(AnimationListEntryViewModel.Name),
        };
        var direction = SortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;

        using (Animations.DeferRefresh())
        {
            Animations.SortDescriptions.Clear();
            Animations.SortDescriptions.Add(DataGridSortDescription.FromPath(path, direction));
        }
    }

    public void Dispose()
    {
        _libraryService.AnimationsChanged -= OnAnimationsChanged;
    }
}
