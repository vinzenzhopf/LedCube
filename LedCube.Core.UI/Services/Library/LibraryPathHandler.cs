using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LedCube.Core.UI.Services.Library.Model;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Services.Library;

/// <summary>
/// Owns the discovered entries for a single library sub-folder (one file kind). Watches the folder
/// for changes and (re)builds entries via an async factory. The entry dictionary is only ever
/// mutated under <see cref="_gate"/>; the (potentially slow) factory runs outside the lock so file
/// I/O never blocks readers. Reads go through <see cref="Snapshot"/>, which copies under the lock to
/// avoid "collection was modified" while the watcher mutates on a thread-pool thread.
/// </summary>
public class LibraryPathHandler<T> : IDisposable
{
    private readonly ILogger<LibraryPathHandler<T>> _logger;
    private readonly Func<string, CancellationToken, Task<T>> _entryFactory;
    private readonly LibraryPathWatcher _watcher;
    private readonly string _filter;

    private readonly Dictionary<string, T> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();
    private readonly CancellationTokenSource _cts = new();

    public event EventHandler<LibraryChangeEventArgs<T>>? Changed;

    public LibraryPathHandler(ILogger<LibraryPathHandler<T>> logger, string filter,
        Func<string, CancellationToken, Task<T>> entryFactory)
    {
        _logger = logger;
        _entryFactory = entryFactory;
        _watcher = new LibraryPathWatcher(filter, OnFileChange);
        _filter = filter;
    }

    /// <summary>Thread-safe copy of the current entries, safe to enumerate by the UI.</summary>
    public IReadOnlyList<T> Snapshot()
    {
        lock (_gate)
        {
            return _entries.Values.ToList();
        }
    }

    /// <summary>Start (or re-point) the file-system watcher at <paramref name="path"/>.</summary>
    public void Watch(string path) => _watcher.Watch(path);

    /// <summary>Stop watching for changes (e.g. when watching is disabled in settings).</summary>
    public void StopWatching() => _watcher.Stop();

    /// <summary>
    /// Discover everything currently in <paramref name="path"/>. Any previously known entries are
    /// dropped first (so this doubles as a re-scan after the configured path changes). Entries are
    /// built sequentially off the calling (background) thread; <see cref="Changed"/> fires per entry.
    /// </summary>
    public async Task ScanAsync(string path, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token);
        var ct = linked.Token;

        // Drop stale entries first (no-op on the initial scan).
        List<string> stale;
        lock (_gate)
        {
            stale = _entries.Keys.ToList();
        }
        foreach (var key in stale)
            RemoveAndNotify(key);

        if (!Directory.Exists(path))
            return;

        foreach (var file in Directory.EnumerateFiles(path, _filter))
        {
            ct.ThrowIfCancellationRequested();
            await AddOrUpdateAsync(file, ct).ConfigureAwait(false);
        }
    }

    // Watcher callback (thread-pool thread). The factory is async, so kick the work off and
    // surface faults via logging instead of letting them escape onto the watcher's timer thread.
    private void OnFileChange(LibraryPathWatcher.FileChange c)
    {
        _ = HandleChangeAsync(c);
    }

    private async Task HandleChangeAsync(LibraryPathWatcher.FileChange c)
    {
        try
        {
            switch (c.Kind)
            {
                case LibraryPathWatcher.ChangeKind.Created:
                case LibraryPathWatcher.ChangeKind.Changed:
                    await AddOrUpdateAsync(c.FullPath, _cts.Token).ConfigureAwait(false);
                    break;
                case LibraryPathWatcher.ChangeKind.Deleted:
                    RemoveAndNotify(c.FullPath);
                    break;
                case LibraryPathWatcher.ChangeKind.Renamed:
                    // A rename is reported as a remove of the old path plus an add of the new one.
                    if (c.OldPath is not null)
                        RemoveAndNotify(c.OldPath);
                    await AddOrUpdateAsync(c.FullPath, _cts.Token).ConfigureAwait(false);
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            // shutting down / re-scanning — ignore
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to process change '{Kind}' for '{Path}'.", c.Kind, c.FullPath);
        }
    }

    private async Task AddOrUpdateAsync(string fullPath, CancellationToken cancellationToken)
    {
        // Build OUTSIDE the lock — this may open and parse the file.
        var newEntry = await _entryFactory(fullPath, cancellationToken).ConfigureAwait(false);

        LibraryChangeEventArgs<T> change;
        lock (_gate)
        {
            var existed = _entries.TryGetValue(fullPath, out var oldEntry);
            _entries[fullPath] = newEntry;
            change = new LibraryChangeEventArgs<T>(
                existed ? LibraryChangeKind.Updated : LibraryChangeKind.Added,
                newEntry, existed ? oldEntry : default, fullPath);
        }

        Changed?.Invoke(this, change);
    }

    private void RemoveAndNotify(string fullPath)
    {
        LibraryChangeEventArgs<T>? change = null;
        lock (_gate)
        {
            if (_entries.Remove(fullPath, out var oldEntry))
                change = new LibraryChangeEventArgs<T>(LibraryChangeKind.Removed, default, oldEntry, fullPath);
        }

        if (change is not null)
            Changed?.Invoke(this, change);
    }

    public void Dispose()
    {
        _cts.Cancel();
        _watcher.Dispose();
        _cts.Dispose();
    }
}
