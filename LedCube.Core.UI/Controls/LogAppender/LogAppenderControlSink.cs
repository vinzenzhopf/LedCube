using System;
using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace LedCube.Core.UI.Controls.LogAppender;

public sealed class LogAppenderControlSink : ILogEventSink
{
    private readonly ConcurrentQueue<LogEntry> _queue = new();

    public IFormatProvider? FormatProvider { get; set; }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(FormatProvider);
        logEvent.Properties.TryGetValue("SourceContext", out var context);
        var entry = new LogEntry(
            logEvent.Level,
            logEvent.Level.ToString(),
            DateTimeOffset.Now,
            context?.ToString() ?? "",
            message);
        _queue.Enqueue(entry);
    }

    public bool TryDequeue(out LogEntry entry) => _queue.TryDequeue(out entry!);
}
