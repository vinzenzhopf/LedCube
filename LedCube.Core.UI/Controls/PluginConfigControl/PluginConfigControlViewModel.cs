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

    public string InstanceName
    {
        get => _selectedEntry?.InstanceName ?? string.Empty;
        set
        {
            if (_selectedEntry is null) return;
            _selectedEntry.InstanceName = value ?? string.Empty;
            NotifyEntryEdited();
            OnPropertyChanged();
        }
    }

    public int RepeatCount
    {
        get => _selectedEntry?.RepeatCount ?? 1;
        set
        {
            if (_selectedEntry is null) return;
            _selectedEntry.RepeatCount = value;
            NotifyEntryEdited();
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
            NotifyEntryEdited();
            OnPropertyChanged();
        }
    }

    private void NotifyEntryEdited()
    {
        if (_selectedEntry is not null)
            WeakReferenceMessenger.Default.Send(new PlaylistEntryEditedMessage(_selectedEntry));
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
        OnPropertyChanged(nameof(InstanceName));
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
