using System;
using Serilog.Events;

namespace LedCube.Core.UI.Controls.LogAppender;

public record LogEntry(LogEventLevel Severity, string Level, DateTimeOffset Time, string Logger, string Message);
