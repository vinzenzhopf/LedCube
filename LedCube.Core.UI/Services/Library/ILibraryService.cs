using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LedCube.Core.UI.Services.Library.Model;

namespace LedCube.Core.UI.Services.Library;

public interface ILibraryService
{
    string LibraryPath { get; }
    string AnimationsPath { get; }
    string PlaylistsPath { get; }
    string ProjectsPath { get; }
    bool WatchDirectory { get; }

    IReadOnlyCollection<LibraryAnimationEntry> AnimationEntries { get; }
    event EventHandler<LibraryChangeEventArgs<LibraryAnimationEntry>>? AnimationsChanged;
    
    IReadOnlyCollection<LibraryPlaylistEntry> PlaylistEntries { get; }
    event EventHandler<LibraryChangeEventArgs<LibraryPlaylistEntry>>? PlaylistsChanged;
    
    IReadOnlyCollection<LibraryProjectEntry> ProjectEntries { get; }
    event EventHandler<LibraryChangeEventArgs<LibraryProjectEntry>>? ProjectsChanged;
}