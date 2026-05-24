using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LedCube.Animator.Controls.MenuBar;

public partial class MenuBar : UserControl
{
    public MenuBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
