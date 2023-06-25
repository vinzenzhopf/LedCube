using LedCube.Core.Common.Model;

namespace LedCube.Core.CubeData;

public delegate void PlaneLedChangedArgs(Point2D p, bool value);
public delegate void PlaneChangedArgs(IPlaneData plane);

public interface IPlaneData
{
    public Point2D Size { get; }
    
    public event PlaneLedChangedArgs? LedChanged;
    
    public event PlaneChangedArgs? PlaneChanged;
    
    public bool GetLed(int index)
    {
        return GetLed(IndexToCoordinates(Size, index));
    }

    public bool GetLed(Point2D p);

    public void SetLed(int index, bool value)
    {
        SetLed(IndexToCoordinates(Size, index), value);
    }

    public void SetLed(Point2D p, bool value);
    
    protected static Point2D IndexToCoordinates(Point2D size, int index) => new(
        index % size.X,
        (index / size.X) % size.Y
    );

    protected static int CoordinatesToIndex(Point2D size, Point2D p) =>
        p.X + p.Y * size.Y;
}