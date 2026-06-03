using Avalonia;
using Avalonia.Markup.Xaml;

namespace LedCube.Core.UI.Test;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
