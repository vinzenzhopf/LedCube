using CommunityToolkit.Mvvm.ComponentModel;

namespace LedCube.Animator.Controls.Timeline;

public partial class TimelineViewModel : ObservableObject
{
    private double _length;
    private double _position;
    private double? _altPosition;
    
    public bool BlockSelection => _altPosition is not null;
    public double? SelectedRange => _altPosition is null ? 0 : _altPosition - _position;
}