using System;
using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.CubeStreamer;


public interface ICubeStreamingStatus
{
    string CubeVersion { get; }
    string StreamerVersion { get; }
    TimeSpan Ping { get; }
    TimeSpan PingMean { get; }
    TimeSpan FrameTimeCurrent { get; }
    TimeSpan FrameTimeMean { get; }
    TimeSpan FrameTime95Pct { get; }
    TimeSpan FrameTime05Pct { get; }
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
}