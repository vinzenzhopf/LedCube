using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Event;

namespace LedCube.Core.Common.CubeData.Projections;

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
            OnPlaneChanged(this, EventArgs.Empty);
        }
    }

    public event LedChangedEventHandler<Point2D>? LedChanged;
    public event PlaneChangedEventHandler? PlaneChanged;
    
    public PlaneCubeProjection(ICubeData data, int z = 0)
    {
        Data = data;
        Data.LedChanged += OnDataLedChangeTriggered;
        Data.CubeChanged += OnDataCubeChangeTriggered;
        Z = z;
    }
    
    private void OnDataLedChangeTriggered(object? sender, LegChangedEventArgs<Point3D> args)
    {
        if (args.Position.Z != Z)
        {
            return;
        }
        OnLedChanged(sender, new(ProjectPoint(args.Position), args.Value));
    }
    
    protected virtual void OnDataCubeChangeTriggered(object? sender, EventArgs args)
    {
        OnPlaneChanged(this, args);
    }
    
    protected virtual void OnPlaneChanged(object? sender, EventArgs args)
    {
        PlaneChanged?.Invoke(sender, args);
    }

    protected virtual void OnLedChanged(object? sender, LegChangedEventArgs<Point2D> args)
    {
        LedChanged?.Invoke(sender, args);
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