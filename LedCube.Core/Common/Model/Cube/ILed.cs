namespace LedCube.Core.Common.Model.Cube
{
    public interface ILed<T>
    {
        public T Value { get; set; }
    }
}