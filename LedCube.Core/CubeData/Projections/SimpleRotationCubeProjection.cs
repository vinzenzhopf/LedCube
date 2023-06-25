using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.CubeData.Projections;

public class SimpleRotationCubeProjection : ICubeData
{
    public ICubeData Data { get; }
    public Orientation3D Rotation { get; }
    public Point3D Size => ProjectSize(Data.Size);
    
    public event LedChangedArgs? LedChanged;
    public event CubeChangedArgs? CubeChanged
    {
        add => Data.CubeChanged += value;
        remove => Data.CubeChanged -= value;
    }
    
    public SimpleRotationCubeProjection(ICubeData cubeData, Orientation3D rotation)
    {
        Data = cubeData;
        Data.LedChanged += OnDataLedChangeTriggered;
        Rotation = rotation;
    }

    private void OnDataLedChangeTriggered(Point3D p, bool value)
    {
        OnLedChanged(ProjectPoint(p), value);
    }
    
    protected virtual void OnLedChanged(Point3D p, bool value)
    {
        LedChanged?.Invoke(p, value);
    }

    public bool GetLed(Point3D p)
    {
        return Data.GetLed(ProjectPoint(p));
    }

    public void SetLed(Point3D p, bool value)
    {
        Data.SetLed(ProjectPoint(p), value);
    }

    private Point3D ProjectPoint(Point3D p)
    {
        var size = Data.Size;
        return Rotation switch
        {
            Orientation3D.Top => new Point3D(p.X, p.Y, p.Z),
            Orientation3D.Front => new Point3D(p.X, p.Z, p.Y),
            Orientation3D.Right => new Point3D(p.Z, p.Y, p.X),
            Orientation3D.Bottom => new Point3D(size.X-1- p.X, size.Y-1- p.Y, size.Z-1-p.Z),
            Orientation3D.Back => new Point3D(size.X-1- p.X, size.Z-1- p.Z, size.Y-1- p.Y),
            Orientation3D.Left => new Point3D(size.Z-1- p.Z, size.Y-1- p.Y, size.X-1- p.X),
            _ => throw new ArgumentException("Undefined Orientation", nameof(Rotation))
        };
    }
    
    private Point3D ProjectSize(Point3D p)
    {
        switch(Rotation)
        {
            case Orientation3D.Top: 
            case Orientation3D.Bottom:
                return new Point3D(p.X, p.Y, p.Z);
            case Orientation3D.Front:
            case Orientation3D.Back:
                return new Point3D(p.X, p.Z, p.Y);
            case Orientation3D.Right:
            case Orientation3D.Left:
                return new Point3D(p.Z, p.Y, p.X);
            default:
                throw new ArgumentException("Undefined Orientation", nameof(Rotation));
        };
    }
}