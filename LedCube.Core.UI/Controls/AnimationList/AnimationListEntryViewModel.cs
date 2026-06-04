using System;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.Common.Model;
using LedCube.Core.UI.Services.Library.Model;
using LedCube.PluginHost;
using Material.Icons;

namespace LedCube.Core.UI.Controls.AnimationList;

public class AnimationListEntryViewModel : ObservableObject
{
    /// <summary>In-process drag-and-drop format used when dragging an entry onto the playlist.</summary>
    public static readonly DataFormat<AnimationListEntryViewModel> DragFormat =
        DataFormat.CreateInProcessFormat<AnimationListEntryViewModel>("ledcube.AnimationListEntry");

    public string FilePath { get; init; }
    public string Name { get; init; }
    public string? Author { get; init; }
    public string? Description { get; init; }
    public DateTimeOffset? CreatedUtc { get; init; }
    public Point3D Size { get; init; }
    public int FrameCount { get; init; }
    public uint FrameTimeUs { get; init; }
    public bool SeamlessLoop { get; init; }
    public string FileName { get; init; }
    public AnimationListEntryType AnimationType { get; init; }

    /// <summary>Optional thumbnail; null falls back to a type icon in the UI. Not yet populated.</summary>
    public Bitmap? Thumbnail { get; init; }

    /// <summary>Set for plugin entries; the generator that produces the animation. Null for file entries.</summary>
    public FrameGeneratorEntry? GeneratorEntry { get; init; }

    /// <summary>Total runtime derived from the authored frame period and frame count.</summary>
    public TimeSpan Duration => TimeSpan.FromMicroseconds((double)FrameTimeUs * FrameCount);

    /// <summary>Authored frames per second (0 when the frame period is unknown).</summary>
    public double Fps => FrameTimeUs > 0 ? 1_000_000d / FrameTimeUs : 0d;

    public string DurationText => FrameCount > 0 ? Duration.ToString(@"hh\:mm\:ss\.fff") : string.Empty;
    public string FpsText => Fps > 0 ? Fps.ToString("F2") : string.Empty;

    /// <summary>Compact "fps" label for list rows; empty when the rate is unknown.</summary>
    public string FpsLabel => Fps > 0 ? $"{Fps:F1} fps" : string.Empty;

    /// <summary>Cube size as "X×Y×Z"; empty for entries without a size (e.g. plugins).</summary>
    public string SizeText => Size.IsEmpty ? string.Empty : $"{Size.X}×{Size.Y}×{Size.Z}";

    public string CreatedText => CreatedUtc?.ToLocalTime().ToString("g") ?? string.Empty;

    public string TypeText => AnimationType == AnimationListEntryType.FileAnimation ? "File" : "Plugin";

    public MaterialIconKind TypeIcon => AnimationType == AnimationListEntryType.FileAnimation
        ? MaterialIconKind.FileStar
        : MaterialIconKind.MovieCogOutline;

    /// <summary>A file-backed animation discovered in the library.</summary>
    public AnimationListEntryViewModel(LibraryAnimationEntry libraryAnimationEntry)
    {
        AnimationType = AnimationListEntryType.FileAnimation;
        FilePath = libraryAnimationEntry.FilePath;
        Name = libraryAnimationEntry.Name;
        Author = libraryAnimationEntry.Author;
        Description = libraryAnimationEntry.Description;
        CreatedUtc = libraryAnimationEntry.CreatedUtc;
        Size = libraryAnimationEntry.Size;
        FrameCount = libraryAnimationEntry.FrameCount;
        FrameTimeUs = libraryAnimationEntry.FrameTimeUs;
        SeamlessLoop = libraryAnimationEntry.SeamlessLoop;
        FileName = libraryAnimationEntry.FileName;
    }

    /// <summary>A procedural animation provided by a loaded plugin.</summary>
    public AnimationListEntryViewModel(FrameGeneratorEntry generatorEntry)
    {
        AnimationType = AnimationListEntryType.PluginAnimation;
        GeneratorEntry = generatorEntry;
        Name = generatorEntry.Info.Name;
        Description = generatorEntry.Info.Description;
        Author = null;
        FilePath = string.Empty;
        FileName = string.Empty;
    }
}
