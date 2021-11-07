namespace LedCube.Core.Common.Model
{
    public class Cube<TLed> where TLed : ILed<T>
    {
        public int SizeX { get; }
        public int SizeY { get; }
        public int SizeZ { get; }
        public TLed<T>[] Leds { get; set; }
    }

    public interface ICube<TLed<T>>
    {
        int SizeX { get; }
        int SizeY { get; }
        int SizeZ { get; }
        TLed<T>[] Leds { get; set; }
        
        TLed<T> GetLed(int x, int y, int z)
        {
            
        }
        
        IPLane<TLed<T>> GetPlane(int z)
        {
            
        }
    }

    public interface IPLane<TLed> where TLed : ILed
    {
        int SizeX { get; }
        int SizeY { get; }
        TLed[] Leds { get; set; }

        TLed GetLed(int x, int y)
        {
            
        }

        IRow<TLed> GetRow(int y)
        {
            
        }
    }

    public interface IRow<TLed>
    {
        int SizeX { get; }
        TLed[] Leds { get; set; }
        TLed GetLed(int x)
        {
            
        }
    }
}