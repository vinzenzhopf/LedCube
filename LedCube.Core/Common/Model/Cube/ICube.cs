namespace LedCube.Core.Common.Model.Cube;

public interface ICube<TLed>
{
    int SizeX { get; }
    int SizeY { get; }
    int SizeZ { get; }
    int Count { get; }
    
    ILed<TLed>[] Leds { get; set; }
        
    ILed<TLed> GetLed(int x, int y, int z, Orientation3D orientation3D);
    ILed<TLed> GetLed(int n, Orientation3D orientation3D);
    ILed<TLed> GetLed(int x, int y, int z)
        => GetLed(x, y, z, Orientation3D.Front);
    ILed<TLed> GetLed(int n)
        => GetLed(n, Orientation3D.Front); 
        
        
    IRow<TLed> GetRow(int x, int y, Orientation3D orientation3D);
    IRow<TLed> GetRow(int n, Orientation3D orientation3D);
    IRow<TLed> GetRow(int x, int y)
        => GetRow(x, y, Orientation3D.Front);
    IRow<TLed> GetRow(int n) => GetRow(n, Orientation3D.Front);
        
        
    IPlane<TLed> GetPlane(int z, Orientation3D orientation3D);
    IPlane<TLed> GetPlane(int z) => 
        GetPlane(z, Orientation3D.Front);
    
    static abstract ICube<TLed> Create(int sizeX, int sizeY, int sizeZ);
}