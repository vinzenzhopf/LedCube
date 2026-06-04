using System;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.Common.Settings;
using LedCube.Core.UI.Controls.LogAppender;
using LedCube.Core.UI.Messages;
using LedCube.Core.UI.Services;
using LedCube.Core.UI.Services.Library;
using Microsoft.Extensions.Logging;

namespace LedCube.Streamer.UI.Controls.MenuBar;

public partial class MenuBarViewModel : ObservableObject
{
    private readonly AppLogFileInfo _logFileInfo;
    private readonly ISettingsProvider<LedCubeStreamerSettings> _settingsProvider;
    private readonly ILibraryService _libraryService;
    private readonly ILogger<MenuBarViewModel> _logger;

    public LogAppenderViewModel AppenderViewModel { get; }

    public MenuBarViewModel(LogAppenderViewModel logAppenderViewModel,
        AppLogFileInfo logFileInfo,
        ISettingsProvider<LedCubeStreamerSettings> settingsProvider,
        ILibraryService libraryService,
        ILogger<MenuBarViewModel> logger)
    {
        AppenderViewModel = logAppenderViewModel;
        _logFileInfo = logFileInfo;
        _settingsProvider = settingsProvider;
        _libraryService = libraryService;
        _logger = logger;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        WeakReferenceMessenger.Default.Send<OpenSettingsNavigationMessage>();
    }

    [RelayCommand]
    private void OpenLogFile() => OpenFile(_logFileInfo.FullPath, "log file");

    [RelayCommand]
    private void OpenSettingsFile() => OpenFile(_settingsProvider.FilePath, "settings file");

    [RelayCommand]
    private void OpenLibraryDirectory() => OpenDirectory(_libraryService.LibraryPath, "library directory");

    // UseShellExecute opens the target with the OS-default handler (editor for files, Explorer for dirs).
    private void OpenFile(string path, string description)
    {
        try
        {
            if (!File.Exists(path))
            {
                _logger.LogWarning("The {description} does not exist (yet): {path}", description, path);
                return;
            }
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not open {description} {path}", description, path);
        }
    }

    private void OpenDirectory(string path, string description)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                _logger.LogWarning("The {description} does not exist (yet): {path}", description, path);
                return;
            }
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not open {description} {path}", description, path);
        }
    }

    [RelayCommand]
    private void ExitApplication()
    {
        WeakReferenceMessenger.Default.Send<ExitApplicationNavigationMessage>();
    }
}
