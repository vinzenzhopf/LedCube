using LedCube.Core.Common.Model;

namespace LedCube.Streamer;

public record StreamerSettings(
    int LocalPort,
    int BroadcastPort,
    TimeSpan BroadcastTimeout,
    bool AlwaysSendFrames,
    CubeDrawRotation DrawRotation,
    Orientation3D ViewDirection,
    uint FrameTimeUs,
    uint SendIntervalUs,
    string StreamerVersion
)
{
    public static StreamerSettings Default => new(
            4242, 4242, 
            TimeSpan.FromSeconds(20), false,
            CubeDrawRotation.RightHandSide, Orientation3D.Front, 
            5000, 10000, 
            StreamerInfo.DataVersion 
        );
}