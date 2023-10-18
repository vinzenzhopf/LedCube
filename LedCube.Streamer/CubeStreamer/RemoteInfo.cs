namespace LedCube.Streamer.CubeStreamer;

public record RemoteInfo(
    string Version,
    UInt32 LastFrameTimeUs,
    UInt32 CurrentTicks,
    int ErrorCode,
    int Status,
    string ActiveAnimation
);