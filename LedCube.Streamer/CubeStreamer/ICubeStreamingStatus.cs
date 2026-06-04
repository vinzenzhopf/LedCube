using System;
using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.CubeStreamer;


public interface ICubeStreamingStatus
{
    string CubeVersion { get; }
    string StreamerVersion { get; }
    TimeSpan Ping { get; }
    TimeSpan PingMean { get; }
    // Cube-reported render time of the last frame (from the device ack).
    TimeSpan FrameTimeCurrent { get; }
    TimeSpan FrameTimeMean { get; }
    TimeSpan FrameTime95Pct { get; }
    TimeSpan FrameTime05Pct { get; }
    // Streamer-measured wall-clock interval between sent frames.
    TimeSpan MeasuredFrameTimeCurrent { get; }
    TimeSpan MeasuredFrameTimeMean { get; }
    long FrameNumber { get; }
    AnimationStatus AnimationStatus { get; }
    string CurrentAnimation { get; }
    UInt32 CurrentTicks { get; }
    CubeErrorCode CubeErrorCode { get; }
    bool ConnectionStable { get; }
}

public interface ICubeStreamingStatusMutable : ICubeStreamingStatus
{
    void UpdateFrameTime(AnimationStatus lastFrameTimeUs, uint payloadLastFrameTimeUs, uint payloadFrameNumber);

    void CommitTimings(long resultSendTicks, long resultReceivedTicks, uint? payloadReceivedTicks,
        uint payloadCurrentTicks);

    void CommitMissingResponse();

    void UpdateInfo(AnimationStatus payloadStatus, uint payloadLastFrameTimeUs, string payloadVersion,
        CubeErrorCode payloadErrorCode);

    /// <summary>Records a streamer-side measured interval between two consecutive sent frames.</summary>
    void CommitFrameInterval(double intervalUs);
}