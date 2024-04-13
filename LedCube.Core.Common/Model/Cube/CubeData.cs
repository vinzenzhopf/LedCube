using System;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.Common.Model.Cube.Event;

namespace LedCube.Core.Common.Model.Cube;

public sealed class CubeData<TBuffer> : ICubeData 
    where TBuffer : struct, ICubeDataBuffer<TBuffer>
{
    public Point3D Size => TBuffer.Size;
    public int Length => TBuffer.Length;

    private TBuffer _buffer;
    public event CubeChangedEventHandler? CubeChanged;
    public event LedChangedEventHandler<Point3D>? LedChanged;

    public CubeData()
    {
        _buffer = default;
    }
    
    public void Serialize(Span<byte> target)
    {
        if (target.Length < TBuffer.Length / 8)
        {
            throw new ArgumentException($"insufficient space for {TBuffer.Length} leds", nameof(target));
        }
        
        var buffer = TBuffer.GetReadOnlyBuffer(in _buffer);
        for (var i = 0; i < buffer.Length; i++)
        {
            var index = i / 8;
            var bit = i % 8;
            if (bit == 0)
            {
                target[index] = 0;
            }
            var led = (byte)(buffer[i] ? 1 : 0);
            target[index] |= (byte)(led << bit);
        }
    }

    public bool GetLed(Point3D p)
    {
        if(!Point3D.CheckBounds(p, Point3D.Empty, Size))
        {
            throw new ArgumentException("Point out of Range", nameof(p));
        }

        var index = TBuffer.CoordinatesToIndex(p);
        var buffer = TBuffer.GetReadOnlyBuffer(in _buffer);
        return buffer[index];
    }

    internal bool GetLed(int index)
    {
        var buffer = TBuffer.GetReadOnlyBuffer(in _buffer);
        return buffer[index];
    }
    
    public void SetLed(Point3D p, bool value)
    {
        if(!Point3D.CheckBounds(p, Point3D.Empty, Size))
        {
            throw new ArgumentException("Point out of Range", nameof(p));
        }

        var index = TBuffer.CoordinatesToIndex(p);
        var buffer = TBuffer.GetBuffer(ref _buffer);
        if (buffer[index] == value)
        {
            return;
        }

        buffer[index] = value;
        OnLedChanged(p, value);
        OnCubeChanged();
    }

    public void Clear()
    {
        var buffer = TBuffer.GetBuffer(ref _buffer);
        buffer.Clear();
        OnCubeChanged();
    }

    private void OnLedChanged(Point3D p, bool value) => LedChanged?.Invoke(this, new(p, value));

    private void OnCubeChanged() => CubeChanged?.Invoke(this, EventArgs.Empty);
}
