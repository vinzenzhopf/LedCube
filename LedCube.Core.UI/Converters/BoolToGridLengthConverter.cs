using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace LedCube.Core.UI.Converters;

/// <summary>Maps a boolean to a <see cref="GridLength"/> — useful for collapsing grid rows/columns on/off.</summary>
public sealed class BoolToGridLengthConverter : ConverterExtensionBase, IValueConverter
{
    public GridLength TrueValue { get; set; } = new(1, GridUnitType.Star);
    public GridLength FalseValue { get; set; } = new(0);

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? TrueValue : FalseValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
