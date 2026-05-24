using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Messages;

namespace LedCube.Streamer.AnimationTestUI.Controls.MainWindow;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel mainViewModel)
    {
        DataContext = mainViewModel;
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new KeyEventMessage(0, e.Key, true, e.KeyModifiers));
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new KeyEventMessage(0, e.Key, false, e.KeyModifiers));
        base.OnKeyUp(e);
    }
}
