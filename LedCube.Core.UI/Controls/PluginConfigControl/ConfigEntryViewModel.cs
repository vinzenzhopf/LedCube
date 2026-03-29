using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.PluginBase;

namespace LedCube.Core.UI.Controls.PluginConfigControl;

public partial class ConfigEntryViewModel : ObservableObject
{
    private readonly AnimationConfig _config;

    public AnimationConfigDescriptor Descriptor { get; }

    [ObservableProperty]
    private object? _value;

    partial void OnValueChanged(object? value)
    {
        _config[Descriptor.Key] = value;
        OnPropertyChanged(nameof(StringValue));
        OnPropertyChanged(nameof(BoolValue));
        OnPropertyChanged(nameof(EnumValue));
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

    public ConfigEntryViewModel(AnimationConfigDescriptor descriptor, AnimationConfig config)
    {
        Descriptor = descriptor;
        _config = config;
        _value = config.TryGetValue(descriptor.Key, out var v) ? v : descriptor.DefaultValue;
    }
}
