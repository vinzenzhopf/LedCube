using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LedCube.Core.UI.Services.Library;
using LedCube.PluginBase;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;
using FilePlaylist = LedCube.Animation.FileFormat.Playlist.Model.Playlist;
using FilePlaylistEntry = LedCube.Animation.FileFormat.Playlist.Model.PlaylistEntry;
using FilePlaylistManifest = LedCube.Animation.FileFormat.Playlist.Model.PlaylistManifest;
using FileRepeatMode = LedCube.Animation.FileFormat.Playlist.Model.PlaylistRepeatMode;

namespace LedCube.Core.UI.Services.Playlist;

/// <summary>
/// Maps between the pure <c>.lcplst</c> format model and live UI playlist state: resolves plugin
/// types via <see cref="IPluginManager"/>, coerces config values to their descriptor types, and
/// resolves file references against the library (library-first, then the playlist directory).
/// </summary>
public sealed class PlaylistFileConverter(
    ILogger<PlaylistFileConverter> logger,
    IPluginManager pluginManager,
    ILibraryService libraryService) : IPlaylistFileConverter
{
    public PlaylistLoadResult ToUiPlaylist(FilePlaylist model, string? playlistDirectory)
    {
        ArgumentNullException.ThrowIfNull(model);

        var generators = pluginManager.AllFrameGeneratorInfos().ToList();
        var entries = new List<PlaylistEntry>(model.Entries.Count);
        var unresolved = new List<string>();

        foreach (var source in model.Entries)
        {
            var generator = generators.FirstOrDefault(g => g.TypeInfo.FullName == source.TypeName);
            if (generator is null)
            {
                logger.LogWarning("Playlist entry references unknown generator type '{TypeName}'; skipping.",
                    source.TypeName);
                unresolved.Add(source.TypeName);
                continue;
            }

            entries.Add(BuildUiEntry(generator, source, playlistDirectory));
        }

        var metadata = new PlaylistMetadata(
            model.Manifest.Name,
            model.Manifest.Author,
            model.Manifest.Description,
            model.Manifest.CreatedUtc);

        return new PlaylistLoadResult(entries, ToUiRepeatMode(model.Manifest.RepeatMode), metadata, unresolved);
    }

    public FilePlaylist ToModel(
        IReadOnlyList<PlaylistEntry> entries,
        PlaylistRepeatMode repeatMode,
        PlaylistMetadata metadata,
        string? playlistDirectory)
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentNullException.ThrowIfNull(metadata);

        var modelEntries = entries.Select(e => BuildModelEntry(e, playlistDirectory)).ToList();

        var manifest = new FilePlaylistManifest
        {
            Name = metadata.Name,
            Author = metadata.Author,
            Description = metadata.Description,
            CreatedUtc = metadata.CreatedUtc,
            RepeatMode = ToFileRepeatMode(repeatMode),
        };

        return new FilePlaylist(manifest, modelEntries);
    }

    private PlaylistEntry BuildUiEntry(FrameGeneratorEntry generator, FilePlaylistEntry source, string? playlistDirectory)
    {
        var entry = new PlaylistEntry(generator.Info, generator.TypeInfo)
        {
            InstanceName = source.InstanceName ?? generator.Info.Name,
            RepeatCount = source.RepeatCount,
            FrameTimeOverride = source.FrameTimeUsOverride is { } us
                ? TimeSpan.FromMicroseconds(us)
                : null,
        };

        var filePathKey = FilePathKey(generator.Info);
        foreach (var (key, value) in source.Config)
        {
            var descriptor = generator.Info.ConfigDescriptors?.FirstOrDefault(d => d.Key == key);
            entry.Config[key] = key == filePathKey
                ? ResolveFilePath(value as string, playlistDirectory)
                : Coerce(value, descriptor?.Type);
        }

        return entry;
    }

    private FilePlaylistEntry BuildModelEntry(PlaylistEntry entry, string? playlistDirectory)
    {
        var typeName = entry.TypeInfo.FullName ?? entry.TypeInfo.Name;
        var filePathKey = FilePathKey(entry.Info);

        Dictionary<string, object?>? config = null;
        if (entry.Config.Count > 0)
        {
            config = new Dictionary<string, object?>(entry.Config.Count);
            foreach (var (key, value) in entry.Config)
            {
                config[key] = key == filePathKey
                    ? RelativizeFilePath(value as string, playlistDirectory)
                    : ToModelValue(value);
            }
        }

        return new FilePlaylistEntry(
            typeName: typeName,
            id: null,
            instanceName: string.IsNullOrWhiteSpace(entry.InstanceName) ? null : entry.InstanceName,
            repeatCount: entry.RepeatCount,
            frameTimeUsOverride: entry.FrameTimeOverride is { } ts
                ? (uint)Math.Round(ts.TotalMicroseconds)
                : null,
            config: config);
    }

    // The FileAnimation generator is identified structurally as the one exposing a FilePath
    // descriptor, so the UI need not reference the plugin assembly directly.
    private static string? FilePathKey(FrameGeneratorInfo info) =>
        info.ConfigDescriptors?.FirstOrDefault(d => d.Type == AnimationConfigType.FilePath)?.Key;

    /// <summary>Coerces a normalized JSON primitive to the CLR type the descriptor (and generator) expects.</summary>
    private static object? Coerce(object? value, AnimationConfigType? type)
    {
        if (value is null) return null;
        return type switch
        {
            AnimationConfigType.Int => Convert.ToInt32(value, CultureInfo.InvariantCulture),
            AnimationConfigType.Float => Convert.ToSingle(value, CultureInfo.InvariantCulture),
            AnimationConfigType.Bool => Convert.ToBoolean(value, CultureInfo.InvariantCulture),
            // String / Enum / FilePath (and unknown keys) keep a string value.
            _ => value.ToString(),
        };
    }

    /// <summary>Normalizes a live config value to the format model's primitive set (string/long/double/bool/null).</summary>
    private static object? ToModelValue(object? value) => value switch
    {
        null => null,
        bool b => b,
        int i => (long)i,
        long l => l,
        float f => (double)f,
        double d => d,
        string s => s,
        _ => value.ToString(),
    };

    /// <summary>Library-first resolution: absolute as-is, else under Animations, else under the playlist dir.</summary>
    private string? ResolveFilePath(string? reference, string? playlistDirectory)
    {
        if (string.IsNullOrEmpty(reference)) return reference;
        if (Path.IsPathRooted(reference)) return reference;

        var animationsPath = libraryService.AnimationsPath;
        var libraryCandidate = string.IsNullOrEmpty(animationsPath)
            ? null
            : Path.GetFullPath(Path.Combine(animationsPath, reference));
        var playlistCandidate = string.IsNullOrEmpty(playlistDirectory)
            ? null
            : Path.GetFullPath(Path.Combine(playlistDirectory, reference));

        // Prefer a file that actually exists (library first); otherwise default to the best
        // candidate (a missing file is kept and surfaces at play time).
        if (libraryCandidate is not null && File.Exists(libraryCandidate)) return libraryCandidate;
        if (playlistCandidate is not null && File.Exists(playlistCandidate)) return playlistCandidate;
        return libraryCandidate ?? playlistCandidate ?? reference;
    }

    /// <summary>Stores a library-relative (or playlist-relative) reference when possible, else an absolute path.</summary>
    private string? RelativizeFilePath(string? absolutePath, string? playlistDirectory)
    {
        if (string.IsNullOrEmpty(absolutePath)) return absolutePath;
        if (!Path.IsPathRooted(absolutePath)) return ToForwardSlashes(absolutePath);

        if (TryMakeRelative(libraryService.AnimationsPath, absolutePath, out var libraryRelative))
            return libraryRelative;

        if (TryMakeRelative(playlistDirectory, absolutePath, out var playlistRelative))
            return playlistRelative;

        return ToForwardSlashes(absolutePath);
    }

    private static bool TryMakeRelative(string? baseDir, string absolutePath, out string relative)
    {
        relative = string.Empty;
        if (string.IsNullOrEmpty(baseDir)) return false;

        var rel = Path.GetRelativePath(baseDir, absolutePath);
        // Outside the base dir (escapes via "..") or a different root → not relative to it.
        if (rel == absolutePath || rel.StartsWith("..", StringComparison.Ordinal) || Path.IsPathRooted(rel))
            return false;

        relative = ToForwardSlashes(rel);
        return true;
    }

    private static string ToForwardSlashes(string path) => path.Replace('\\', '/');

    private static PlaylistRepeatMode ToUiRepeatMode(FileRepeatMode mode) => mode switch
    {
        FileRepeatMode.StopAtEnd => PlaylistRepeatMode.StopAtEnd,
        FileRepeatMode.RepeatCurrentEntry => PlaylistRepeatMode.RepeatCurrentEntry,
        FileRepeatMode.FairRandomPlay => PlaylistRepeatMode.FairRandomPlay,
        FileRepeatMode.TrueRandomPlay => PlaylistRepeatMode.TrueRandomPlay,
        _ => PlaylistRepeatMode.LoopWholePlaylist,
    };

    private static FileRepeatMode ToFileRepeatMode(PlaylistRepeatMode mode) => mode switch
    {
        PlaylistRepeatMode.StopAtEnd => FileRepeatMode.StopAtEnd,
        PlaylistRepeatMode.RepeatCurrentEntry => FileRepeatMode.RepeatCurrentEntry,
        PlaylistRepeatMode.FairRandomPlay => FileRepeatMode.FairRandomPlay,
        PlaylistRepeatMode.TrueRandomPlay => FileRepeatMode.TrueRandomPlay,
        _ => FileRepeatMode.LoopWholePlaylist,
    };
}
