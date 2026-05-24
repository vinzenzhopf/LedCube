using System;
using Avalonia.Media;
using Serilog.Events;

namespace LedCube.Core.UI.Controls.LogAppender;

public record LogEntry(IBrush Color, LogEventLevel Severity, string Level, DateTimeOffset Time, string Logger, string Message);
