using System;
using System.Windows.Data;

namespace LedCube.Streamer.UI.Converter;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        try
        {
            return Enum.GetName((value.GetType()), value) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}