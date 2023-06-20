using System;

namespace LedCube.Core.Common.Model.Cube;

public class PwmLed : ILed<double>
{
    public double Value { get; set; }
    public static ILed<double> Create()
    {
        throw new NotImplementedException();
    }
}