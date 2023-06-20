using System;

namespace LedCube.Core.Common.Model.Cube;

public class Row<TLed> : IRow<TLed>
{
    public int SizeX { get; }
    public ILed<TLed>[] Leds { get; set; }

    public Row(int sizeX)
    {
        SizeX = sizeX;
        Leds = new ILed<TLed>[SizeX];
    }

    public ILed<TLed> GetLed(int x, Orientation1D orientation3D)
    {
        throw new System.NotImplementedException();
    }
}