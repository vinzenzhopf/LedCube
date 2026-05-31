using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Serilog.Events;

namespace LedCube.Core.UI.Converters;

/// <summary>
/// Maps a Serilog <see cref="LogEventLevel"/> to the theme-aware brush
/// defined in General.axaml (LogFatalBrush, LogErrorBrush, …).
/// </summary>
public sealed class LogLevelToBrushConverter : ConverterExtensionBase, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = value switch
        {
            LogEventLevel.Fatal       => "LogFatalBrush",
            LogEventLevel.Error       => "LogErrorBrush",
            LogEventLevel.Warning     => "LogWarningBrush",
            LogEventLevel.Information => "LogInformationBrush",
            LogEventLevel.Debug       => "LogDebugBrush",
            LogEventLevel.Verbose     => "LogVerboseBrush",
            _                         => "LogDefaultBrush",
        };

        var app = Application.Current;
        if (app is not null && app.TryGetResource(key, app.ActualThemeVariant, out var brush) && brush is IBrush b)
            return b;
        return Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
