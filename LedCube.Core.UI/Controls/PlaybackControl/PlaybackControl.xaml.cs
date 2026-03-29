using System.Windows.Controls;
using LedCube.Core.UI.TimelineControl;

namespace LedCube.Core.UI.Controls.PlaybackControl;

public partial class PlaybackControl : UserControl
{
    public PlaybackControl()
    {
        InitializeComponent();
    }

    private void OnPlayheadChanged(object sender, PlayheadChangedEventArgs e)
    {
        if (DataContext is PlaybackControlViewModel vm)
            vm.PlaybackService.SeekToFrame(e.NewFrame);
    }
}