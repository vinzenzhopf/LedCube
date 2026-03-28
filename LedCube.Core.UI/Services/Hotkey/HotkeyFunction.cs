using System.Windows.Input;

namespace LedCube.Core.UI.Services.Hotkey;

public enum HotkeyFunction
{
    [Hotkey("Timeline.Playback.PlayPause", "Play/Pause", Key.Space)]
    TimelinePlaybackPlayPause,

    [Hotkey("Timeline.Playback.Stop", "Stop", Key.Back)]
    TimelinePlaybackStop,

    [Hotkey("Timeline.Playback.FrameForward", "Frame Forward", Key.Right)]
    TimelinePlaybackFrameForward,

    [Hotkey("Timeline.Playback.FrameBackward", "Frame Backward", Key.Left)]
    TimelinePlaybackFrameBackward,

    [Hotkey("Timeline.Zoom.In", "Zoom In", InputType.MouseScrollUp, ModifierKeys.Control)]
    TimelineZoomIn,

    [Hotkey("Timeline.Zoom.Out", "Zoom Out", InputType.MouseScrollDown, ModifierKeys.Control)]
    TimelineZoomOut,

    [Hotkey("Timeline.Zoom.Reset", "Reset Zoom", Key.NumPad0, ModifierKeys.Control)]
    TimelineZoomReset,

    [Hotkey("Timeline.Selection.Delete", "Delete Selection", Key.Delete)]
    TimelineSelectionDelete,

    [Hotkey("Viewport.Pan", "Pan Viewport", ExtendedMouseButton.Middle)]
    ViewportPan,

    [Hotkey("Viewport.Select", "Select", ExtendedMouseButton.Left)]
    ViewportSelect,

    [Hotkey("Viewport.Context", "Context Menu", ExtendedMouseButton.Right)]
    ViewportContextMenu,

    [Hotkey("Navigation.Back", "Navigate Back", ExtendedMouseButton.XButton1)]
    NavigationBack,

    [Hotkey("Navigation.Forward", "Navigate Forward", ExtendedMouseButton.XButton2)]
    NavigationForward
}