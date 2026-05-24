using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LedCube.Core.UI.Converters;

/// <summary>Convert between boolean and IsVisible (bool).</summary>
public sealed class BooleanToVisibilityConverterEx : IValueConverter
{
    public bool TrueValue { get; set; } = true;
    public bool FalseValue { get; set; } = false;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        return boolValue ? TrueValue : FalseValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool visible)
            return visible == TrueValue;
        return false;
    }
}
