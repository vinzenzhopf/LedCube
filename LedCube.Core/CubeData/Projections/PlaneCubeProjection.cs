using System.Data.Common;
using LedCube.Core.Common.Model;

namespace LedCube.Core.CubeData.Projections;

public class PlaneCubeProjection : IPlaneData
{
    private int _z;
    public ICubeData Data { get; }
    public Point2D Size => ProjectPoint(Data.Size);

    public int Z
    {
        get => _z;
        set
        {
            _z = value;
            OnPlaneChanged(this);
        }
    }

    public event PlaneLedChangedArgs? LedChanged;
    public event PlaneChangedArgs? PlaneChanged;
    
    public PlaneCubeProjection(ICubeData data, int z = 0)
    {
        Data = data;
        Data.LedChanged += OnDataLedChangeTriggered;
        Data.CubeChanged += OnDataCubeChangeTriggered;
        Z = z;
    }
    
    private void OnDataLedChangeTriggered(Point3D p, bool value)
    {
        if (p.Z != Z) 
            return;
        OnLedChanged(ProjectPoint(p), value);
    }
    
    protected virtual void OnDataCubeChangeTriggered(ICubeData cubeData)
    {
        OnPlaneChanged(this);
    }
    
    protected virtual void OnPlaneChanged(IPlaneData plane)
    {
        PlaneChanged?.Invoke(plane);
    }

    protected virtual void OnLedChanged(Point2D p, bool value)
    {
        LedChanged?.Invoke(p, value);
    }
    
    public bool GetLed(Point2D p)
    {
        return Data.GetLed(ProjectPoint(p));
    }

    public void SetLed(Point2D p, bool value)
    {
        Data.SetLed(ProjectPoint(p), value);
    }

    private Point2D ProjectPoint(Point3D p)
    {
        return new Point2D(p.X, p.Y);
    }
    
    private Point3D ProjectPoint(Point2D p)
    {
        return new Point3D(p.X, p.Y, Z);
    }
}