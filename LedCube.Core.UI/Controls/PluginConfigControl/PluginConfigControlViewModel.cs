using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using LedCube.Core.UI.Controls.AnimationInstanceList;

namespace LedCube.Core.UI.Controls.PluginConfigControl;

public partial class PluginConfigControlViewModel : ObservableObject
{
    private readonly AnimationListViewModel _animationList;
    private AnimationInstanceViewModel? _selectedInstance;

    public ObservableCollection<ConfigEntryViewModel> ConfigEntries { get; } = [];

    [ObservableProperty]
    private bool _hasConfigDescriptors;

    [ObservableProperty]
    private bool _hasSelection;

    public int RepeatCount
    {
        get => _selectedInstance?.RepeatCount ?? 1;
        set
        {
            if (_selectedInstance is not null)
                _selectedInstance.RepeatCount = value;
            OnPropertyChanged();
        }
    }

    public string FrameTimeOverrideMs
    {
        get => _selectedInstance?.FrameTimeOverride?.TotalMilliseconds.ToString("G") ?? string.Empty;
        set
        {
            if (_selectedInstance is null) return;
            if (string.IsNullOrWhiteSpace(value))
                _selectedInstance.FrameTimeOverride = null;
            else if (double.TryParse(value, System.Globalization.NumberStyles.Float,
                         System.Globalization.CultureInfo.InvariantCulture, out var ms))
                _selectedInstance.FrameTimeOverride = System.TimeSpan.FromMilliseconds(ms);
            OnPropertyChanged();
        }
    }

    public PluginConfigControlViewModel(AnimationListViewModel animationList)
    {
        _animationList = animationList;
        animationList.PropertyChanged += OnAnimationListPropertyChanged;
        UpdateEntries(animationList.SelectedInstance);
    }

    private void OnAnimationListPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AnimationListViewModel.SelectedInstance))
            UpdateEntries(_animationList.SelectedInstance);
    }

    private void UpdateEntries(AnimationInstanceViewModel? instance)
    {
        _selectedInstance = instance;
        ConfigEntries.Clear();

        HasSelection = instance is not null;
        OnPropertyChanged(nameof(RepeatCount));
        OnPropertyChanged(nameof(FrameTimeOverrideMs));

        var descriptors = instance?.Animation.GeneratorInfo?.ConfigDescriptors;
        HasConfigDescriptors = descriptors is { Count: > 0 };

        if (descriptors is null || instance is null)
            return;

        foreach (var descriptor in descriptors)
            ConfigEntries.Add(new ConfigEntryViewModel(descriptor, instance.Config));
    }
}
