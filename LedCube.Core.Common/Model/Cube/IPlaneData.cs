using LedCube.Core.Common.Model.Cube.Event;

namespace LedCube.Core.Common.Model.Cube;

public interface IPlaneData
{
    public Point2D Size { get; }
    
    public event LedChangedEventHandler<Point2D>? LedChanged;
    public event PlaneChangedEventHandler? PlaneChanged;

    public bool GetLed(Point2D p);

    public void SetLed(Point2D p, bool value);
}