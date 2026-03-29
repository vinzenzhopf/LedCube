using System;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI.Controls.AnimationInstanceList;

public partial class AnimationViewModel : ObservableObject
{
    public const int DYMANIC_FRAME_COUNT = -1;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalTime))]
    private int _frameCount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameFrequency))]
    [NotifyPropertyChangedFor(nameof(TotalTime))]
    private TimeSpan _frameTime = TimeSpan.Zero;
    
    [ObservableProperty]
    private double _animationLength = Double.NaN;

    [ObservableProperty]
    private TypeInfo? _typeInfo;
    
    public double FrameFrequency => FrameTime.TotalSeconds;
    public TimeSpan TotalTime => TimeSpan.FromTicks((long)(FrameCount * FrameTime.Ticks));

    public AnimationViewModel()
    {
    }

    public static AnimationViewModel DefaultAnimation { get; } = new AnimationViewModel()
    {
        Name = "<empty>",
        Description = ""
    };
}