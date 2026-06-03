using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using LedCube.Core.UI.Controls.PlaybackControl;
using LedCube.Core.UI.Services.Playback;
using LedCube.Core.UI.Services.Playlist;
using LedCube.Core.UI.Test.Fakes;
using LedCube.PluginBase;
using Xunit;

namespace LedCube.Core.UI.Test;

/// <summary>
/// Proves the headless Avalonia harness can exercise a real control + ViewModel:
/// loads XAML, resolves bindings, runs commands on the UI thread, and renders to a bitmap.
/// </summary>
public class PlaybackControlTests
{
    private static PlaylistEntry MakeEntry(string name = "TestAnim", string description = "A test animation")
        => new(new FrameGeneratorInfo(name, description), typeof(PlaybackControlViewModel).GetTypeInfo());

    [AvaloniaFact]
    public void Control_BindsAnimationName_AndRenders()
    {
        var playback = new FakePlaybackService { CurrentEntry = MakeEntry(), FrameCount = 100 };
        var vm = new PlaybackControlViewModel(playback, new FakePlaylistService());

        var control = new PlaybackControl { DataContext = vm };
        var window = new Window { Content = control, Width = 640, Height = 200 };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        // Binding actually flowed to a visual: the animation name appears in a TextBlock.
        var texts = control.GetLogicalDescendants().OfType<TextBlock>().Select(t => t.Text).ToList();
        Assert.Contains("TestAnim", texts);

        // Rendering works — capture a PNG screenshot to the test output for inspection.
        var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        Assert.True(frame!.PixelSize.Width > 0 && frame.PixelSize.Height > 0);

        var outDir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(outDir);
        frame.Save(Path.Combine(outDir, "playback-control.png"));
    }

    [AvaloniaFact]
    public void PlayContinueCommand_StartsPlayback_WhenStopped()
    {
        var playback = new FakePlaybackService { CurrentEntry = MakeEntry(), PlaybackState = PlaybackState.Stopped };
        var vm = new PlaybackControlViewModel(playback, new FakePlaylistService());

        Assert.True(vm.PlayContinueCommand.CanExecute(null));
        vm.PlayContinueCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(playback.StartPlaybackCalled);
        Assert.False(playback.ContinuePlaybackCalled);
    }

    [AvaloniaFact]
    public void ServicePlaybackStateChange_PropagatesToViewModel()
    {
        var playback = new FakePlaybackService { CurrentEntry = MakeEntry() };
        var vm = new PlaybackControlViewModel(playback, new FakePlaylistService());

        playback.PlaybackState = PlaybackState.Playing;

        Assert.Equal(PlaybackState.Playing, vm.PlaybackState);
    }

    [AvaloniaFact]
    public void ForwardCommand_PlaysNextEntry_WhenPlaylistHasMultipleEntries()
    {
        var playlist = new FakePlaylistService();
        playlist.Add(MakeEntry("A"));
        playlist.Add(MakeEntry("B"));
        var vm = new PlaybackControlViewModel(new FakePlaybackService { CurrentEntry = MakeEntry() }, playlist);

        Assert.True(vm.ForwardCommand.CanExecute(null));
        vm.ForwardCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        // Navigates playback to the next entry without touching the playlist selection.
        Assert.True(playlist.PlayNextCalled);
        Assert.Null(playlist.SelectedEntry);
    }

    [AvaloniaFact]
    public void BackwardCommand_PlaysPreviousEntry_WhenPlaylistHasMultipleEntries()
    {
        var playlist = new FakePlaylistService();
        playlist.Add(MakeEntry("A"));
        playlist.Add(MakeEntry("B"));
        var vm = new PlaybackControlViewModel(new FakePlaybackService { CurrentEntry = MakeEntry() }, playlist);

        Assert.True(vm.BackwardCommand.CanExecute(null));
        vm.BackwardCommand.Execute(null);
        Dispatcher.UIThread.RunJobs();

        Assert.True(playlist.PlayPreviousCalled);
        Assert.Null(playlist.SelectedEntry);
    }

    [AvaloniaFact]
    public void NavigationCommands_Disabled_WithFewerThanTwoEntries()
    {
        var playlist = new FakePlaylistService();
        playlist.Add(MakeEntry("A"));
        var vm = new PlaybackControlViewModel(new FakePlaybackService { CurrentEntry = MakeEntry() }, playlist);

        Assert.False(vm.ForwardCommand.CanExecute(null));
        Assert.False(vm.BackwardCommand.CanExecute(null));
    }
}
