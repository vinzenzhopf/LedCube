using Avalonia;
using Avalonia.Controls.Primitives;

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
}
