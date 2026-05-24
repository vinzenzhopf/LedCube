using System;
using Avalonia.Media;

namespace LedCube.Animator.Controls.LogAppender;

public record LogEntry(IBrush Color, string Level, DateTimeOffset Time, string Logger, string Message);
