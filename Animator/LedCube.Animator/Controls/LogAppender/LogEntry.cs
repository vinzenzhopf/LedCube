using System;
using System.Windows.Media;

namespace LedCube.Animator.Controls.LogAppender;

public record LogEntry(Brush Color, string Level, DateTimeOffset Time, string Logger, string Message);