using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.SimpleDialog;
using LedCube.Core.UI.Services.Library;
using LedCube.Core.UI.Services.Library.Model;
using LedCube.Core.UI.Services.Playlist;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Controls.PlaylistSelection;

/// <summary>
/// Drives the playlist-selection bar above the playlist: a dropdown of discovered playlists (plus an
/// unsaved option), editable metadata, and Save/Reload — all backed by <see cref="IPlaylistSessionService"/>.
/// </summary>
public partial class PlaylistSelectionViewModel : ObservableObject, IDisposable
{
    private readonly IPlaylistSessionService _session;
    private readonly ILibraryService _library;
    private readonly ILogger<PlaylistSelectionViewModel> _logger;

    private readonly PlaylistOptionViewModel _unsavedOption = new(null, "<Unsaved playlist>");

    // Guards reentrancy: when the code (not the user) changes the selection or the metadata fields.
    private bool _suppressSelectionGuard;
    private bool _suppressMetadataPush;

    public ObservableCollection<PlaylistOptionViewModel> Playlists { get; } = [];

    [ObservableProperty]
    private PlaylistOptionViewModel? _selectedPlaylist;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _author = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _createdText = string.Empty;

    [ObservableProperty]
    private string _sourcePathText = string.Empty;

    [ObservableProperty]
    private bool _isDirty;

    public PlaylistSelectionViewModel(
        IPlaylistSessionService session,
        ILibraryService library,
        ILogger<PlaylistSelectionViewModel> logger)
    {
        _session = session;
        _library = library;
        _logger = logger;

        _session.PropertyChanged += OnSessionPropertyChanged;
        _library.PlaylistsChanged += OnLibraryPlaylistsChanged;

        RebuildOptions();
        SyncFromSession();
    }

    // ---- session → view ----

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IPlaylistSessionService.Metadata):
                SyncMetadataFields();
                break;
            case nameof(IPlaylistSessionService.SourceFilePath):
                SourcePathText = _session.SourceFilePath ?? string.Empty;
                SyncSelectedOption();
                ReloadCommand.NotifyCanExecuteChanged();
                break;
            case nameof(IPlaylistSessionService.IsDirty):
                IsDirty = _session.IsDirty;
                break;
        }
    }

    private void SyncFromSession()
    {
        SyncMetadataFields();
        SourcePathText = _session.SourceFilePath ?? string.Empty;
        IsDirty = _session.IsDirty;
        SyncSelectedOption();
    }

    private void SyncMetadataFields()
    {
        _suppressMetadataPush = true;
        try
        {
            Name = _session.Metadata.Name;
            Author = _session.Metadata.Author ?? string.Empty;
            Description = _session.Metadata.Description ?? string.Empty;
            CreatedText = _session.Metadata.CreatedUtc?.LocalDateTime.ToString("g") ?? string.Empty;
        }
        finally
        {
            _suppressMetadataPush = false;
        }
    }

    private void SyncSelectedOption()
    {
        var match = _session.SourceFilePath is { } path
            ? Playlists.FirstOrDefault(o => string.Equals(o.FilePath, path, StringComparison.OrdinalIgnoreCase))
            : _unsavedOption;

        WithSuppressedSelection(() => SelectedPlaylist = match ?? _unsavedOption);
    }

    // ---- library → options ----

    private void OnLibraryPlaylistsChanged(object? sender, LibraryChangeEventArgs<LibraryPlaylistEntry> e)
        => Dispatcher.UIThread.Post(RebuildOptions);

    private void RebuildOptions()
    {
        // Suppress for the whole rebuild: clearing the collection makes the ComboBox null its
        // SelectedItem, which would otherwise be handled as a user selection (and detach the file).
        WithSuppressedSelection(() =>
        {
            Playlists.Clear();
            Playlists.Add(_unsavedOption);
            foreach (var entry in _library.PlaylistEntries.OrderBy(p => p.DisplayName, StringComparer.OrdinalIgnoreCase))
                Playlists.Add(new PlaylistOptionViewModel(entry.FilePath, entry.DisplayName));

            EnsureOptionFor(_session.SourceFilePath);

            var match = _session.SourceFilePath is { } path
                ? Playlists.FirstOrDefault(o => string.Equals(o.FilePath, path, StringComparison.OrdinalIgnoreCase))
                : _unsavedOption;
            SelectedPlaylist = match ?? _unsavedOption;
        });
    }

    /// <summary>Adds a transient option for a just-saved file the library watcher hasn't indexed yet.</summary>
    private void EnsureOptionFor(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;
        if (Playlists.Any(o => string.Equals(o.FilePath, filePath, StringComparison.OrdinalIgnoreCase))) return;

        var name = string.IsNullOrWhiteSpace(Name) ? System.IO.Path.GetFileName(filePath) : Name;
        Playlists.Add(new PlaylistOptionViewModel(filePath, name));
    }

    // ---- metadata fields → session ----

    partial void OnNameChanged(string value) => PushMetadata();
    partial void OnAuthorChanged(string value) => PushMetadata();
    partial void OnDescriptionChanged(string value) => PushMetadata();

    private void PushMetadata()
    {
        if (_suppressMetadataPush) return;
        _session.UpdateMetadata(_session.Metadata with
        {
            Name = Name,
            Author = string.IsNullOrWhiteSpace(Author) ? null : Author,
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
        });
    }

    // ---- selection (user) ----

    partial void OnSelectedPlaylistChanged(PlaylistOptionViewModel? oldValue, PlaylistOptionViewModel? newValue)
    {
        if (_suppressSelectionGuard || ReferenceEquals(oldValue, newValue))
            return;

        _ = HandleSelectionAsync(oldValue, newValue);
    }

    private async Task HandleSelectionAsync(PlaylistOptionViewModel? oldValue, PlaylistOptionViewModel? newValue)
    {
        if (!await ConfirmDiscardAsync())
        {
            RevertSelection(oldValue);
            return;
        }

        try
        {
            if (newValue is null || newValue.IsUnsaved)
                _session.DetachFromFile();
            else
                await _session.LoadAsync(newValue.FilePath!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load playlist '{Name}'.", newValue?.DisplayName);
            RevertSelection(oldValue);
        }
    }

    private void RevertSelection(PlaylistOptionViewModel? value)
        => WithSuppressedSelection(() => SelectedPlaylist = value);

    /// <summary>Runs <paramref name="action"/> with selection-change handling suppressed (re-entrant).</summary>
    private void WithSuppressedSelection(Action action)
    {
        var previous = _suppressSelectionGuard;
        _suppressSelectionGuard = true;
        try
        {
            action();
        }
        finally
        {
            _suppressSelectionGuard = previous;
        }
    }

    // ---- commands ----

    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task Save() => SaveCoreAsync();

    private bool CanReload => !string.IsNullOrEmpty(_session.SourceFilePath);

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(CanReload))]
    private async Task Reload()
    {
        if (_session.SourceFilePath is not { } path) return;
        if (!await ConfirmDiscardAsync()) return;

        try
        {
            await _session.LoadAsync(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reload playlist from {Path}.", path);
        }
    }

    /// <summary>
    /// Saves to the bound file, or — for an unsaved playlist — to the library playlists folder under a
    /// file name inferred from the playlist name. Returns success.
    /// </summary>
    private async Task<bool> SaveCoreAsync()
    {
        try
        {
            var path = _session.SourceFilePath;
            if (string.IsNullOrEmpty(path))
            {
                if (string.IsNullOrEmpty(_library.PlaylistsPath))
                {
                    _logger.LogError("Cannot save playlist: the library playlists folder is not configured.");
                    return false;
                }

                path = System.IO.Path.Combine(_library.PlaylistsPath, BuildFileName(Name));
            }

            await _session.SaveToAsync(path);
            EnsureOptionFor(path);
            SyncSelectedOption();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save playlist.");
            return false;
        }
    }

    /// <summary>Derives a safe <c>.lcplst</c> file name from the playlist name (invalid chars replaced).</summary>
    private static string BuildFileName(string? name)
    {
        var baseName = string.IsNullOrWhiteSpace(name) ? "Playlist" : name.Trim();
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
            baseName = baseName.Replace(c, '_');
        return baseName + ".lcplst";
    }

    /// <summary>Returns true to proceed (saved or discarded), false to cancel the pending action.</summary>
    private async Task<bool> ConfirmDiscardAsync()
    {
        if (!_session.IsDirty) return true;

        var label = string.IsNullOrWhiteSpace(Name) ? "The playlist" : $"'{Name}'";
        var message = new OpenSimpleDialogMessage(
            SimpleDialogButtons.YesNoCancel,
            $"{label} has unsaved changes. Save them first?",
            "Unsaved changes");
        WeakReferenceMessenger.Default.Send(message);
        await message.Completion.Task;

        return message.Result switch
        {
            DialogResult.Yes => await SaveCoreAsync(),
            DialogResult.No => true,
            _ => false,
        };
    }

    public void Dispose()
    {
        _session.PropertyChanged -= OnSessionPropertyChanged;
        _library.PlaylistsChanged -= OnLibraryPlaylistsChanged;
    }
}
