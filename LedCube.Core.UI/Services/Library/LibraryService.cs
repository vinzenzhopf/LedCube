using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Core.Common.Config;
using LedCube.Core.Common.Settings;
using LedCube.Core.UI.Services.Library.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Services.Library;

public partial class LibraryService : ObservableObject, ILibraryService, IHostedService, IDisposable
{
    private readonly ILogger<LibraryService> _logger;
    private readonly ISettingsProvider<LibrarySettings> _settingsProvider;

    [ObservableProperty]
    public partial string LibraryPath { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string AnimationsPath { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string PlaylistsPath { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial string ProjectsPath { get; private set; } = string.Empty;
    [ObservableProperty]
    public partial bool WatchDirectory { get; private set; } = false;

    private readonly LibraryPathHandler<LibraryAnimationEntry> _animationEntriesHandler;
    private readonly LibraryPathHandler<LibraryPlaylistEntry> _playlistEntriesHandler;
    private readonly LibraryPathHandler<LibraryProjectEntry> _projectEntriesHandler;

    // Cancels in-flight discovery on shutdown.
    private readonly CancellationTokenSource _cts = new();

    public IReadOnlyCollection<LibraryAnimationEntry> AnimationEntries => _animationEntriesHandler.Snapshot();
    public IReadOnlyCollection<LibraryPlaylistEntry> PlaylistEntries => _playlistEntriesHandler.Snapshot();
    public IReadOnlyCollection<LibraryProjectEntry> ProjectEntries => _projectEntriesHandler.Snapshot();

    public event EventHandler<LibraryChangeEventArgs<LibraryAnimationEntry>>? AnimationsChanged;
    public event EventHandler<LibraryChangeEventArgs<LibraryPlaylistEntry>>? PlaylistsChanged;
    public event EventHandler<LibraryChangeEventArgs<LibraryProjectEntry>>? ProjectsChanged;

    public LibraryService(ILogger<LibraryService> logger, ILoggerFactory loggerFactory, ISettingsProvider<LibrarySettings> settingsProvider)
    {
        _logger = logger;
        _settingsProvider = settingsProvider;

        _animationEntriesHandler = new(loggerFactory.CreateLogger<LibraryPathHandler<LibraryAnimationEntry>>(), "*.lcanimraw", BuildAnimationEntry);
        _playlistEntriesHandler = new(loggerFactory.CreateLogger<LibraryPathHandler<LibraryPlaylistEntry>>(), "*.lcplst", BuildPlaylistEntry);
        _projectEntriesHandler = new(loggerFactory.CreateLogger<LibraryPathHandler<LibraryProjectEntry>>(), "*.lcanim", BuildProjectEntry);

        _animationEntriesHandler.Changed += (_, e) => AnimationsChanged?.Invoke(this, e);
        _playlistEntriesHandler.Changed += (_, e) => PlaylistsChanged?.Invoke(this, e);
        _projectEntriesHandler.Changed += (_, e) => ProjectsChanged?.Invoke(this, e);

        // Constructor only resolves paths from config — no file I/O or watchers here. Discovery is
        // driven by the hosted-service lifecycle (StartAsync) so host startup is never blocked.
        LoadFromConfig(_settingsProvider.Settings);
        _settingsProvider.SettingsChanged += OnSettingsChanged;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RestartDiscovery();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }

    /// <summary>
    /// (Re)configure the watchers and kick off a full background scan. Watchers are wired up first so
    /// files created during the scan are not missed (a re-add is harmless — it keys on the path).
    /// </summary>
    private void RestartDiscovery()
    {
        ConfigureWatchers();

        var token = _cts.Token;
        _ = Task.Run(() => ScanAllAsync(token), token)
            .ContinueWith(t => _logger.LogError(t.Exception, "Library discovery scan failed."),
                CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }

    private void ConfigureWatchers()
    {
        if (WatchDirectory)
        {
            _animationEntriesHandler.Watch(AnimationsPath);
            _playlistEntriesHandler.Watch(PlaylistsPath);
            _projectEntriesHandler.Watch(ProjectsPath);
        }
        else
        {
            _animationEntriesHandler.StopWatching();
            _playlistEntriesHandler.StopWatching();
            _projectEntriesHandler.StopWatching();
        }
    }

    private Task ScanAllAsync(CancellationToken cancellationToken) => Task.WhenAll(
        _animationEntriesHandler.ScanAsync(AnimationsPath, cancellationToken),
        _playlistEntriesHandler.ScanAsync(PlaylistsPath, cancellationToken),
        _projectEntriesHandler.ScanAsync(ProjectsPath, cancellationToken));

    private static async Task<LibraryAnimationEntry> BuildAnimationEntry(string fullPath, CancellationToken cancellationToken)
    {
        // Reading the manifest is cheap (no frame decompression) but still touches the disk —
        // run it off the calling thread so the watcher/scan pipeline stays responsive.
        return await Task.Run(() =>
        {
            using var stream = File.OpenRead(fullPath);
            var manifest = LcAnimRawReader.ReadManifest(stream);
            return new LibraryAnimationEntry
            {
                FilePath = fullPath,
                Name = manifest.Name,
                Author = manifest.Author,
                Description = manifest.Description,
                CreatedUtc = manifest.CreatedUtc,
                Size = manifest.Size,
                FrameCount = manifest.FrameCount,
                FrameTimeUs = manifest.FrameTimeUs,
                SeamlessLoop = manifest.SeamlessLoop,
            };
        }, cancellationToken).ConfigureAwait(false);
    }

    // Playlists and projects are not parsed yet — only the path is tracked for now.
    private static Task<LibraryPlaylistEntry> BuildPlaylistEntry(string fullPath, CancellationToken cancellationToken)
        => Task.FromResult(new LibraryPlaylistEntry { FilePath = fullPath });

    private static Task<LibraryProjectEntry> BuildProjectEntry(string fullPath, CancellationToken cancellationToken)
        => Task.FromResult(new LibraryProjectEntry { FilePath = fullPath });

    private void OnSettingsChanged(object? sender, LibrarySettings settings)
    {
        LoadFromConfig(settings);
        RestartDiscovery();
    }

    private void LoadFromConfig(LibrarySettings settings)
    {
        try
        {
            LibraryPath = ResolveAndCreatePath(settings.LibraryPath, null);
            AnimationsPath = ResolveAndCreatePath(settings.AnimationsPath, LibraryPath);
            PlaylistsPath = ResolveAndCreatePath(settings.PlaylistsPath, LibraryPath);
            ProjectsPath = ResolveAndCreatePath(settings.ProjectsPath, LibraryPath);
            WatchDirectory = settings.WatchDirectory;

            _logger.LogDebug("Updated Library Settings:\n" +
                             "LibraryPath: {LibraryPath}\n" +
                             "AnimationsPath: {AnimationsPath}\n" +
                             "PlaylistsPath: {PlaylistsPath}\n" +
                             "ProjectsPath: {ProjectsPath}",
                LibraryPath, AnimationsPath, PlaylistsPath, ProjectsPath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while loading library settings");
            throw;
        }
    }

    private static string ResolveAndCreatePath(string subPath, string? libraryPath)
    {
        var path = ResolveVariables(subPath, libraryPath);
        path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ResolveVariables(string path, string? libraryPath)
    {
        return PathVariableRegex().Replace(path, match =>
        {
            var name = match.Groups[1].Value;
            return name switch
            {
                "UserProfile" => Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                "LibraryPath" => libraryPath ??
                                 throw new InvalidOperationException("LibraryPath is not yet defined!"),
                _ => Environment.GetEnvironmentVariable(name)
                     ?? throw new InvalidOperationException(
                         $"Unknown variable '{name}'")
            };
        });
    }

    [GeneratedRegex(@"\$\{([^}]+)\}")]
    private static partial Regex PathVariableRegex();

    public void Dispose()
    {
        _cts.Cancel();
        _settingsProvider.SettingsChanged -= OnSettingsChanged;
        _animationEntriesHandler.Dispose();
        _playlistEntriesHandler.Dispose();
        _projectEntriesHandler.Dispose();
        _cts.Dispose();
    }
}
