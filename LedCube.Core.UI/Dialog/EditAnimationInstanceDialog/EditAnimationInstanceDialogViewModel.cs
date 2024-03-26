using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.BaseDialog;
using LedCube.Core.UI.Dialog.SelectAnimationDialog;
using LedCube.PluginHost;
using Microsoft.Extensions.Logging;
using AnimationInstanceViewModel = LedCube.Core.UI.Controls.AnimationInstanceList.AnimationInstanceViewModel;
using AnimationViewModel = LedCube.Core.UI.Controls.AnimationInstanceList.AnimationViewModel;
using ILogger = Microsoft.Extensions.Logging.ILogger;

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
        new(new AnimationInstanceViewModel(AnimationViewModel.DefaultAnimation));

    public EditAnimationInstanceDialogViewModel(ILoggerFactory loggerFactory, IPluginManager pluginManager)
    {
        _logger = loggerFactory.CreateLogger(GetType());
        _pluginManager = pluginManager;
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