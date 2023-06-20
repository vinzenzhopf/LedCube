using System.Drawing;

namespace LedCube.Core.Common.Model.Cube;

public class Cube<TLed> : ICube<TLed>
{
    public Cube(int sizeX, int sizeY, int sizeZ)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        SizeZ = sizeZ;
        Count = sizeX * sizeY * sizeZ;
        Leds = new ILed<TLed>[Count];
        for (var i = 0; i < Count; i++)
        {
            Leds[i] = LedFactory.CreateInstance<TLed>();
        }
    }

    public int SizeX { get; }
    public int SizeY { get; }
    public int SizeZ { get; }
    public int Count { get; }
    public ILed<TLed>[] Leds { get; set; }

    public ILed<TLed> GetLed(int x, int y, int z, Orientation3D orientation3D)
    {
        throw new System.NotImplementedException();
    }

    public ILed<TLed> GetLed(int n, Orientation3D orientation3D)
    {
        throw new System.NotImplementedException();
    }

    public IRow<TLed> GetRow(int x, int y, Orientation3D orientation3D)
    {
        throw new System.NotImplementedException();
    }

    public IRow<TLed> GetRow(int n, Orientation3D orientation3D)
    {
        throw new System.NotImplementedException();
    }

    public IPlane<TLed> GetPlane(int z, Orientation3D orientation3D)
    {
        throw new System.NotImplementedException();
    }

    public static ICube<TLed> Create(int sizeX, int sizeY, int sizeZ)
    {
        return new Cube<TLed>(sizeX, sizeY, sizeZ);
    }
}