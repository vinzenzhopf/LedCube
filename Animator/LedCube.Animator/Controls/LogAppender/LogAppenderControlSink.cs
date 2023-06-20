using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog.Core;
using Serilog.Events;

namespace LedCube.Animator.Controls.LogAppender;

public sealed class LogAppenderControlSink : ILogEventSink
{
	private ObservableCollection<LogEntry>? _logEntries;

	public int MaxEntries { get; set; } = 1024;
	public IFormatProvider? FormatProvider { get; set; } = null;

	public LogAppenderControlSink()
	{
	}

	public void Register(ObservableCollection<LogEntry> logEntries)
	{
		_logEntries = logEntries;
	}

	private static Brush BrushFromLevel(LogEventLevel level)
    {
	    return level switch
	    {
		    LogEventLevel.Fatal => Brushes.DarkRed,
		    LogEventLevel.Error => Brushes.Red,
		    LogEventLevel.Warning => Brushes.Orange,
		    LogEventLevel.Information => Brushes.RoyalBlue,
		    LogEventLevel.Debug => Brushes.ForestGreen,
		    LogEventLevel.Verbose => Brushes.DimGray,
		    _ => Brushes.Black
	    };
    }

    public void Emit(LogEvent logEvent)
    {
	    if (_logEntries == null) return;
	    
	    var message = logEvent.RenderMessage(FormatProvider);
	    logEvent.Properties.TryGetValue("SourceContext", out var context);
	    var entry = new LogEntry(
		    BrushFromLevel(logEvent.Level), 
		    logEvent.Level.ToString(),
		    DateTimeOffset.Now,
		    context?.ToString() ?? "",
		    message);

	    System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,
		    (Action) (() =>
		    {
			    _logEntries.Insert(0, entry);
			    if (_logEntries.Count > MaxEntries)
			    {
				    _logEntries.RemoveAt(_logEntries.Count - 1);
			    }
		    }));
    }
}
