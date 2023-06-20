namespace LedCube.Core.Common.Model.Cube;

public interface IRow<TLed>
{
    int SizeX { get; }
    ILed<TLed>[] Leds { get; set; }
        
    ILed<TLed> GetLed(int x, Orientation1D orientation3D);
    ILed<TLed> GetLed(int x)
        => GetLed(x, Orientation1D.Right);
}