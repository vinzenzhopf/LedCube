using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;
using LedCube.Core.UI.Dialog.SelectAnimationDialog;
using LedCube.Core.UI.Services;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Controls.AnimationInstanceList;

[ObservableObject]
public partial class AnimationListViewModel
{
    private readonly IPlaybackService _playbackService;
    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;

    public ObservableCollection<AnimationInstanceViewModel> Instances { get; } = [];

    [ObservableProperty]
    private AnimationInstanceViewModel? _selectedInstance;
    
    public AnimationListViewModel(ILoggerFactory loggerFactory, IPlaybackService playbackService, IPluginManager pluginManager, IServiceProvider serviceProvider )
    {
        _playbackService = playbackService;
        _pluginManager = pluginManager;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    public Task AddAnimation()
    {
        var result = WeakReferenceMessenger.Default.Send(new OpenSelectAnimationDialogMessage());
        if (result.Result is {Result: true, Animation: not null})
        {
            var instance = new AnimationInstanceViewModel(result.Result.Animation)
            {
                InstanceName = result.Result.InstanceName ?? string.Empty
            };
            Instances.Add(instance);
            UpdateIndex();
        }
        return Task.CompletedTask;
    }

    private void UpdateIndex()
    {
        for (var index = 0; index < Instances.Count; index++)
        {
            var a = Instances[index];
            a.Index = index;
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task EditAnimation()
    {
        if (SelectedInstance is null)
        {
            return Task.CompletedTask;
        }

        var copy = new AnimationInstanceViewModel(SelectedInstance); 
        var result = WeakReferenceMessenger.Default.Send(new EditAnimationInstanceDialogMessage(copy));
        if (result.Result?.Result is not true)
        {
            return Task.CompletedTask;
        }
        var index = Instances.IndexOf(SelectedInstance);
        Instances.Insert(index, copy);
        UpdateIndex();
        return Task.CompletedTask;
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task DeleteAnimation()
    {
        return Task.CompletedTask;
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task CopyAnimation()
    {
        return Task.CompletedTask;
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task MoveAnimationUp()
    {
        return Task.CompletedTask;
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task MoveAnimationDown()
    {
        
    }
    
    //Design Time Constructor
    // private AnimationListViewModel()
    // {
    //     _pluginManager = null!;
    //     _playbackService = null!;
    //     Animations = [ 
    //         new AnimationViewModel()
    //         {
    //             Name = "Test1",
    //             Description = "Desc1",
    //             FrameCount = 350,
    //             FrameTime = TimeSpan.FromMilliseconds(5)
    //         },
    //         new AnimationViewModel()
    //         {
    //             Name = "Test2",
    //             Description = "Desc2",
    //             FrameCount = 10,
    //             FrameTime = TimeSpan.FromMilliseconds(200)
    //         },
    //         new AnimationViewModel()
    //         {
    //             Name = "Test3",
    //             Description = "Desc3",
    //             FrameCount = 5600,
    //             FrameTime = TimeSpan.FromMilliseconds(5)
    //         }
    //     ];
    // }
}