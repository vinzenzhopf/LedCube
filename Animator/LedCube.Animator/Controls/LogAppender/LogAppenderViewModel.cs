using System.Collections.ObjectModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace LedCube.Animator.Controls.LogAppender;

[ObservableObject]
public partial class LogAppenderViewModel
{
    [ObservableProperty]
    private bool _showLog = true;
    public ObservableCollection<LogEntry> LogEntries { get; } = new ObservableCollection<LogEntry>();

    public LogAppenderViewModel(LogAppenderControlSink logAppenderControlSink)
    {
        logAppenderControlSink.Register(LogEntries);
    }

    [RelayCommand]
    private void OnClose()
    {
        ShowLog = false;
    }

    [RelayCommand]
    private void OnClear()
    {
        LogEntries.Clear();
    }
}