﻿namespace LedCube.Core.Common.Model.Cube;

public class CubeData : ICubeDataBuffer
{
    public Point3D Size { get; }
    public bool[] Buffer { get; }
    
    public event CubeChangedArgs? CubeChanged;
    public event LedChangedArgs? LedChanged;
    public int Length => Size.X * Size.Y * Size.Z;

    public CubeData(Point3D size)
    {
        Size = size;
        Buffer = new bool[Length];
        for (var i = 0; i < Length; i++)
        {
            Buffer[i] = false;
        }
    }

    public bool GetLed(int index)
    {
        return Buffer[index];
    }
    
    /// <summary>
    /// Sets the LED by the given index. Not part of ICubeData. Does not update LedChanged event!
    /// </summary>
    /// <param name="index">Index of the array.</param>
    /// <param name="value">The new Value</param>
    public void SetLed(int index, bool value)
    {
        Buffer[index] = value;
    }
    
    public bool GetLed(Point3D p)
    {
        if(!Point3D.CheckBounds(p, Point3D.Empty, Size))
            throw new ArgumentException("Point out of Range", nameof(p));
        return Buffer[CoordinatesToIndex(p)];
    }

    public void SetLed(Point3D p, bool value)
    {
        if(!Point3D.CheckBounds(p, Point3D.Empty, Size))
            throw new ArgumentException("Point out of Range", nameof(p));
        var i = CoordinatesToIndex(p);
        if (Buffer[i] == value) 
            return;
        Buffer[i] = value;
        OnLedChanged(p, value);
    }

    public void Clear()
    {
        for (var i = 0; i < Length; i++)
        {
            Buffer[i] = false;
        }
        OnCubeChanged(this);
    }

    protected virtual void OnLedChanged(Point3D p, bool value)
    {
        LedChanged?.Invoke(p, value);
    }

    protected virtual void OnCubeChanged(ICubeData cubedata)
    {
        CubeChanged?.Invoke(cubedata);
    }
    
    public int CoordinatesToIndex(Point3D p) => 
        p.X + 
        p.Y * Size.X + 
        p.Z * Size.X * Size.Y;

    public Point3D IndexToCoordinates(int index) => new(
        index % Size.X,
        (index / Size.X) % Size.Y,
        (index / (Size.X * Size.Y)) % Size.Z
    );
}
