using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LedCube.Core.UI.Converters;

public sealed class VisibleWhenIntConverter : ConverterExtensionBase, IValueConverter
{
    public bool TrueValue { get; set; } = true;
    public bool FalseValue { get; set; } = false;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var v = System.Convert.ToInt32(value);
        var p = System.Convert.ToInt32(parameter);
        return v == p ? TrueValue : FalseValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
