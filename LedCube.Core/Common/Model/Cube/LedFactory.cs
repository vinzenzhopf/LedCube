using System;
using System.Drawing;

namespace LedCube.Core.Common.Model.Cube;

public static class LedFactory
{
    public static ILed<TLed> CreateInstance<TLed>()
    {
        if (typeof(TLed) == typeof(bool))
            return (ILed<TLed>)new BiLed();
        if (typeof(TLed) == typeof(double))
            return (ILed<TLed>)new PwmLed();
        if(typeof(TLed) == typeof(Color))
            return (ILed<TLed>)new RgbLed();

        throw new NotSupportedException($"Creation of {typeof(TLed).Name} instances is not supported.");
    }
}