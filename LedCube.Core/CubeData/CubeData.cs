using System;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.CubeData;

public class CubeData : ICubeData
{
    private readonly bool[] _leds;
    
    public Point3D Size { get; }
    
    public event CubeChangedArgs? CubeChanged;
    public event LedChangedArgs? LedChanged;
    public int Count => Size.X * Size.Y * Size.Z;

    public CubeData(Point3D size)
    {
        Size = size;
        _leds = new bool[Count];
        for (var i = 0; i < Count; i++)
        {
            _leds[i] = false;
        }
    }

    public bool GetLedIndex(int index)
    {
        return _leds[index];
    }
    
    /// <summary>
    /// Sets the LED by the given index. Not part of ICubeData. Does not update LedChanged event!
    /// </summary>
    /// <param name="index">Index of the array.</param>
    /// <param name="value">The new Value</param>
    public void SetLedIndex(int index, bool value)
    {
        _leds[index] = value;
    }
    
    public bool GetLed(Point3D p)
    {
        if(!Point3D.CheckBounds(p, Point3D.Empty, Size))
            throw new ArgumentException("Point out of Range", nameof(p));
        return _leds[CoordinatesToIndex(p)];
    }

    public void SetLed(Point3D p, bool value)
    {
        if(!Point3D.CheckBounds(p, Point3D.Empty, Size))
            throw new ArgumentException("Point out of Range", nameof(p));
        var i = CoordinatesToIndex(p);
        if (_leds[i] == value) 
            return;
        _leds[i] = value;
        OnLedChanged(p, value);
    }

    private int CoordinatesToIndex(Point3D p) => 
        p.X + 
        p.Y * Size.X + 
        p.Z * Size.X * Size.Y;

    private Point3D IndexToCoordinates(int index) => new(
        index % Size.X,
        (index / Size.X) % Size.Y,
        (index / (Size.X * Size.Y)) % Size.Z
    );
    
    protected virtual void OnLedChanged(Point3D p, bool value)
    {
        LedChanged?.Invoke(p, value);
    }

    protected virtual void OnCubeChanged(ICubeData cubedata)
    {
        CubeChanged?.Invoke(cubedata);
    }
}