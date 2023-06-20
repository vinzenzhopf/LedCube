namespace LedCube.Core.Common.Model.Cube;

public interface ILed<TLed>
{
    public TLed Value { get; set; }
    
    // static abstract ILed<TLed> Create();
    // static ILed<TLed> Create<T>() where T : ILed<TLed> => T.Create();
}