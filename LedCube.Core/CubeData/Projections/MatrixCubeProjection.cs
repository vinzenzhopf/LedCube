using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using MathNet.Numerics.LinearAlgebra;

namespace LedCube.Core.CubeData.Projections;

public class MatrixCubeProjection : ICubeData
{
    public Matrix<double> RotationMatrix { get; }
    public Point3D ProjectionSize { get; }
    public ICubeData Data { get; }
    public Point3D Size => ProjectPoint(Data.Size);

    public event LedChangedArgs? LedChanged;
    public event CubeChangedArgs? CubeChanged
    {
        add => Data.CubeChanged += value;
        remove => Data.CubeChanged -= value;
    }

    public MatrixCubeProjection(ICubeData cubeData, Matrix<double> rotationMatrix)
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
    
    public MatrixCubeProjection(ICubeData cubeData, double angleX, double angleY, double angleZ) : 
        this(cubeData, CreateRotationMatrix(angleX, angleY, angleZ))
    {
    }
    
    public MatrixCubeProjection(ICubeData cubeData, Point3D projectionSize, Matrix<double> rotationMatrix) :
        this(cubeData, rotationMatrix)
    {
        ProjectionSize = projectionSize;
    }
    
    public MatrixCubeProjection(ICubeData cubeData, Point3D projectionSize, Orientation3D rotation) :
        this(cubeData, projectionSize, CreateRotationMatrix(rotation))
    {
    }

    public MatrixCubeProjection(ICubeData cubeData, Point3D projectionSize, double angleX, double angleY, double angleZ) : 
        this(cubeData, projectionSize, CreateRotationMatrix(angleX, angleY, angleZ))
    {
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
        // Apply rotation transformation
        var pointVector = Vector<double>.Build.DenseOfArray(new double[] { p.X, p.Y, p.Z, 1 });
        var rotatedPoint = RotationMatrix.Multiply(pointVector).SubVector(0, 3);

        // Perform projection based on the rotated coordinates and projection size
        var scaleX = ProjectionSize.X / (double)Data.Size.X;
        var scaleY = ProjectionSize.Y / (double)Data.Size.Y;
        var scaleZ = ProjectionSize.Z / (double)Data.Size.Z;

        var projectedX = (int)(rotatedPoint[0] * scaleX);
        var projectedY = (int)(rotatedPoint[1] * scaleY);
        var projectedZ = (int)(rotatedPoint[2] * scaleZ);

        return new Point3D(projectedX, projectedY, projectedZ);
    }
    
    public static Matrix<double> CreateRotationMatrix(Orientation3D rotation)
    {
        return rotation switch
        {
            Orientation3D.Right => CreateRotationMatrix(0, Math.PI / 2, 0), // Rotate around Y-axis by 90 degrees
            Orientation3D.Top => CreateRotationMatrix(-Math.PI / 2, 0, 0), // Rotate around X-axis by -90 degrees
            Orientation3D.Back => CreateRotationMatrix(0, Math.PI, 0), // Rotate around Y-axis by 180 degrees
            Orientation3D.Left => CreateRotationMatrix(0, -Math.PI / 2, 0), // Rotate around Y-axis by -90 degrees
            Orientation3D.Bottom => CreateRotationMatrix(Math.PI / 2, 0, 0), // Rotate around X-axis by 90 degrees
            _ => CreateIdentiyMatrix()
        };
    }

    public static Matrix<double> CreateIdentiyMatrix() => 
        Matrix<double>.Build.DenseIdentity(4, 4);

    public static Matrix<double> CreateRotationMatrix(double angleX, double angleY, double angleZ)
    {
        var rotationX = CreateRotationMatrixX(angleX);
        var rotationY = CreateRotationMatrixY(angleY);
        var rotationZ = CreateRotationMatrixZ(angleZ);
        return rotationZ.Multiply(rotationX).Multiply(rotationY);
    }

    public static Matrix<double> CreateRotationMatrixX(double angle)
    {
        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            {1, 0, 0, 0},
            {0, Math.Cos(angle), -Math.Sin(angle), 0},
            {0, Math.Sin(angle), Math.Cos(angle), 0},
            {0, 0, 0, 1}
        });
    }

    public static Matrix<double> CreateRotationMatrixY(double angle)
    {
        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            {Math.Cos(angle), 0, Math.Sin(angle), 0},
            {0, 1, 0, 0},
            {-Math.Sin(angle), 0, Math.Cos(angle), 0},
            {0, 0, 0, 1}
        });
    }

    public static Matrix<double> CreateRotationMatrixZ(double angle)
    {
        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            {Math.Cos(angle), -Math.Sin(angle), 0, 0},
            {Math.Sin(angle), Math.Cos(angle), 0, 0},
            {0, 0, 1, 0},
            {0, 0, 0, 1}
        });
    }
}