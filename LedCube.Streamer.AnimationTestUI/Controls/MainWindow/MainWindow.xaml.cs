using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using LedCube.Core.UI.Messages;
using MahApps.Metro.Controls;
using Serilog;

namespace LedCube.Streamer.AnimationTestUI.Controls.MainWindow;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : MetroWindow
{
    public MainWindow(MainViewModel mainViewModel)
    {
        DataContext = mainViewModel;
        InitializeComponent();
    }
        
    private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new KeyEventMessage(e.Timestamp, e.Key, e.KeyStates, e.KeyboardDevice.Modifiers));
    }

    private void MainWindow_OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new KeyEventMessage(e.Timestamp, e.Key, e.KeyStates, e.KeyboardDevice.Modifiers));
    }
}