using System;
using System.Windows.Media;

namespace LedCube.Core.UI.Controls.LogAppender;

public record LogEntry(Brush Color, string Level, DateTimeOffset Time, string Logger, string Message);