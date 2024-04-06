using System.Numerics;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Event;

namespace LedCube.Core.Common.CubeData.Projections;

public class MatrixCubeProjection : ICubeData
{
    public Matrix4x4 RotationMatrix { get; }
    public Point3D ProjectionSize { get; }
    public ICubeData Data { get; }
    public Point3D Size => ProjectPoint(Data.Size);
    public int Length => Data.Length;

    public event LedChangedEventHandler<Point3D>? LedChanged;
    public void Serialize(Span<byte> target) => Data.Serialize(target);

    public event CubeChangedEventHandler? CubeChanged
    {
        add => Data.CubeChanged += value;
        remove => Data.CubeChanged -= value;
    }

    public MatrixCubeProjection(ICubeData cubeData, Matrix4x4 rotationMatrix)
    {
        Data = cubeData;
        Data.LedChanged += OnDataLedChangeTriggered;
        RotationMatrix = rotationMatrix;
        ProjectionSize = Size;
    }
    
    public MatrixCubeProjection(ICubeData cubeData, Orientation3D rotation) :
        this(cubeData, CreateRotationMatrix(rotation))
    {
    }
    
    public MatrixCubeProjection(ICubeData cubeData, float angleX, float angleY, float angleZ) : 
        this(cubeData, CreateRotationMatrix(angleX, angleY, angleZ))
    {
    }
    
    public MatrixCubeProjection(ICubeData cubeData, Point3D projectionSize, Matrix4x4 rotationMatrix) :
        this(cubeData, rotationMatrix)
    {
        ProjectionSize = projectionSize;
    }
    
    public MatrixCubeProjection(ICubeData cubeData, Point3D projectionSize, Orientation3D rotation) :
        this(cubeData, projectionSize, CreateRotationMatrix(rotation))
    {
    }

    public MatrixCubeProjection(ICubeData cubeData, Point3D projectionSize, float angleX, float angleY, float angleZ) : 
        this(cubeData, projectionSize, CreateRotationMatrix(angleX, angleY, angleZ))
    {
    }

    private void OnDataLedChangeTriggered(object? sender, LegChangedEventArgs<Point3D> args)
    {
        OnLedChanged(sender, new(ProjectPoint(args.Position), args.Value));   
    }

    protected virtual void OnLedChanged(object? sender, LegChangedEventArgs<Point3D> args)
    {
        LedChanged?.Invoke(sender, args);
    }
    
    public bool GetLed(Point3D p) => Data.GetLed(ProjectPoint(p));

    public void SetLed(Point3D p, bool value) => Data.SetLed(ProjectPoint(p), value);

    public void Clear() => Data.Clear();

    private Point3D ProjectPoint(Point3D p)
    {
        // Apply rotation transformation
        var rotatedPoint = Vector3.Transform(p, RotationMatrix);

        // Perform projection based on the rotated coordinates and projection size
        var scaleX = ProjectionSize.X / (float)Data.Size.X;
        var scaleY = ProjectionSize.Y / (float)Data.Size.Y;
        var scaleZ = ProjectionSize.Z / (float)Data.Size.Z;

        var projectedX = (int)(rotatedPoint[0] * scaleX);
        var projectedY = (int)(rotatedPoint[1] * scaleY);
        var projectedZ = (int)(rotatedPoint[2] * scaleZ);

        return new Point3D(projectedX, projectedY, projectedZ);
    }
    
    public static Matrix4x4 CreateRotationMatrix(Orientation3D rotation)
    {
        return rotation switch
        {
            Orientation3D.Right => Matrix4x4.CreateRotationY(MathF.PI / 2), // Rotate around Y-axis by 90 degrees
            Orientation3D.Top => Matrix4x4.CreateRotationX(-MathF.PI / 2), // Rotate around X-axis by -90 degrees
            Orientation3D.Back => Matrix4x4.CreateRotationY(MathF.PI), // Rotate around Y-axis by 180 degrees
            Orientation3D.Left => Matrix4x4.CreateRotationY(-MathF.PI / 2), // Rotate around Y-axis by -90 degrees
            Orientation3D.Bottom => Matrix4x4.CreateRotationX(MathF.PI / 2), // Rotate around X-axis by 90 degrees
            _ => Matrix4x4.Identity
        };
    }

    private static Matrix4x4 CreateRotationMatrix(float angleX, float angleY, float angleZ)
    {
        var rotationX = Matrix4x4.CreateRotationX(angleX);
        var rotationY = Matrix4x4.CreateRotationY(angleY);
        var rotationZ = Matrix4x4.CreateRotationZ(angleZ);
        return  Matrix4x4.Multiply(rotationZ, Matrix4x4.Multiply(rotationX, rotationY));
    }
}