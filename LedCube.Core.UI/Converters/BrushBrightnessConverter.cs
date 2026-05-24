using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LedCube.Core.UI.Converters;

/// <summary>
/// Adjust the brightness of a Brush.
/// The brightness correction factor parameter must be between -1 and 1.
/// Negative values produce darker colors.
/// </summary>
public class BrushBrightnessConverter : ConverterExtensionBase, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handles both SolidColorBrush (mutable) and ImmutableSolidColorBrush (Brushes.Gray etc).
        if (value is not ISolidColorBrush brush)
            return new SolidColorBrush();

        var correctionFactor = System.Convert.ToSingle(parameter, CultureInfo.InvariantCulture);
        if (Math.Abs(correctionFactor) < 0.01f)
            return value;

        var red = (float)brush.Color.R;
        var green = (float)brush.Color.G;
        var blue = (float)brush.Color.B;

        if (correctionFactor < 0)
        {
            red *= correctionFactor + 1;
            green *= correctionFactor + 1;
            blue *= correctionFactor + 1;
        }
        else
        {
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        var color = Color.FromArgb(brush.Color.A, (byte)red, (byte)green, (byte)blue);
        return new SolidColorBrush(color);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
