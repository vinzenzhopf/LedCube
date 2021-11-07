namespace LedCube.Core.Common.Model
{
    public interface ILed<T>
    {
        public T Value { get; set; }
    }
}