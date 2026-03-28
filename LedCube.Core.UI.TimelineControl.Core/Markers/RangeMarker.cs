namespace LedCube.Core.UI.TimelineControl;

public class RangeMarker : MarkerBase
{
    public int StartFrame { get; set; }
    public int EndFrame { get; set; }

    internal override bool ApplyFrameLimit(int newTotalFrames)
    {
        if (StartFrame >= newTotalFrames)
        {
            if (ClampBehavior == ClampBehavior.Drop)
                return false;
            StartFrame = newTotalFrames - 1;
            EndFrame = newTotalFrames - 1;
            return true;
        }
        if (EndFrame >= newTotalFrames)
        {
            if (ClampBehavior == ClampBehavior.Drop)
                return false;
            EndFrame = newTotalFrames - 1;
        }
        return true;
    }
}
