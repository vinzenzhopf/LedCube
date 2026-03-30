using System.Windows.Controls;
using System.Windows.Input;

namespace LedCube.Core.UI.Controls.PlaylistControl;

public partial class PlaylistControl : UserControl
{
    public PlaylistControl()
    {
        InitializeComponent();
    }

    private void OnItemDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListViewItem { DataContext: PlaylistEntryControlViewModel vm } &&
            DataContext is PlaylistControlViewModel parent)
        {
            parent.PlayEntryCommand.Execute(vm);
        }
    }
}
