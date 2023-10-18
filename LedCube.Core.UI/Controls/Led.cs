using System.Windows;
using System.Windows.Controls;

namespace LedCube.Streamer.UI.Controls;

public class Led : CheckBox
{
    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(double), typeof(Led));

    static Led()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Led),
            new FrameworkPropertyMetadata(typeof(Led)));
    }

    public double Size
    {
        get => (double) GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
