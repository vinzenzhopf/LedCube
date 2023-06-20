using LedCube.Core.Common.Model.Cube;

namespace LedCube.Core.Common.Model
{
    public class Cube<TLed>
    {
        public int SizeX { get; }
        public int SizeY { get; }
        public int SizeZ { get; }
        //public IPlane<TLed>[] Planes { get; set; }
    }

    public interface ICube<TLed>
    {
        int SizeX { get; }
        int SizeY { get; }
        int SizeZ { get; }
        ILed<TLed>[] Leds { get; set; }
        
        // ILed<TLed> GetLed(int x, int y, int z)
        // {
        //     
        // }
        //
        // IPLane<TLed> GetPlane(int z)
        // {
        //     
        // }
    }

    public interface IPLane<TLed>
    {
        int SizeX { get; }
        int SizeY { get; }
        ILed<TLed>[] Leds { get; set; }

        // ILed<TLed> GetLed(int x, int y)
        // {
        //     
        // }
        //
        // IRow<TLed> GetRow(int y)
        // {
        //     
        // }
    }

    public interface IRow<TLed>
    {
        int SizeX { get; }
        ILed<TLed>[] Leds { get; set; }
        // ILed<TLed> GetLed(int x)
        // {
        //     
        // }
    }
}