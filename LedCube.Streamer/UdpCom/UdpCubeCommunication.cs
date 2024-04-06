using LedCube.Streamer.Datagram;
using Microsoft.Extensions.Logging;

namespace LedCube.Streamer.UdpCom;

public class UdpCubeCubeCommunication : UdpCommunication, IUdpCubeCommunication
{
    private ILogger Logger { get; }
    
    public TimeSpan ReceiveTimeout { get; set; } = TimeSpan.FromSeconds(3);

    public UdpCubeCubeCommunication(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }
    
    public async IAsyncEnumerable<HostAndPort> SendBroadcastSearch(string version, int port, TimeSpan timeout, CancellationToken cts)
    {
        var payload = new InfoPayload()
        {
            Version = version
        };
        var responses = SendAndReceiveDatagramMultipleAsync(DatagramType.Discovery,
            InfoPayload.WriteToMemory(payload), new HostAndPort("255.255.255.255", port),
            timeout, cts).ConfigureAwait(false);
        await foreach (var x in responses)
        {
            yield return new HostAndPort(x.Remote.Address.ToString(), x.Remote.Port);   
        }
    }

    public async Task<ReceivedDatagram?> SendStatusRequestAsync(string version, TimeSpan timeout,
        CancellationToken cts)
    {
        var payload = new InfoPayload()
        {
            Version = version
        };
        return await SendAndReceiveDatagramAsync(DatagramType.Info, InfoPayload.WriteToMemory(payload),
            timeout, cts).ConfigureAwait(false);
    }
    
    public async Task<ReceivedDatagram?> SendStartAnimationAsync(uint frameTimeUs, string animationName, TimeSpan timeout,
        CancellationToken cts)
    {
        var payload = new AnimationStartPayload()
        {
            FrameTimeUs = frameTimeUs,
            AnimationName = animationName
        };
        return await SendAndReceiveDatagramAsync(DatagramType.AnimationStart, AnimationStartPayload.WriteToMemory(payload),
            timeout, cts).ConfigureAwait(false);
    }

    public async Task<ReceivedDatagram?> SendEndAnimationAsync(uint currentTicks, TimeSpan timeout,
        CancellationToken cts)
    {
        var payload = new AnimationEndPayload()
        {
            CurrentTicks = currentTicks
        };
        return await SendAndReceiveDatagramAsync(DatagramType.AnimationEnd, AnimationEndPayload.WriteToMemory(payload),
            timeout, cts).ConfigureAwait(false);
    }

    public async Task<ReceivedDatagram?> SendFrameAsync(uint frameNumber, uint frameTimeUs, Memory<byte> frameData, 
        TimeSpan timeout, CancellationToken cts)
    {
        var payload = new FramePayload()
        {
            FrameNumber = frameNumber,
            FrameTimeUs = frameTimeUs,
            Data = frameData
        };
        return await SendAndReceiveDatagramAsync(DatagramType.FrameData, FramePayload.WriteToMemory(payload),
            timeout, cts).ConfigureAwait(false);
    }
}
