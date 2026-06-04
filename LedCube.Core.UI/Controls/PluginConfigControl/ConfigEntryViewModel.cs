using System;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.PluginBase;

namespace LedCube.Core.UI.Controls.PluginConfigControl;

public partial class ConfigEntryViewModel : ObservableObject
{
    private readonly AnimationConfig _config;
    private readonly Action? _onChanged;

    public AnimationConfigDescriptor Descriptor { get; }

    [ObservableProperty]
    private object? _value;

    partial void OnValueChanged(object? value)
    {
        _config[Descriptor.Key] = value;
        OnPropertyChanged(nameof(StringValue));
        OnPropertyChanged(nameof(BoolValue));
        OnPropertyChanged(nameof(EnumValue));
        OnPropertyChanged(nameof(DoubleValue));
        _onChanged?.Invoke();
    }

    public string StringValue
    {
        get => Value?.ToString() ?? string.Empty;
        set
        {
            Value = Descriptor.Type switch
            {
                AnimationConfigType.Int => int.TryParse(value, out var i) ? (object)i : Value,
                AnimationConfigType.Float => float.TryParse(value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var f) ? (object)f : Value,
                _ => value
            };
        }
    }

    public bool BoolValue
    {
        get => Value is true;
        set => Value = value;
    }

    public string? EnumValue
    {
        get => Value?.ToString();
        set => Value = value;
    }

    /// <summary>Numeric value for slider-style editors. Writes back as int or float per the descriptor type.</summary>
    public double DoubleValue
    {
        get => ToDouble(Value, 0.0);
        set => Value = Descriptor.Type == AnimationConfigType.Int
            ? (int)Math.Round(value)
            : (float)Math.Round(value, 2);
    }

    public double Minimum => ToDouble(Descriptor.MinValue, 0.0);

    public double Maximum => ToDouble(Descriptor.MaxValue, 1.0);

    /// <summary>True when this is a numeric entry with both bounds set — render it as a slider.</summary>
    public bool HasRange => Descriptor.Type is AnimationConfigType.Int or AnimationConfigType.Float
        && Descriptor.MinValue is not null && Descriptor.MaxValue is not null;

    public bool IsInteger => Descriptor.Type == AnimationConfigType.Int;

    private static double ToDouble(object? value, double fallback) => value switch
    {
        float f => f,
        double d => d,
        int i => i,
        long l => l,
        _ => double.TryParse(value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
            ? v
            : fallback,
    };

    public ConfigEntryViewModel(AnimationConfigDescriptor descriptor, AnimationConfig config, Action? onChanged = null)
    {
        Descriptor = descriptor;
        _config = config;
        _onChanged = onChanged;
        _value = config.TryGetValue(descriptor.Key, out var v) ? v : descriptor.DefaultValue;
    }
}
