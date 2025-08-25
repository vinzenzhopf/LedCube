using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Core.UI.Controls.AnimationInstanceList;

public partial class AnimationInstanceViewModel : ObservableObject
{
    [ObservableProperty]
    private AnimationViewModel _animation;
    
    [ObservableProperty]
    private int _index = 0;
    
    [ObservableProperty]
    private bool _isSelected;
    
    [ObservableProperty]
    private string _instanceName = string.Empty;
    
    [ObservableProperty]
    private int _frameCount = 0;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameFrequency))]
    private TimeSpan _frameTime = TimeSpan.Zero;
    
    [ObservableProperty]
    private double _animationLength = Double.NaN;
    
    public double FrameFrequency => FrameTime.TotalSeconds;

    public AnimationInstanceViewModel(AnimationViewModel animation)
    {
        _animation = animation;
    }

    public AnimationInstanceViewModel(AnimationInstanceViewModel other)
    {
        _animation = other.Animation;
        _index = other.Index;
        _isSelected = other.IsSelected;
        _frameCount = other.FrameCount;
        _animationLength = other.AnimationLength;
    }
}