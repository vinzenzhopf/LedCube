using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LedCube.Animator.Controls.SettingsWindow;

public partial class SettingsWindow : Window
{
    private SettingsViewModel? _viewModel;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _viewModel = viewModel;
        _viewModel.CloseAction = Close;
        Closing += OnClosingHandler;
    }

    private void OnClosingHandler(object? sender, WindowClosingEventArgs e)
    {
        if (_viewModel?.SaveEnabled == false)
            e.Cancel = true;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
