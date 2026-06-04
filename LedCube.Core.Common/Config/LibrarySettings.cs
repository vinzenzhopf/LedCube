namespace LedCube.Core.Common.Config;

public record LibrarySettings
{
    public string LibraryPath { get; init; } = @"%UserProfile%\LedCube\Library";
    public string AnimationsPath { get; init; } = @"${LibraryPath}/Animations";
    public string PlaylistsPath { get; init; } = @"${LibraryPath}/Playlists";
    public string ProjectsPath { get; init; } = @"${LibraryPath}/Project";
    public bool WatchDirectory { get; init; } = true;
}