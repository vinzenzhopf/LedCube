using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LedCube.Core.UI.Controls.StreamingControl;

namespace LedCube.Core.UI.Converters;

public class ConnectionStateToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is ConnectionState state)
            {
                return state switch
                {
                    ConnectionState.Disconnected => Brushes.Gray,
                    ConnectionState.Connected => Brushes.Green,
                    ConnectionState.Connecting => Brushes.Orange,
                    _ => Brushes.Red
                };
            }
            return Brushes.Red;
        }
        catch
        {
            return Brushes.Gray;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}