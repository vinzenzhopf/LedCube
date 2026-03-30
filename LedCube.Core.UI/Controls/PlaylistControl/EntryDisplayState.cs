namespace LedCube.Core.UI.Controls.PlaylistControl;

public enum EntryDisplayState
{
    None,    // not loaded
    Active,  // loaded but stopped — dot indicator
    Playing, // currently playing — play icon
    Paused,  // paused — pause icon
}
