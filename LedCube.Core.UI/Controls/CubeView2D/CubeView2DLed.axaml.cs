using Avalonia;
using Avalonia.Controls.Primitives;

namespace LedCube.Core.UI.Controls.CubeView2D;

public partial class CubeView2DLed : ToggleButton
{
    public static readonly StyledProperty<int> IndexProperty =
        AvaloniaProperty.Register<CubeView2DLed, int>(nameof(Index), 0);

    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<CubeView2DLed, double>(nameof(Size), 10.0);

    public int Index
    {
        get => GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public CubeView2DLed()
    {
        InitializeComponent();
    }
}
