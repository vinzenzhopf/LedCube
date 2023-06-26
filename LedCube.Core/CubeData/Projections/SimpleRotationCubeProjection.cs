using System;
using System.Drawing;
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
        OnLedChanged(ProjectBackPoint(p), value);
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

    private Point3D ProjectBackPoint(Point3D p)
    {
        var max = Data.Size-1;
        return Rotation switch
        {
            Orientation3D.Front => new Point3D(p.X, p.Y, p.Z),
            Orientation3D.Left => new Point3D(max.Y-p.Y, p.X, p.Z),
            Orientation3D.Back => new Point3D(max.X-p.X, max.Y-p.Y, p.Z),
            Orientation3D.Right => new Point3D(p.Y, max.X-p.X, p.Z),
            Orientation3D.Top => new Point3D(p.X, max.Z-p.Z, p.Y),
            Orientation3D.Bottom => new Point3D(p.X, p.Z, max.Y-p.Y),
            _ => throw new ArgumentException("Unsupported orientation.")
        };
    }
    
    private Point3D ProjectPoint(Point3D p)
    {
        var max = Data.Size - 1;
        var p2 = Rotation switch
        {
            Orientation3D.Front => new Point3D(p.X, p.Y, p.Z),
            Orientation3D.Left => new Point3D(p.Y, max.X-p.X, p.Z),
            Orientation3D.Back => new Point3D(max.X-p.X, max.Y-p.Y, p.Z),
            Orientation3D.Right => new Point3D(max.Y-p.Y, p.X, p.Z),
            Orientation3D.Top => new Point3D(p.X, p.Z, max.Y-p.Y),
            Orientation3D.Bottom => new Point3D(p.X, max.Z-p.Z, p.Y),
            _ => throw new ArgumentException("Unsupported orientation.")
        };
        return p2;
    }

    
    // case Orientation3D.Front:
    // return new Point3D(p.X, p.Y, p.Z);
    //     case Orientation3D.Right:
    // return new Point3D(p.Z, p.Y, -p.X + Size.X - 1);
    // case Orientation3D.Top:
    // return new Point3D(p.X, -p.Z + Size.Z - 1, p.Y);
    // case Orientation3D.Back:
    // return new Point3D(-p.X + Size.X - 1, p.Y, -p.Z + Size.Z - 1);
    // case Orientation3D.Left:
    // return new Point3D(-p.Z + Size.Z - 1, p.Y, p.X);
    // case Orientation3D.Bottom:
    // return new Point3D(p.X, p.Z, p.Y);
    //     default:
    //     return p;
    
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