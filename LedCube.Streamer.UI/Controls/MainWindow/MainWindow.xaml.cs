using System.Windows;
using MahApps.Metro.Controls;

namespace LedCube.Streamer.UI.Controls.MainWindow
{
    public partial class MainWindow : MetroWindow
    {
        private readonly MainViewModel _mainViewModel;

        public MainWindow(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            DataContext = _mainViewModel;
            InitializeComponent();
        }
    }
}