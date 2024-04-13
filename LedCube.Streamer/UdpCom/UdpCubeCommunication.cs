using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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
    
    public async IAsyncEnumerable<HostAndPort> SendBroadcastSearch(string version, int port, TimeSpan timeout, [EnumeratorCancellation] CancellationToken cts)
    {
        var payload = new InfoPayload()
        {
            Version = version
        };
        var responses = SendAndReceiveDatagramMultipleAsync(DatagramType.Discovery,
             DatagramExtensions.ToMemory(in payload), new HostAndPort("255.255.255.255", port),
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
        return await SendAndReceiveDatagramAsync(DatagramType.Info, payload, timeout, cts).ConfigureAwait(false);
    }
    
    public async Task<ReceivedDatagram?> SendStartAnimationAsync(uint frameTimeUs, string animationName, TimeSpan timeout,
        CancellationToken cts)
    {
        var payload = new AnimationStartPayload()
        {
            FrameTimeUs = frameTimeUs,
            AnimationName = animationName
        };
        return await SendAndReceiveDatagramAsync(DatagramType.AnimationStart, payload, timeout, cts).ConfigureAwait(false);
    }

    public async Task<ReceivedDatagram?> SendEndAnimationAsync(uint currentTicks, TimeSpan timeout,
        CancellationToken cts)
    {
        var payload = new AnimationEndPayload()
        {
            CurrentTicks = currentTicks
        }; 
        return await SendAndReceiveDatagramAsync(DatagramType.AnimationEnd,  payload, timeout, cts).ConfigureAwait(false);
    }

    public async Task<ReceivedDatagram?> SendFrameAsync(uint frameNumber, uint frameTimeUs, FramePayloadData frameData, 
        TimeSpan timeout, CancellationToken cts)
    {
        var payload = new FramePayload()
        {
            FrameNumber = frameNumber,
            FrameTimeUs = frameTimeUs,
            Data = frameData
        };
        return await SendAndReceiveDatagramAsync(DatagramType.FrameData, payload, timeout, cts).ConfigureAwait(false);
    }
}
