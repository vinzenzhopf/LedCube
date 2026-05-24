using System;
using System.Collections.Concurrent;
using Avalonia.Media;
using Serilog.Core;
using Serilog.Events;

namespace LedCube.Core.UI.Controls.LogAppender;

public sealed class LogAppenderControlSink : ILogEventSink
{
    private readonly ConcurrentQueue<LogEntry> _queue = new();

    public IFormatProvider? FormatProvider { get; set; }

    private static IBrush BrushFromLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Fatal => Brushes.DarkRed,
        LogEventLevel.Error => Brushes.Red,
        LogEventLevel.Warning => Brushes.Orange,
        LogEventLevel.Information => Brushes.RoyalBlue,
        LogEventLevel.Debug => Brushes.ForestGreen,
        LogEventLevel.Verbose => Brushes.DimGray,
        _ => Brushes.Black
    };

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(FormatProvider);
        logEvent.Properties.TryGetValue("SourceContext", out var context);
        var entry = new LogEntry(
            BrushFromLevel(logEvent.Level),
            logEvent.Level,
            logEvent.Level.ToString(),
            DateTimeOffset.Now,
            context?.ToString() ?? "",
            message);
        _queue.Enqueue(entry);
    }

    public bool TryDequeue(out LogEntry entry) => _queue.TryDequeue(out entry!);
}
