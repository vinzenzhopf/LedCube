using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Animation.FileFormat.Playlist.Io;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Services.Playlist;

public partial class PlaylistSessionService : ObservableObject, IPlaylistSessionService,
    IRecipient<PlaylistEntryConfigChangedMessage>,
    IRecipient<PlaylistEntryEditedMessage>
{
    private readonly ILogger<PlaylistSessionService> _logger;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaylistFileConverter _converter;

    // Set while applying a loaded playlist so the resulting collection churn is not counted as edits.
    private bool _suppressDirty;

    [ObservableProperty]
    private PlaylistMetadata _metadata = PlaylistMetadata.Empty;

    [ObservableProperty]
    private string? _sourceFilePath;

    [ObservableProperty]
    private bool _isDirty;

    public PlaylistSessionService(
        ILogger<PlaylistSessionService> logger,
        IPlaylistService playlistService,
        IPlaylistFileConverter converter)
    {
        _logger = logger;
        _playlistService = playlistService;
        _converter = converter;

        ((INotifyCollectionChanged)_playlistService.Entries).CollectionChanged += OnEntriesChanged;
        _playlistService.PropertyChanged += OnPlaylistPropertyChanged;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e) => MarkDirty();

    private void OnPlaylistPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IPlaylistService.RepeatMode))
            MarkDirty();
    }

    public void Receive(PlaylistEntryConfigChangedMessage message) => MarkDirty();

    public void Receive(PlaylistEntryEditedMessage message) => MarkDirty();

    private void MarkDirty()
    {
        if (!_suppressDirty)
            IsDirty = true;
    }

    public void UpdateMetadata(PlaylistMetadata metadata)
    {
        Metadata = metadata;
        IsDirty = true;
    }

    public async Task LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var model = await Task.Run(() =>
        {
            using var stream = File.OpenRead(filePath);
            return LcPlstReader.Read(stream);
        }, cancellationToken).ConfigureAwait(true);

        var directory = Path.GetDirectoryName(filePath);
        var result = _converter.ToUiPlaylist(model, directory);

        if (result.UnresolvedTypeNames.Count > 0)
        {
            _logger.LogWarning("Playlist '{File}' skipped {Count} entries with unknown generators: {Types}",
                filePath, result.UnresolvedTypeNames.Count, string.Join(", ", result.UnresolvedTypeNames));
        }

        _suppressDirty = true;
        try
        {
            _playlistService.Clear();
            foreach (var entry in result.Entries)
                _playlistService.Add(entry);
            _playlistService.RepeatMode = result.RepeatMode;
            Metadata = result.Metadata;
            SourceFilePath = filePath;
        }
        finally
        {
            _suppressDirty = false;
        }

        IsDirty = false;
        _logger.LogInformation("Loaded playlist '{Name}' ({Count} entries) from {File}.",
            Metadata.Name, result.Entries.Count, filePath);
    }

    public Task SaveAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(SourceFilePath))
            throw new InvalidOperationException("The playlist has no source file; use SaveToAsync instead.");
        return SaveToAsync(SourceFilePath, cancellationToken);
    }

    public async Task SaveToAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        // A playlist file requires a name; default to the file name when the user left it blank.
        var name = string.IsNullOrWhiteSpace(Metadata.Name)
            ? Path.GetFileNameWithoutExtension(filePath)
            : Metadata.Name;
        var metadata = Metadata with { Name = name };
        var directory = Path.GetDirectoryName(filePath);

        var model = _converter.ToModel(
            _playlistService.Entries.ToList(),
            _playlistService.RepeatMode,
            metadata,
            directory);

        await Task.Run(() =>
        {
            using var stream = File.Create(filePath);
            LcPlstWriter.Write(stream, model);
        }, cancellationToken).ConfigureAwait(true);

        Metadata = metadata;
        SourceFilePath = filePath;
        IsDirty = false;
        _logger.LogInformation("Saved playlist '{Name}' to {File}.", metadata.Name, filePath);
    }

    public void DetachFromFile()
    {
        SourceFilePath = null;
        // No longer backed by a file → treat as unsaved so Save offers a save-as.
        IsDirty = true;
    }
}
