namespace LedCube.Streamer.Datagram;

public enum AnimationStatus : byte
{
    NotActive       = 0b00000000,
    Running         = 0b00000001,
    PackageLost     = 0b00000100, // Network package lost, receive order wrong.
    FrameDropped    = 0b00001000, // Frame dropped, too many frames to show.
    FrameRedrawn    = 0b00010000, // Received too slow, frame has been redrawn.
};