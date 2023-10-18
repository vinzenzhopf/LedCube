using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Streamer.UI.Controls.AnimationList;

[ObservableObject]
public partial class AnimationViewModel
{
    public const int DYMANIC_FRAME_COUNT = -1;

    [ObservableProperty]
    private int _index = 0;
    
    [ObservableProperty]
    private bool _isSelected;
    
    [ObservableProperty]
    private string _name = string.Empty;
    
    [ObservableProperty]
    private string _description = string.Empty;
    
    [ObservableProperty]
    private int _frameCount = 0;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameFrequency))]
    private TimeSpan _frameTime = TimeSpan.Zero;
    
    [ObservableProperty]
    private double _animationLength = Double.NaN;
    
    public double FrameFrequency => FrameTime.TotalSeconds;

    public AnimationViewModel()
    {
    }
}