using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LedCube.Core.UI.Converters;

public class IntToDoubleConverter : ConverterExtensionBase, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int intValue)
            return 0.0;
        return (double)intValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double doubleValue)
            return 0;
        return (int)Math.Floor(doubleValue);
    }
}
