namespace LedCube.Core.UI.TimelineControl;

public class PointMarker : MarkerBase
{
    public int Frame { get; set; }

    internal override bool ApplyFrameLimit(int newTotalFrames)
    {
        if (Frame < newTotalFrames)
            return true;
        if (ClampBehavior == ClampBehavior.Drop)
            return false;
        Frame = newTotalFrames - 1;
        return true;
    }
}
