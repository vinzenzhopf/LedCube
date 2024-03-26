using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.BaseDialog;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;
using AnimationViewModel = LedCube.Core.UI.Controls.AnimationInstanceList.AnimationViewModel;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LedCube.Core.UI.Dialog.SelectAnimationDialog;

public partial class SelectAnimationDialogViewModel : ObservableValidator
{
    private readonly ILogger _logger;
    private readonly IPluginManager _pluginManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddButtonActive))]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private AnimationViewModel? _selectedAnimation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AddButtonActive))]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string? _animationInstanceName;

    [ObservableProperty]
    private string? _searchText = string.Empty;

    public ObservableCollection<AnimationViewModel> FilteredAnimations { get; } = new();
    private ObservableCollection<AnimationViewModel> Animations { get; } = new();

    public bool AddButtonActive => SelectedAnimation is not null && !HasErrors;

    public SelectAnimationDialogResult? DialogResult { get; set; }

    public SelectAnimationDialogViewModel(ILoggerFactory loggerFactory, IPluginManager pluginManager)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _pluginManager = pluginManager;
    }
    
    [RelayCommand]
    private Task LoadedAsync(CancellationToken token)
    {
        Animations.Clear(); 
        FilteredAnimations.Clear();
        foreach (var a in _pluginManager.AllFrameGeneratorInfos())
        {
            var animation = new AnimationViewModel()
            {
                Name = a.Info.Name,
                Description = a.Info.Description,
            };
            Animations.Add(animation);
            FilteredAnimations.Add(animation);
        }
        SelectedAnimation = FilteredAnimations.FirstOrDefault();
        return Task.CompletedTask;
    }

    partial void OnSearchTextChanged(string? value)
    {
        if (value is null)
        {
            return;
        }
        FilteredAnimations.Clear();
        foreach (var instance in Animations.Where(
                     x => x.Name.Contains(value) || x.Description.Contains(value)))
        {
            FilteredAnimations.Add(instance);
        }
    }

    [RelayCommand(CanExecute = nameof(AddButtonActive))]
    private async Task AddAsync()
    {
        if (SelectedAnimation is null)
        {
            return;
        }
        DialogResult = new SelectAnimationDialogResult(true, SelectedAnimation, AnimationInstanceName);
        SendCloseMessage(true);
    }
    
    [RelayCommand]
    private void Closed()
    {
        // DialogResult = new SelectAnimationDialogResult(false, null);
        // SendCloseMessage(false);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = new SelectAnimationDialogResult(false, null, null);
        SendCloseMessage(false);
    }

    private void SendCloseMessage(bool? result)
    {
        WeakReferenceMessenger.Default.Send(new CloseDialogMessage(nameof(SelectAnimationDialogViewModel), result));
    }
}