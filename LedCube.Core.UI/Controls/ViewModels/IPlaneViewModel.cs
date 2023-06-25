using System;
using System.CodeDom;

namespace LedCube.Core.UI.Controls.ViewModels;

public interface ILedViewModel
{
    public delegate void LedChangedArgs(int index, bool? value); 
    public event LedChangedArgs? LedChanged;
}

public interface IPlaneViewModel : ILedViewModel
{
    public delegate void PlaneChangedArgs(IPlaneData data); 
    public event PlaneChangedArgs? PlaneChanged;
}

public interface ICubeViewModel : IPlaneViewModel
{
    public delegate void CubeChangedArgs(ICubeData data); 
    public event CubeChangedArgs? CubeChanged;
}

public interface ICubeData<T>
{
    int SizeX { get; }
    int SizeY { get; }
    int SizeZ { get; }

    ILed<T> GetLed(int x, int y, int z);
    ILed<T> GetLed(int i);
    void SetLed(int x, int y, int z, T value);
    void SetLed(int i, T value);
}

public interface ICubeData : ICubeData<object>
{
}

public interface IPlaneData<T>
{
    int SizeX { get; }
    int SizeY { get; }

    ILed<T> GetLed(int x, int y);
    bool? GetLed(int i);
    void SetLed(int x, int y, T value);
    void SetLed(int i, T value);
}

public interface IPlaneData : IPlaneData<object>
{
}

public interface ILed<out T>
{
    Type LedType => typeof(T);
    T Value { get; }

    public static ILed<TLed> AsTypedLed<TLed>(ILed led)
    {
        if (led.LedType != typeof(TLed))
        {
            throw new ArgumentException(nameof(led));
        }
        return (ILed<TLed>) led;
    }
}

public interface ILed : ILed<object>
{
}