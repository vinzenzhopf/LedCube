using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Controls.PlaylistControl;
using LedCube.Core.UI.Dialog.BaseDialog;
using LedCube.Core.UI.Dialog.SelectAnimationDialog;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;

namespace LedCube.Core.UI.Dialog.EditAnimationInstanceDialog;

public partial class EditAnimationInstanceDialogViewModel : ObservableValidator
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

    public ObservableCollection<AnimationViewModel> FilteredAnimations { get; } = new();
    private ObservableCollection<AnimationViewModel> Animations { get; } = new();

    public bool AddButtonActive => SelectedAnimation is not null && !HasErrors;

    public SelectAnimationDialogResult? DialogResult { get; set; }

    [ObservableProperty]
    private EditAnimationInstanceDialogMessage _message =
        new(new PlaylistEntryControlViewModel(AnimationViewModel.DefaultAnimation));

    public EditAnimationInstanceDialogViewModel(ILoggerFactory loggerFactory, IPluginManager pluginManager)
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
            var animation = new AnimationViewModel
            {
                Name = a.Info.Name,
                Description = a.Info.Description,
                TypeInfo = a.TypeInfo,
                GeneratorInfo = a.Info,
            };
            Animations.Add(animation);
            FilteredAnimations.Add(animation);
        }

        // Pre-select the current animation
        var currentTypeInfo = Message.Instance.Animation.TypeInfo;
        SelectedAnimation = currentTypeInfo is not null
            ? FilteredAnimations.FirstOrDefault(x => x.TypeInfo == currentTypeInfo) ?? FilteredAnimations.FirstOrDefault()
            : FilteredAnimations.FirstOrDefault();
        AnimationInstanceName = Message.Instance.InstanceName;
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(AddButtonActive))]
    private async Task AddAsync()
    {
        if (SelectedAnimation is null) return;
        DialogResult = new SelectAnimationDialogResult(true, SelectedAnimation, AnimationInstanceName);
        SendCloseMessage(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = new SelectAnimationDialogResult(false, null, null);
        SendCloseMessage(false);
    }

    private void SendCloseMessage(bool? result)
    {
        WeakReferenceMessenger.Default.Send(new CloseDialogMessage(nameof(EditAnimationInstanceDialogViewModel), result));
    }
}
