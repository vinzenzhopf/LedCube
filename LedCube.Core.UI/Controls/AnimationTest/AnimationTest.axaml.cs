using Avalonia.Controls;

namespace LedCube.Core.UI.Controls.AnimationTest;

public partial class AnimationTest : UserControl
{
    public AnimationTest()
    {
        InitializeComponent();
    }

    private void OnSelectedAnimationChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is AnimationTestViewModel vm)
            vm.SelectedAnimationChangedCommand.Execute(null);
    }
}
