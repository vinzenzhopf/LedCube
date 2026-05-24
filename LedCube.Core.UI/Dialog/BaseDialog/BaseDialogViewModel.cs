using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LedCube.Core.UI.Dialog.BaseDialog;

public partial class BaseDialogViewModel<T> : ObservableObject where T : BaseDialogMessage
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _text = string.Empty;
    
    public bool OkEnabled { get; }

    protected BaseDialogMessage _dialogMessage = null!;
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private Task Loaded(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    [RelayCommand(CanExecute = nameof(OkEnabled))]
    private void OnOkClicked(object window)
    {
        if (window is not Window w) return;
        _dialogMessage.DialogResult = true;
        w.Close(true);
    }

    [RelayCommand]
    private void OnCancelClicked(object window)
    {
        if (window is not Window w) return;
        _dialogMessage.DialogResult = false;
        w.Close(false);
    }

    [RelayCommand]
    private void OnClosed(object window)
    {
        if (window is not Window w) return;
        _dialogMessage.DialogResult = null;
    }
}