using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LedCube.Core.UI.Dialog.SimpleDialog;

[ObservableObject]
public partial class SimpleDialogViewModel
{
    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(YesIsPrimary))]
    [NotifyPropertyChangedFor(nameof(OkIsPrimary))]
    private bool _cancelIsPrimary;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowOkButton))]
    [NotifyPropertyChangedFor(nameof(ShowCancelButton))]
    [NotifyPropertyChangedFor(nameof(ShowYesButton))]
    [NotifyPropertyChangedFor(nameof(ShowNoButton))]
    [NotifyPropertyChangedFor(nameof(YesIsPrimary))]
    [NotifyPropertyChangedFor(nameof(OkIsPrimary))]
    private SimpleDialogButtons _buttons;

    [ObservableProperty]
    private DialogResult _result;

    public bool ShowOkButton => Buttons is SimpleDialogButtons.Ok or SimpleDialogButtons.OkCancel;

    public bool ShowCancelButton => Buttons is SimpleDialogButtons.OkCancel or SimpleDialogButtons.YesNoCancel;

    public bool ShowYesButton => Buttons is SimpleDialogButtons.YesNo or SimpleDialogButtons.YesNoCancel;
    
    public bool ShowNoButton => Buttons is SimpleDialogButtons.YesNo or SimpleDialogButtons.YesNoCancel;

    public bool YesIsPrimary => !CancelIsPrimary && ShowYesButton;
    
    public bool OkIsPrimary => !CancelIsPrimary && ShowOkButton;
    
    [RelayCommand]
    private void OnOkClicked(object window)
    {
        Result = DialogResult.Ok;
        if (window is Window w)
        {
            w.DialogResult = true;
            w.Close();
        }
    } 
    
    [RelayCommand]
    private void OnCancelClicked(object window)
    {
        Result = DialogResult.Cancel;
        if (window is Window w)
        {
            w.DialogResult = null;
            w.Close();
        }
    }
    
    [RelayCommand]
    private void OnYesClicked(object window)
    {
        Result = DialogResult.Yes;
        if (window is Window w)
        {
            w.DialogResult = true;
            w.Close();
        }
    }
    
    [RelayCommand]
    private void OnNoClicked(object window)
    {
        Result = DialogResult.No;
        if (window is Window w)
        {
            w.DialogResult = false;
            w.Close();
        }
    }

    [RelayCommand]
    private void OnClosed(object window)
    {
        if (Result == DialogResult.None)
        {
            Result = DialogResult.Cancel; 
        }
    }
}