using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Dialog.BaseDialog;
using MahApps.Metro.Controls;

namespace LedCube.Core.UI.Dialog.SelectAnimationDialog;

public partial class SelectAnimationDialog : MetroWindow, IRecipient<CloseDialogMessage>
{
    public SelectAnimationDialog()
    {
        WeakReferenceMessenger.Default.Register(this);
        InitializeComponent();
    }

    public void Receive(CloseDialogMessage message)
    {
        if (message.Name != nameof(SelectAnimationDialogViewModel)) return;
        
        // DialogResult = message.Result;
        Close();
    }
}