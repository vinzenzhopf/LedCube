using System;
using System.Globalization;
using System.Windows.Data;

namespace LedCube.Core.UI.Converters;

[ValueConversion(typeof(TimeSpan), typeof(double))]
public class TimeSpanToTotalTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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
        else
        {
            return 0.0;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}