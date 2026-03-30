using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.UI.Controls.PlaylistControl;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Controls.AnimationTest;

public partial class AnimationTestViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IPluginManager _pluginManager;
    private readonly IPlaybackService _playbackService;

    public ObservableCollection<AnimationViewModel> Animations { get; set; } = new();

    [ObservableProperty]
    private AnimationViewModel? _selectedAnimation;

    public AnimationTestViewModel(ILogger<AnimationTestViewModel> logger, IPluginManager pluginManager, IPlaybackService playbackService)
    {
        _logger = logger;
        _pluginManager = pluginManager;
        _playbackService = playbackService;
    }

    [RelayCommand]
    private Task LoadedAsync(CancellationToken token)
    {
        Animations.Clear();
        foreach (var a in _pluginManager.AllFrameGeneratorInfos())
        {
            Animations.Add(new AnimationViewModel
            {
                Name = a.Info.Name,
                Description = a.Info.Description,
                TypeInfo = a.TypeInfo,
                GeneratorInfo = a.Info,
            });
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SelectedAnimationChanged(CancellationToken token)
    {
        if (SelectedAnimation is null) return;
        var frameEntry = _pluginManager.AllFrameGeneratorInfos()
            .FirstOrDefault(x => x.TypeInfo.Equals(SelectedAnimation.TypeInfo));
        if (frameEntry is null) return;
        var entry = new PlaylistEntry(frameEntry.Info, frameEntry.TypeInfo);
        await _playbackService.UpdateFrameGeneratorAsync(entry, token);
    }

    [RelayCommand]
    private Task Closed()
    {
        return Task.CompletedTask;
    }
}
