using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace LedCube.Core.UI.Converters;

/// <summary>Shows a placeholder (default "-") when the bound value is null, empty, or whitespace.</summary>
public sealed class EmptyToDashConverter : ConverterExtensionBase, IValueConverter
{
    public string Placeholder { get; set; } = "-";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value?.ToString();
        return string.IsNullOrWhiteSpace(text) ? Placeholder : text;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
