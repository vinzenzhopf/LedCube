using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace LedCube.Core.UI.Controls.CubeView2D;

public class Led : ToggleButton
{
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Led, double>(nameof(Size), defaultValue: 28.0);

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public static readonly StyledProperty<IBrush?> FillBrushProperty =
        AvaloniaProperty.Register<Led, IBrush?>(nameof(FillBrush));

    /// <summary>Computed fill: Foreground when checked, Foreground brightened 0.9 when not.</summary>
    public IBrush? FillBrush
    {
        get => GetValue(FillBrushProperty);
        private set => SetValue(FillBrushProperty, value);
    }

    static Led()
    {
        ForegroundProperty.Changed.AddClassHandler<Led>((led, _) => led.UpdateFillBrush());
        IsCheckedProperty.Changed.AddClassHandler<Led>((led, _) => led.UpdateFillBrush());
    }

    private void UpdateFillBrush()
    {
        if (Foreground is not ISolidColorBrush fg)
        {
            FillBrush = Brushes.Transparent;
            return;
        }
        if (IsChecked == true)
        {
            FillBrush = fg;
            return;
        }
        const float factor = 0.9f;
        var c = fg.Color;
        var r = (byte)((255 - c.R) * factor + c.R);
        var g = (byte)((255 - c.G) * factor + c.G);
        var b = (byte)((255 - c.B) * factor + c.B);
        FillBrush = new SolidColorBrush(Color.FromArgb(c.A, r, g, b));
    }
}
