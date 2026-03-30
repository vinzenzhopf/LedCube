using System;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Services.Playlist;

namespace LedCube.Core.UI.Controls.PluginConfigControl;

public partial class PluginConfigControlViewModel : ObservableObject, IRecipient<PlaylistSelectionChangedMessage>
{
    private PlaylistEntry? _selectedEntry;

    public ObservableCollection<ConfigEntryViewModel> ConfigEntries { get; } = [];

    [ObservableProperty]
    private bool _hasConfigDescriptors;

    [ObservableProperty]
    private bool _hasSelection;

    public int RepeatCount
    {
        get => _selectedEntry?.RepeatCount ?? 1;
        set
        {
            if (_selectedEntry is not null)
                _selectedEntry.RepeatCount = value;
            OnPropertyChanged();
        }
    }

    public string FrameTimeOverrideMs
    {
        get => _selectedEntry?.FrameTimeOverride?.TotalMilliseconds.ToString("G") ?? string.Empty;
        set
        {
            if (_selectedEntry is null) return;
            if (string.IsNullOrWhiteSpace(value))
                _selectedEntry.FrameTimeOverride = null;
            else if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
                _selectedEntry.FrameTimeOverride = TimeSpan.FromMilliseconds(ms);
            OnPropertyChanged();
        }
    }

    public PluginConfigControlViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(PlaylistSelectionChangedMessage message)
    {
        UpdateEntries(message.Entry);
    }

    private void UpdateEntries(PlaylistEntry? entry)
    {
        _selectedEntry = entry;
        ConfigEntries.Clear();

        HasSelection = entry is not null;
        OnPropertyChanged(nameof(RepeatCount));
        OnPropertyChanged(nameof(FrameTimeOverrideMs));

        var descriptors = entry?.Info.ConfigDescriptors;
        HasConfigDescriptors = descriptors is { Count: > 0 };

        if (descriptors is null || entry is null)
            return;

        foreach (var descriptor in descriptors)
            ConfigEntries.Add(new ConfigEntryViewModel(descriptor, entry.Config));
    }
}
