using System;

namespace WpfTimelineControl.Element;

public class MouseDragStartEventArgs : EventArgs
{
    public MouseDragStartEventArgs(int startValue)
    {
        StartValue = startValue;
    }
    
    public virtual int StartValue { get; }
}