using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LedCube.Animator.Controls.LogAppender;

public partial class LogAppenderControl : UserControl
{
    public LogAppenderControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
