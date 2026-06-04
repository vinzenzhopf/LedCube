using System;
using LedCube.Streamer.Datagram;

namespace LedCube.Core.UI.Controls.StreamingControl;

public record CubeStreamingStatusSnapshot(
    string CubeVersion,
    string StreamerVersion,
    TimeSpan Ping,
    TimeSpan PingMean,
    TimeSpan FrameTimeCurrent,
    TimeSpan FrameTimeMean,
    TimeSpan FrameTime95Pct,
    TimeSpan FrameTime05Pct,
    TimeSpan MeasuredFrameTimeCurrent,
    TimeSpan MeasuredFrameTimeMean,
    long FrameNumber,
    AnimationStatus AnimationStatus,
    string CurrentAnimation,
    uint CurrentTicks,
    CubeErrorCode CubeErrorCode,
    bool ConnectionStable
)
{
    public CubeStreamingStatusSnapshot() : this(
        string.Empty, 
        string.Empty, 
        TimeSpan.Zero, 
        TimeSpan.Zero, 
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        TimeSpan.Zero,
        0,
        AnimationStatus.NotActive,
        string.Empty, 
        0, 
        CubeErrorCode.Ok, 
        false)
    {
    }
}