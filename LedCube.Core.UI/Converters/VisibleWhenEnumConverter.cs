using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LedCube.Core.UI.Converters;

[ValueConversion(typeof(int), typeof(Visibility))]
public sealed class VisibleWhenIntConverter : IValueConverter
{
    public Visibility TrueValue { get; set; }
    public Visibility FalseValue { get; set; }

    public VisibleWhenIntConverter()
    {
        // set defaults
        TrueValue = Visibility.Visible;
        FalseValue = Visibility.Collapsed;
    }

    public object Convert(object value, Type targetType, 
        object parameter, CultureInfo culture)
    {
        var v = System.Convert.ToInt32(value);
        var p = System.Convert.ToInt32(parameter);
        return (v == p) ? TrueValue : FalseValue;
    }

    public object ConvertBack(object value, Type targetType, 
        object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
