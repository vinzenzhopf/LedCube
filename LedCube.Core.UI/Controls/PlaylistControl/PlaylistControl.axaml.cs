using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LedCube.Core.UI.Controls.PlaylistControl;

public partial class PlaylistControl : UserControl
{
    public PlaylistControl()
    {
        InitializeComponent();
    }

    private void OnItemDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Control { DataContext: PlaylistEntryControlViewModel vm } &&
            DataContext is PlaylistControlViewModel parent)
        {
            parent.PlayEntryCommand.Execute(vm);
        }
    }
}
