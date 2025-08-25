using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LedCube.Core.UI.Controls.LogAppender;

public partial class LogAppenderViewModel : ObservableObject
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