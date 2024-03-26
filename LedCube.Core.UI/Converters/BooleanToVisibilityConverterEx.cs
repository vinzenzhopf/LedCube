using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LedCube.Core.UI.Converters;
/// <summary>
/// Convert between boolean and visibility
/// </summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public sealed class BooleanToVisibilityConverterEx : IValueConverter
{
    public Visibility TrueValue { get; set; } = Visibility.Visible;
    public Visibility FalseValue { get; set; } = Visibility.Collapsed;
    
    /// <summary>
    /// Convert bool or Nullable&lt;bool&gt; to Visibility
    /// </summary>
    /// <param name="value">bool or Nullable&lt;bool&gt;</param>
    /// <param name="targetType">Visibility</param>
    /// <param name="parameter">null</param>
    /// <param name="culture">null</param>
    /// <returns>Visible or Collapsed</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolValue = value switch
        {
            null => false,
            bool b => b,
            _ => false
        };

        return (boolValue) ? TrueValue : FalseValue;
    }

    /// <summary>
    /// Convert Visibility to boolean
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == TrueValue;
        }
        return false;
    }
}