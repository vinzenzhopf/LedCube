using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LedCube.Core.UI.Dialog.BaseDialog;

[ObservableObject]
public partial class BaseDialogViewModel<T> where T : BaseDialogMessage
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _text;
    
    public bool OkEnabled { get; }

    protected BaseDialogMessage _dialogMessage; 
    
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
        w.DialogResult = true;
        w.Close();
    } 
    
    [RelayCommand]
    private void OnCancelClicked(object window)
    {
        if (window is not Window w) return;
        _dialogMessage.DialogResult = false;
        w.DialogResult = false;
        w.Close();
    }
    
    [RelayCommand]
    private void OnClosed(object window)
    {
        if (window is not Window w) return;
        _dialogMessage.DialogResult = null;
        w.DialogResult = null;
    }
}