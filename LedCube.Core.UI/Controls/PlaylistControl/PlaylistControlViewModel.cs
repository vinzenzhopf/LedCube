using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;
using LedCube.Core.UI.Dialog.SelectAnimationDialog;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;

namespace LedCube.Core.UI.Controls.PlaylistControl;

public partial class PlaylistControlViewModel : ObservableObject,
    IRecipient<PlaylistSelectionChangedMessage>,
    IRecipient<PlaylistEntryEditedMessage>
{
    private readonly IPlaylistService _playlistService;
    private readonly IPlaybackService _playbackService;
    private readonly Dictionary<PlaylistEntry, PlaylistEntryControlViewModel> _entryMap = new();

    public ObservableCollection<PlaylistEntryControlViewModel> Instances { get; } = [];

    [ObservableProperty]
    private PlaylistEntryControlViewModel? _selectedInstance;

    public PlaylistControlViewModel(IPlaylistService playlistService, IPlaybackService playbackService)
    {
        _playlistService = playlistService;
        _playbackService = playbackService;
        ((INotifyCollectionChanged)playlistService.Entries).CollectionChanged += OnEntriesChanged;
        ((INotifyPropertyChanged)playbackService).PropertyChanged += OnPlaybackPropertyChanged;
        WeakReferenceMessenger.Default.Register<PlaylistSelectionChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<PlaylistEntryEditedMessage>(this);
    }

    private void OnPlaybackPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IPlaybackService.CurrentEntry) or nameof(IPlaybackService.PlaybackState))
            UpdateDisplayStates();
    }

    private void UpdateDisplayStates()
    {
        var current = _playbackService.CurrentEntry;
        var state = _playbackService.PlaybackState;
        foreach (var (entry, vm) in _entryMap)
        {
            vm.DisplayState = entry == current
                ? state switch
                {
                    PlaybackState.Playing => EntryDisplayState.Playing,
                    PlaybackState.Paused  => EntryDisplayState.Paused,
                    _                     => EntryDisplayState.Active,
                }
                : EntryDisplayState.None;
        }
    }

    partial void OnSelectedInstanceChanged(PlaylistEntryControlViewModel? value)
    {
        _playlistService.Select(value?.Entry);
    }

    public void Receive(PlaylistSelectionChangedMessage message)
    {
        var vm = message.Entry is null ? null : _entryMap.GetValueOrDefault(message.Entry);
        if (SelectedInstance != vm)
            SelectedInstance = vm;
    }

    public void Receive(PlaylistEntryEditedMessage message)
    {
        if (!_entryMap.TryGetValue(message.Entry, out var vm)) return;
        vm.InstanceName = message.Entry.InstanceName;
        vm.RepeatCount = message.Entry.RepeatCount;
        vm.FrameTimeOverride = message.Entry.FrameTimeOverride;
    }

    private void OnEntriesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (PlaylistEntry entry in e.NewItems!)
                {
                    var vm = new PlaylistEntryControlViewModel(entry);
                    _entryMap[entry] = vm;
                    Instances.Add(vm);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                foreach (PlaylistEntry entry in e.OldItems!)
                {
                    if (_entryMap.Remove(entry, out var vm))
                        Instances.Remove(vm);
                }
                break;
            case NotifyCollectionChangedAction.Move:
                Instances.Move(e.OldStartingIndex, e.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                _entryMap.Clear();
                Instances.Clear();
                break;
        }
        UpdateIndex();
    }

    private void UpdateIndex()
    {
        for (var i = 0; i < Instances.Count; i++)
            Instances[i].Index = i;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task AddAnimation()
    {
        var msg = WeakReferenceMessenger.Default.Send(new OpenSelectAnimationDialogMessage());
        await msg.Completion.Task;
        if (msg.Result is { Result: true, Animation.GeneratorInfo: not null, Animation.TypeInfo: not null })
        {
            var anim = msg.Result.Animation;
            var entry = new PlaylistEntry(anim.GeneratorInfo, anim.TypeInfo)
            {
                InstanceName = msg.Result.InstanceName ?? string.Empty
            };
            _playlistService.Add(entry);
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task EditAnimation()
    {
        if (SelectedInstance is null) return;

        var copy = new PlaylistEntryControlViewModel(SelectedInstance);
        var msg = WeakReferenceMessenger.Default.Send(new EditAnimationInstanceDialogMessage(copy));
        await msg.Completion.Task;
        if (msg.Result?.Result is not true) return;

        var originalEntry = SelectedInstance.Entry;
        if (originalEntry is null) return;

        if (copy.Animation.TypeInfo == originalEntry.TypeInfo)
        {
            // Same animation type — update in place
            originalEntry.InstanceName = copy.InstanceName;
            originalEntry.RepeatCount = copy.RepeatCount;
            originalEntry.FrameTimeOverride = copy.FrameTimeOverride;
            if (_entryMap.TryGetValue(originalEntry, out var vm))
            {
                vm.InstanceName = originalEntry.InstanceName;
                vm.RepeatCount = originalEntry.RepeatCount;
                vm.FrameTimeOverride = originalEntry.FrameTimeOverride;
            }
        }
        else
        {
            // Different animation — replace entry
            if (copy.Animation.GeneratorInfo is null || copy.Animation.TypeInfo is null)
                return;
            var idx = _playlistService.Entries.IndexOf(originalEntry);
            var newEntry = new PlaylistEntry(copy.Animation.GeneratorInfo, copy.Animation.TypeInfo)
            {
                InstanceName = copy.InstanceName,
                RepeatCount = copy.RepeatCount,
                FrameTimeOverride = copy.FrameTimeOverride,
            };
            _playlistService.Remove(originalEntry);
            _playlistService.Insert(idx, newEntry);
            _playlistService.Select(newEntry);
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task DeleteAnimation()
    {
        if (SelectedInstance?.Entry is not null)
            _playlistService.Remove(SelectedInstance.Entry);
        return Task.CompletedTask;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task CopyAnimation()
    {
        if (SelectedInstance?.Entry is null) return Task.CompletedTask;
        var copy = new PlaylistEntry(SelectedInstance.Entry);
        copy.InstanceName += " (Copy)";
        _playlistService.Add(copy);
        return Task.CompletedTask;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task MoveAnimationUp()
    {
        if (SelectedInstance?.Entry is null) return Task.CompletedTask;
        var idx = _playlistService.Entries.IndexOf(SelectedInstance.Entry);
        if (idx > 0) _playlistService.Move(idx, idx - 1);
        return Task.CompletedTask;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task MoveAnimationDown()
    {
        if (SelectedInstance?.Entry is null) return Task.CompletedTask;
        var idx = _playlistService.Entries.IndexOf(SelectedInstance.Entry);
        if (idx < _playlistService.Entries.Count - 1) _playlistService.Move(idx, idx + 1);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task PlayEntry(PlaylistEntryControlViewModel vm)
    {
        if (vm.Entry is null) return;
        _playlistService.Select(vm.Entry);
        await _playbackService.UpdateFrameGeneratorAsync(vm.Entry, System.Threading.CancellationToken.None);
        _playbackService.StartPlayback();
    }
}
