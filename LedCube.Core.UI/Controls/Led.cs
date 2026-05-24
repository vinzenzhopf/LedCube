using Avalonia;
using Avalonia.Controls;

namespace LedCube.Core.UI.Controls;

public class Led : CheckBox
{
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<Led, double>(nameof(Size));

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
