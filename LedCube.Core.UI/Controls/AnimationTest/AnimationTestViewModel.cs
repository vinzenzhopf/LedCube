using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LedCube.Core.UI.Controls.AnimationInstanceList;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Services;
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
            
            var animation = new AnimationViewModel()
            {
                Name = a.Info.Name,
                Description = a.Info.Description,
                TypeInfo = a.TypeInfo
            };
            Animations.Add(animation);
        }
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task SelectedAnimationChanged(CancellationToken token)
    {
        if (SelectedAnimation is null)
            return Task.CompletedTask;
        var entry = _pluginManager.AllFrameGeneratorInfos()
            .FirstOrDefault(x => x.TypeInfo.Equals(SelectedAnimation.TypeInfo));
        if(entry is null)
            return Task.CompletedTask;
        _playbackService.UpdateFrameGenerator(entry, SelectedAnimation);
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task Closed()
    {
        return Task.CompletedTask;
    }
}