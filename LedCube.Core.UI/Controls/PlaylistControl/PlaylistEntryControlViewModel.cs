using System;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginBase;

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
    private string _instanceName = string.Empty;

    [ObservableProperty]
    private int _frameCount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameFrequency))]
    private TimeSpan _frameTime = TimeSpan.Zero;

    [ObservableProperty]
    private double _animationLength = Double.NaN;

    [ObservableProperty]
    private int _repeatCount = 1;

    [ObservableProperty]
    private TimeSpan? _frameTimeOverride;

    public AnimationConfig Config { get; }

    public double FrameFrequency => FrameTime.TotalSeconds;

    public PlaylistEntryControlViewModel(PlaylistEntry entry)
    {
        Entry = entry;
        _instanceName = entry.InstanceName;
        _repeatCount = entry.RepeatCount;
        _frameTimeOverride = entry.FrameTimeOverride;
        Config = entry.Config;
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
        _frameCount = other.FrameCount;
        _animationLength = other.AnimationLength;
        _repeatCount = other.RepeatCount;
        _frameTimeOverride = other.FrameTimeOverride;
        Config = new AnimationConfig(other.Config);
    }
}
