namespace LedCube.Core.Common.Model.Cube;

public interface IPlane<TLed>
{
    int SizeX { get; }
    int SizeY { get; }
    ILed<TLed>[] Leds { get; set; }

    ILed<TLed> GetLed(int x, int y, Orientation2D orientation3D);
    ILed<TLed> GetLed(int n, Orientation2D orientation3D);
    ILed<TLed> GetLed(int x, int y)
        => GetLed(x, y, Orientation2D.Top);
    ILed<TLed> GetLed(int n)
        => GetLed(n, Orientation2D.Top); 
        
        
    IRow<TLed> GetRow(int x, Orientation2D orientation3D);
    IRow<TLed> GetRow(int x) => GetRow(x, Orientation2D.Top);        
}