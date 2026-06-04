using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace LedCube.Core.UI.Services.Library;

public class LibraryPathWatcher : IDisposable
{
    public enum ChangeKind { Created, Changed, Deleted, Renamed }
    public sealed record FileChange(ChangeKind Kind, string FullPath, string? OldPath = null);

    private sealed class Pending
    {
        public Timer Timer = null!;
        public FileChange Change = null!;
    }

    private readonly string _filter;
    private readonly Action<FileChange> _onChange;
    private readonly int _debounceMs;
    private readonly ConcurrentDictionary<string, Pending> _pending =
        new(StringComparer.OrdinalIgnoreCase);

    private FileSystemWatcher? _watcher;

    public LibraryPathWatcher(string filter, Action<FileChange> onChange, int debounceMs = 250)
    {
        _filter = filter;
        _onChange = onChange;
        _debounceMs = debounceMs;
    }

    /// <summary>Point the watcher at a new path. Safe to call repeatedly (e.g. on settings change).</summary>
    public void Watch(string path)
    {
        Stop();
        Directory.CreateDirectory(path);   // you already do this, but be defensive

        _watcher = new FileSystemWatcher(path, _filter)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            IncludeSubdirectories = false,
        };
        _watcher.Created += (_, e) => Queue(new FileChange(ChangeKind.Created, e.FullPath));
        _watcher.Changed += (_, e) => Queue(new FileChange(ChangeKind.Changed, e.FullPath));
        _watcher.Deleted += (_, e) => Queue(new FileChange(ChangeKind.Deleted, e.FullPath));
        _watcher.Renamed += (_, e) => Queue(new FileChange(ChangeKind.Renamed, e.FullPath, e.OldFullPath));
        _watcher.Error   += OnError;
        _watcher.EnableRaisingEvents = true;
    }

    private void Queue(FileChange change)
    {
        _pending.AddOrUpdate(change.FullPath,
            key =>
            {
                var p = new Pending { Change = change };
                p.Timer = new Timer(_ => Flush(key), null, _debounceMs, Timeout.Infinite);
                return p;
            },
            (key, existing) =>
            {
                existing.Change = change;                                // keep the newest
                existing.Timer.Change(_debounceMs, Timeout.Infinite);    // reset the window
                return existing;
            });
    }

    private void Flush(string path)
    {
        if (_pending.TryRemove(path, out var p))
        {
            p.Timer.Dispose();
            _onChange(p.Change);   // fires on a thread-pool thread — see note below
        }
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        // Usually InternalBufferOverflowException. Re-establish the watcher.
        if (_watcher is not null)
        {
            var path = _watcher.Path;
            Watch(path);   // caller should also rescan the folder, since events were lost
        }
    }

    /// <summary>Stop watching and drop any pending debounced changes. Safe to call repeatedly.</summary>
    public void Stop()
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;
        }
        foreach (var p in _pending.Values) p.Timer.Dispose();
        _pending.Clear();
    }

    public void Dispose() => Stop();
}