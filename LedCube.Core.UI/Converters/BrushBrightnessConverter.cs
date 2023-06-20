using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LedCube.Core.UI.Converters;

/// <summary>
///     Adjust the brightness of a Brush.
///     The brightness correction factor parameter must be between -1 and 1.
///     Negative values produce darker colors.
/// </summary>
[ValueConversion(typeof(Brush), typeof(Brush))]
public class BrushBrightnessConverter : ConverterExtensionBase, IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var brush = value as SolidColorBrush;
        if (brush == null)
        {
            return new SolidColorBrush();
        }

        var correctionFactor = System.Convert.ToSingle(parameter, CultureInfo.InvariantCulture);
        if (Math.Abs(correctionFactor) < 0.01)
        {
            return value;
        }

        var red = (float) brush.Color.R;
        var green = (float) brush.Color.G;
        var blue = (float) brush.Color.B;

        if (correctionFactor < 0)
        {
            // darker color
            red *= correctionFactor + 1;
            green *= correctionFactor + 1;
            blue *= correctionFactor + 1;
        }
        else
        {
            // lighter color
            red = (255 - red) * correctionFactor + red;
            green = (255 - green) * correctionFactor + green;
            blue = (255 - blue) * correctionFactor + blue;
        }

        var color = Color.FromArgb(brush.Color.A, (byte) red, (byte) green, (byte) blue);
        return new SolidColorBrush(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}