namespace LedCube.Core.Common.Model.Cube;

public class Plane<TLed> : IPlane<TLed>
{
    public int SizeX { get; }
    public int SizeY { get; }
    
    public ILed<TLed>[] Leds { get; set; }
    
    public Plane(int sizeX, int sizeY)
    {
        SizeX = sizeX;
        SizeY = sizeY;
    }

    public ILed<TLed> GetLed(int x, int y, Orientation2D orientation3D)
    {
        throw new System.NotImplementedException();
    }

    public ILed<TLed> GetLed(int n, Orientation2D orientation3D)
    {
        throw new System.NotImplementedException();
    }

    public IRow<TLed> GetRow(int x, Orientation2D orientation3D)
    {
        throw new System.NotImplementedException();
    }
}