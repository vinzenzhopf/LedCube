using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Animation.FileFormat.AnimationRaw.Io;
using LedCube.Core.UI.Controls.AnimationList;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginBase;
using Material.Icons;

namespace LedCube.Core.UI.Controls.PlaylistControl;

public partial class PlaylistEntryControlViewModel : ObservableObject
{
    public PlaylistEntry? Entry { get; }

    [ObservableProperty]
    private AnimationViewModel _animation;

    [ObservableProperty]
    private int _index = 0;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private EntryDisplayState _displayState;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSecondaryName))]
    private string _instanceName = string.Empty;

    /// <summary>Animation name — the plugin name, or the linked file's manifest name for file entries.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSecondaryName))]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(HasFrameCount))]
    [NotifyPropertyChangedFor(nameof(HasTiming))]
    private int _frameCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FpsText))]
    [NotifyPropertyChangedFor(nameof(DurationText))]
    [NotifyPropertyChangedFor(nameof(HasFps))]
    [NotifyPropertyChangedFor(nameof(HasTiming))]
    private TimeSpan _frameTime = TimeSpan.Zero;

    /// <summary>Thumbnail from the linked animation file; null falls back to a type icon in the UI.</summary>
    [ObservableProperty]
    private Bitmap? _thumbnail;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TypeIcon))]
    private AnimationListEntryType _animationType = AnimationListEntryType.PluginAnimation;

    [ObservableProperty]
    private int _repeatCount = 1;

    [ObservableProperty]
    private TimeSpan? _frameTimeOverride;

    public AnimationConfig Config { get; }

    /// <summary>Same icons as the library list: plugin = MovieCogOutline, file = FileStar.</summary>
    public MaterialIconKind TypeIcon => AnimationType == AnimationListEntryType.FileAnimation
        ? MaterialIconKind.FileStar
        : MaterialIconKind.MovieCogOutline;

    /// <summary>Show the animation name as a second line only when it differs from the instance label.</summary>
    public bool ShowSecondaryName => !string.IsNullOrEmpty(Name)
        && !string.Equals(Name, InstanceName, StringComparison.Ordinal);

    /// <summary>Frame rate is known (most plugins expose a frame time; file animations carry one).</summary>
    public bool HasFps => Fps > 0;

    /// <summary>Total frame count is known — file animations only; procedural plugins are open-ended.</summary>
    public bool HasFrameCount => FrameCount > 0;

    /// <summary>Any timing info to show (fps and/or frame count).</summary>
    public bool HasTiming => HasFps || HasFrameCount;

    public double Fps => FrameTime.TotalSeconds > 0 ? 1.0 / FrameTime.TotalSeconds : 0;

    public string FpsText => Fps > 0 ? $"{Fps:F1} fps" : string.Empty;

    public string DurationText => FrameCount > 0 && FrameTime > TimeSpan.Zero
        ? TimeSpan.FromTicks((long)FrameCount * FrameTime.Ticks).ToString(@"hh\:mm\:ss\.fff")
        : string.Empty;

    public PlaylistEntryControlViewModel(PlaylistEntry entry)
    {
        Entry = entry;
        _instanceName = entry.InstanceName;
        _repeatCount = entry.RepeatCount;
        _frameTimeOverride = entry.FrameTimeOverride;
        Config = entry.Config;
        _name = entry.Info.Name;
        _description = entry.Info.Description;
        _animationType = IsFileAnimation(entry.Info)
            ? AnimationListEntryType.FileAnimation
            : AnimationListEntryType.PluginAnimation;
        _animation = new AnimationViewModel
        {
            Name = entry.Info.Name,
            Description = entry.Info.Description,
            TypeInfo = entry.TypeInfo,
            GeneratorInfo = entry.Info,
        };
    }

    public PlaylistEntryControlViewModel(AnimationViewModel animation)
    {
        _animation = animation;
        _name = animation.Name;
        _description = animation.Description;
        var descriptors = animation.GeneratorInfo?.ConfigDescriptors;
        Config = descriptors is not null
            ? AnimationConfig.FromDescriptors(descriptors)
            : new AnimationConfig();
    }

    public PlaylistEntryControlViewModel(PlaylistEntryControlViewModel other)
    {
        Entry = other.Entry;
        _animation = other.Animation;
        _index = other.Index;
        _isSelected = other.IsSelected;
        _instanceName = other.InstanceName;
        _name = other.Name;
        _description = other.Description;
        _frameCount = other.FrameCount;
        _frameTime = other.FrameTime;
        _thumbnail = other.Thumbnail;
        _animationType = other.AnimationType;
        _repeatCount = other.RepeatCount;
        _frameTimeOverride = other.FrameTimeOverride;
        Config = new AnimationConfig(other.Config);
    }

    private static bool IsFileAnimation(FrameGeneratorInfo info) =>
        info.ConfigDescriptors?.Any(d => d.Type == AnimationConfigType.FilePath) == true;

    /// <summary>
    /// Populates display details. File animations read name/description/frame stats/thumbnail from
    /// the linked <c>.lcanimraw</c> manifest; other plugins are instantiated and configured to read
    /// their frame time / frame count (mirrors how PlaybackService resolves them).
    /// </summary>
    public async Task LoadDetailsAsync(CubeInfo cube)
    {
        if (Entry is null) return;

        var fileDescriptor = Entry.Info.ConfigDescriptors?.FirstOrDefault(d => d.Type == AnimationConfigType.FilePath);
        if (fileDescriptor is not null)
        {
            await LoadFileDetailsAsync(fileDescriptor.Key).ConfigureAwait(true);
            return;
        }

        LoadPluginTiming(cube);
    }

    private async Task LoadFileDetailsAsync(string filePathKey)
    {
        var path = Config.GetString(filePathKey);
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        try
        {
            var (manifest, thumbBytes) = await Task.Run(() =>
            {
                using var stream = File.OpenRead(path);
                var m = LcAnimRawReader.ReadManifest(stream);
                stream.Position = 0;
                var t = LcAnimRawReader.ReadThumbnail(stream);
                return (m, t);
            }).ConfigureAwait(true);

            Bitmap? bitmap = null;
            if (thumbBytes is { Length: > 0 })
            {
                using var ms = new MemoryStream(thumbBytes);
                bitmap = new Bitmap(ms);
            }

            var frameTime = Entry!.FrameTimeOverride ?? TimeSpan.FromMicroseconds(manifest.FrameTimeUs);

            void Apply()
            {
                Name = manifest.Name;
                Description = manifest.Description ?? string.Empty;
                FrameCount = manifest.FrameCount;
                FrameTime = frameTime;
                Thumbnail = bitmap;
            }

            RunOnUi(Apply);
        }
        catch
        {
            // A missing or corrupt file must not break the row — keep the generic name.
        }
    }

    // Resolves plugin timing purely from declarative metadata + config — no instantiation.
    // FrameTime:  override -> Info.FrameTime -> Info.EstimateFrameTime(cube, config).
    // FrameCount: Info.FrameCount -> Info.EstimateFrameCount(cube, config) -> DurationConfig-derived.
    private void LoadPluginTiming(CubeInfo cube)
    {
        var info = Entry!.Info;

        var frameTime = Entry.FrameTimeOverride
            ?? info.FrameTime
            ?? info.EstimateFrameTime?.Invoke(cube, Config);

        var frameCount = info.FrameCount
            ?? info.EstimateFrameCount?.Invoke(cube, Config);

        // Generic DurationConfig convention: a configured run time gives the frame count for free.
        if (frameCount is null && frameTime is { } ft && ft > TimeSpan.Zero
            && Config.Get<float>(DurationConfig.Key) is { } seconds && seconds > 0f)
        {
            frameCount = (int)Math.Round(seconds * 1000.0 / ft.TotalMilliseconds);
        }

        if (frameTime is { } t) FrameTime = t;
        if (frameCount is { } c) FrameCount = c;
    }

    private static void RunOnUi(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            Dispatcher.UIThread.Post(action);
    }
}
