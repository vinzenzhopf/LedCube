using System.Diagnostics;
using LedCube.Core.Common.Util;
using LedCube.Streamer.Datagram;
using Microsoft.Extensions.Logging;

namespace LedCube.Streamer.CubeStreamer;

public class CubeStreamingStatus : ICubeStreamingStatusMutable
{
    private ILogger _logger;

    public string CubeVersion { get; set; } = string.Empty;
    
    public string StreamerVersion { get; set; } = string.Empty;

    public TimeSpan Ping => TimeSpan.FromMilliseconds(_pingStatsMs.Last);
    
    public TimeSpan PingMean => TimeSpan.FromMilliseconds(_pingStatsMs.Mean);
    
    public TimeSpan FrameTimeCurrent => TimeSpan.FromMilliseconds(_frameTimeStatsMs.Last);
    
    public TimeSpan FrameTimeMean => TimeSpan.FromMilliseconds(_frameTimeStatsMs.Mean);
    
    public TimeSpan FrameTime95Pct => TimeSpan.FromMilliseconds(_frameTimeStatsMs.Pct95);
    
    public TimeSpan FrameTime05Pct => TimeSpan.FromMilliseconds(_frameTimeStatsMs.Pct05);

    public long FrameNumber { get; set; }
    
    public AnimationStatus AnimationStatus { get; set; }
    
    public string CurrentAnimation { get; set; } = string.Empty;
    
    public UInt32 CurrentTicks { get; set; }
    
    public CubeErrorCode CubeErrorCode { get; set; }
    
    public bool ConnectionStable { get; set; }
    

    private StatisticList _frameTimeStatsMs = new(100);
    
    private StatisticList _pingStatsMs = new(100);
    
    public CubeStreamingStatus(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public void UpdateFrameTime(AnimationStatus payloadStatus, uint payloadLastFrameTimeUs, uint payloadFrameNumber)
    {
        ConnectionStable = true;
        AnimationStatus = payloadStatus;
        FrameNumber = (int) payloadFrameNumber;
        _frameTimeStatsMs.AddValue(payloadLastFrameTimeUs);
        // _logger.LogDebug("UpdateFrameTime: payloadStatus={status}, payloadLastFrameTimeUs={LastFrameTime}, payloadFrameNumber={FrameNumber}", 
        //     payloadStatus, payloadLastFrameTimeUs, payloadFrameNumber);
    }

    public void CommitTimings(long resultSendTicks, long resultReceivedTicks, uint? payloadReceivedTicks, uint payloadCurrentTicks)
    {
        ConnectionStable = true;
        CurrentTicks = payloadCurrentTicks;
        var ping = (double)(resultReceivedTicks - resultSendTicks) / Stopwatch.Frequency;
        _pingStatsMs.AddValue(ping * 1000);
        // _logger.LogDebug("CommitTimings: resultSendTicks={resultSendTicks}, payloadReceivedTicks={payloadReceivedTicks}, payloadCurrentTicks={payloadCurrentTicks}", 
        //     resultSendTicks, payloadReceivedTicks, payloadCurrentTicks);
    }

    public void CommitMissingResponse()
    {
        ConnectionStable = false;
        // _logger.LogDebug("CommitMissingResponse:");
    }

    public void UpdateInfo(AnimationStatus payloadStatus, uint payloadLastFrameTimeUs, string payloadVersion,
        CubeErrorCode payloadErrorCode)
    {
        ConnectionStable = true;
        AnimationStatus = payloadStatus;
        CubeErrorCode = payloadErrorCode;
        CubeVersion = payloadVersion;
        _frameTimeStatsMs.AddValue(payloadLastFrameTimeUs);
        
        // _logger.LogDebug("UpdateInfo: payloadStatus={payloadStatus}, payloadLastFrameTimeUs={payloadLastFrameTimeUs}, payloadVersion={payloadVersion}, payloadErrorCode={payloadErrorCode}", 
        //     payloadStatus, payloadLastFrameTimeUs, payloadVersion, payloadErrorCode);
    }
}
