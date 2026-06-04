namespace LedCube.Core.UI.Services;

/// <summary>
/// Holds the absolute path of the application's log file so UI components
/// (e.g. an "open log file" command) can locate it.
/// </summary>
public sealed record AppLogFileInfo(string FullPath);
