using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LedCube.Animator.Controls.MainWindow;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
