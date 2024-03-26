using System.Net;
using LedCube.Streamer.UdpCom;

namespace LedCube.Streamer.CubeStreamer;

public interface ICubeStreamer
{
    StreamerSettings Settings { get; }
    public StreamingState StreamingState { get; }
    
    public Task<bool> ConnectAsync(int localPort, IPAddress localAddress, HostAndPort hostAndPort,
        CancellationToken token);
    
    public Task DisconnectAsync(CancellationToken token);
    
    public Task StartAnimationAsync(uint frameTimeUs, string animationName, CancellationToken token);

    public Task EndAnimationAsync(CancellationToken token);

    public Task StartStreaming(CancellationToken token);
    
    public Task StopStreaming(CancellationToken token);

    public ushort FrameCounter { get; }
    
    public string? CurrentAnimation { get; } 
}