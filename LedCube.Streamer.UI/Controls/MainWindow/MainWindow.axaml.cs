using Avalonia.Controls;

namespace LedCube.Streamer.UI.Controls.MainWindow;

public partial class MainWindow : Window
{
    private readonly MainViewModel _mainViewModel;

    public MainWindow(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        DataContext = _mainViewModel;
        InitializeComponent();
    }
}
