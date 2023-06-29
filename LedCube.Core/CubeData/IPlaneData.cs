using LedCube.Core.Common.Model;

namespace LedCube.Core.CubeData;

public delegate void PlaneLedChangedArgs(Point2D p, bool value);
public delegate void PlaneChangedArgs(IPlaneData plane);

public interface IPlaneData
{
    public Point2D Size { get; }
    
    public event PlaneLedChangedArgs? LedChanged;
    
    public event PlaneChangedArgs? PlaneChanged;

    public bool GetLed(Point2D p);

    public void SetLed(Point2D p, bool value);
}