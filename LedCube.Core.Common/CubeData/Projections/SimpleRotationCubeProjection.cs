using System.Diagnostics;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Event;

namespace LedCube.Core.Common.CubeData.Projections;

public sealed class SimpleRotationCubeProjection : ICubeData, IDisposable
{
    private Orientation3D _rotation;
    public ICubeData Data { get; }

    public Orientation3D Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            OnCubeChanged(this, EventArgs.Empty);
        }
    }
    public void Serialize(Span<byte> target) => Data.Serialize(target);
    public Point3D Size => ProjectSize(Data.Size);
    public int Length => Data.Length;
    public event LedChangedEventHandler<Point3D>? LedChanged;
    public event CubeChangedEventHandler? CubeChanged;
    
    public SimpleRotationCubeProjection(ICubeData cubeData, Orientation3D rotation)
    {
        Data = cubeData;
        Data.LedChanged += OnDataLedChangeTriggered;
        Data.CubeChanged += OnDataCubeChangeTriggered;
        Rotation = rotation;
    }
    
    

    private void OnDataLedChangeTriggered(object? sender, LegChangedEventArgs<Point3D> args)
    {
        OnLedChanged(sender, new(ProjectBackPoint(args.Position), args.Value));
    }

    private void OnDataCubeChangeTriggered(object? sender, EventArgs args)
    {
        OnCubeChanged(sender, args);
    }
    
    private void OnLedChanged(object? sender, LegChangedEventArgs<Point3D> args)
    {
        LedChanged?.Invoke(sender, args);
    }
    
    private void OnCubeChanged(object? sender, EventArgs args)
    {
        CubeChanged?.Invoke(sender, args);
    }

    public void Dispose()
    {
        Data.CubeChanged -= OnDataCubeChangeTriggered;
        Data.LedChanged -= OnDataLedChangeTriggered;
    }

    public bool GetLed(Point3D p)
    {
        return Data.GetLed(ProjectPoint(p));
    }

    public void SetLed(Point3D p, bool value)
    {
        Data.SetLed(ProjectPoint(p), value);
    }

    public void Clear()
    {
        Data.Clear();
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
        var max = Size - 1;
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

    private Point3D ProjectSize(Point3D p)
    {
        switch(Rotation)
        {
            case Orientation3D.Top: 
            case Orientation3D.Bottom:
                return new Point3D(p.X, p.Z, p.Y);
            case Orientation3D.Front:
            case Orientation3D.Back:
                return new Point3D(p.X, p.Y, p.Z);
            case Orientation3D.Right:
            case Orientation3D.Left:
                return new Point3D(p.Y, p.X, p.Z);
            default:
                throw new UnreachableException($"Undefined Orientation: {Rotation}");
        };
    }
}