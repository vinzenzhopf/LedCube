namespace WpfTimelineControl.Element;

public sealed class MouseDragEndEventArgs : MouseDragStartEventArgs
{
    public MouseDragEndEventArgs(int startValue, int endValue) : base(startValue)
    {
        EndValue = endValue;
    }
    
    public int EndValue { get; }
}