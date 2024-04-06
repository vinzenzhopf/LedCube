using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.UdpCom;

public interface IUdpCubeCommunication : IUdpCommunication
{
    
    public TimeSpan ReceiveTimeout { get; set; }

    /// <summary>
    /// Sends a discovery Datagram to the local Broadcast address and the specified remotePort, and returns all received hosts.
    /// <param name="remotePort">The remote Port to send the datagrams to.</param>
    /// <param name="timeout">The maximum search Timeout, after that the search is stopped and the results are returned.</param>
    /// <returns>Return all found results</returns>
    IAsyncEnumerable<HostAndPort> SendBroadcastSearch(string version, int port, TimeSpan timeout, CancellationToken cts);

    /// <summary>
    /// Sends an StatusRequest-Datagram to the remote host and waits for the acknowledge or returns with an timeout-Exception.
    /// </summary>
    /// <param name="version"></param>
    /// <param name="timeout"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    Task<ReceivedDatagram?> SendStatusRequestAsync(string version, TimeSpan timeout,
        CancellationToken cts);

    Task<ReceivedDatagram?> SendStatusRequestAsync(string version, CancellationToken cts)
        => SendStatusRequestAsync(version, ReceiveTimeout, cts);
    
    /// <summary>
    /// Sends an StartAnimation-Datagram to the remote host and waits for the acknowledge or returns with an timeout-Exception.
    /// </summary>
    /// <param name="frameTimeUs">frameTimeUs parameter in the Datagram</param>
    /// <param name="animationName">animationName parameter in the Datagram</param>
    /// <param name="timeout"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    Task<ReceivedDatagram?> SendStartAnimationAsync(uint frameTimeUs, string animationName, TimeSpan timeout,
        CancellationToken cts);

    Task<ReceivedDatagram?> SendStartAnimationAsync(uint frameTimeUs, string animationName,
        CancellationToken cts) => SendStartAnimationAsync(frameTimeUs, animationName, ReceiveTimeout, cts); 
    
    /// <summary>
    /// Sends an EndAnimation-Datagram to the remote host and waits for the acknowledge or returns with an timeout-Exception.
    /// </summary>
    /// <returns></returns>
    Task<ReceivedDatagram?> SendEndAnimationAsync(uint currentTicks, TimeSpan timeout, CancellationToken cts);

    Task<ReceivedDatagram?> SendEndAnimationAsync(uint currentTicks, CancellationToken cts)
        => SendEndAnimationAsync(currentTicks, ReceiveTimeout, cts);

    /// <summary>
    /// Sends a Frame package to the remote host and waits for the acknowledge or return with an timeout exception 
    /// </summary>
    /// <para name="updateTimeUs">The interval in which the frame data is sent.</para>
    /// <returns>When the sending started and the first acknowledge is returned, this method returns true</returns>
    Task<ReceivedDatagram?> SendFrameAsync(uint frameNumber, uint frameTimeUs, FramePayloadData frameData,
        TimeSpan timeout, CancellationToken cts);

    Task<ReceivedDatagram?> SendFrameAsync(uint frameNumber, uint frameTimeUs, FramePayloadData frameData,
        CancellationToken cts) => SendFrameAsync(frameNumber, frameTimeUs, frameData, ReceiveTimeout, cts);
}