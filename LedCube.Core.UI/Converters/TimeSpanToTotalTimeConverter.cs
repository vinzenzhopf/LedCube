using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LedCube.Core.UI.Converters;

public class TimeSpanToTotalTimeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
        {
            return parameter switch
            {
                "ms" => timeSpan.TotalMilliseconds,
                "us" => timeSpan.TotalMicroseconds,
                "ns" => timeSpan.TotalNanoseconds,
                _ => timeSpan.TotalSeconds
            };
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
