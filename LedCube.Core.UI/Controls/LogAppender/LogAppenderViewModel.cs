using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog.Events;

namespace LedCube.Core.UI.Controls.LogAppender;

public partial class LogAppenderViewModel : ObservableObject
{
    private readonly LogAppenderControlSink _sink;
    private readonly List<LogEntry> _allEntries = new();
    private readonly DispatcherTimer _flushTimer;

    public int MaxEntries { get; set; } = 10_000;
    public TimeSpan FlushInterval
    {
        get => _flushTimer.Interval;
        set => _flushTimer.Interval = value;
    }

    [ObservableProperty]
    private bool _showLog = true;

    [ObservableProperty]
    private LogEventLevel _minLevel = LogEventLevel.Debug;

    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    public LogAppenderViewModel(LogAppenderControlSink logAppenderControlSink)
    {
        _sink = logAppenderControlSink;
        _flushTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(100),
            DispatcherPriority.Background,
            OnFlushTick);
        _flushTimer.Start();
    }

    private void OnFlushTick(object? sender, EventArgs e)
    {
        while (_sink.TryDequeue(out var entry))
        {
            _allEntries.Insert(0, entry);
            if (entry.Severity >= MinLevel)
                LogEntries.Insert(0, entry);
        }

        while (_allEntries.Count > MaxEntries)
        {
            var dropped = _allEntries[^1];
            _allEntries.RemoveAt(_allEntries.Count - 1);
            LogEntries.Remove(dropped);
        }
    }

    partial void OnMinLevelChanged(LogEventLevel value)
    {
        LogEntries.Clear();
        foreach (var item in _allEntries.Where(x => x.Severity >= value))
            LogEntries.Add(item);
    }

    [RelayCommand]
    private void SetMinLevel(LogEventLevel level) => MinLevel = level;

    [RelayCommand]
    private void Close()
    {
        ShowLog = false;
    }

    [RelayCommand]
    private void Clear()
    {
        _allEntries.Clear();
        LogEntries.Clear();
    }
}
