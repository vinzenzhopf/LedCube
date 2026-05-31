using Avalonia;
using Avalonia.Controls.Primitives;

namespace LedCube.Core.UI.Controls;

/// <summary>
/// One-line "label: value [unit]" row used in stats panels.
/// Templated in Styles/Controls.axaml.
/// </summary>
public class StatRow : TemplatedControl
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<StatRow, string?>(nameof(Label));

    public static readonly StyledProperty<object?> ValueProperty =
        AvaloniaProperty.Register<StatRow, object?>(nameof(Value));

    public static readonly StyledProperty<string?> UnitProperty =
        AvaloniaProperty.Register<StatRow, string?>(nameof(Unit));

    public string? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public object? Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public string? Unit  { get => GetValue(UnitProperty);  set => SetValue(UnitProperty,  value); }
}
