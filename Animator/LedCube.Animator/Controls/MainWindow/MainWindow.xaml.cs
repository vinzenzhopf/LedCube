using MahApps.Metro.Controls;

namespace LedCube.Animator.Controls.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly NavigationController _navigationController;

        public MainWindow(MainViewModel mainViewModel, NavigationController navigationController)
        {
            DataContext = mainViewModel;
            _navigationController = navigationController;
            InitializeComponent();
        }
        
    }
}