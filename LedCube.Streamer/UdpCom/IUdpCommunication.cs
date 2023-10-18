using System.Net;
using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.UdpCom;

public interface IUdpCommunication : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the connection is established.
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Gets information about the remote host.
    /// </summary>
    HostAndPort RemoteHost { get; }
    
    /// <summary>
    /// Gets the current packet count.
    /// </summary>
    ushort CurrentPacketCount { get; }
    
    /// <summary>
    /// Occurs when an unlisted message is received.
    /// </summary>
    public event UnlistedMessageReceived? UnlistedMessageReceived;

    /// <summary>
    /// Connects to the specified remote host and port, ensuring the listener is listening for responses.
    /// </summary>
    /// <param name="hostname">The hostname of the remote client.</param>
    /// <param name="port">The port used to send packets.</param>
    /// <param name="token">A cancellation token to interrupt the connection procedure</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean 
    /// that is true if the connection is successfully established; otherwise false.</returns>
    Task<bool> ConnectAsync(string hostname, int port, CancellationToken token);

    /// <summary>
    /// Connects to the specified remote host and port, ensuring the listener is listening for responses.
    /// </summary>
    /// <param name="hostAndPort">The hostname and port of the remote client.</param>
    /// <param name="token">A cancellation token to interrupt the connection procedure</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean 
    /// that is true if the connection is successfully established; otherwise false.</returns>
    Task<bool> ConnectAsync(HostAndPort hostAndPort, CancellationToken token);
    

    /// <summary>
    /// Disconnects the client from a remote host
    /// </summary>
    /// <param name="token">A cancellation token that can be used to interrupt the method.</param>
    /// <returns>A Task representing the operation</returns>
    Task DisconnectAsync(CancellationToken token);


    /// <summary>
    /// Starts listening with the provided settings.
    /// </summary>
    /// <param name="localPort">The local port to listen on.</param>
    /// <param name="cts">The CancellationToken that stops listening.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean
    /// that is true if the listener has successfully started or restarted with the given settings; otherwise false.</returns>
    Task<bool> ReStartListeningAsync(int localPort, CancellationToken cts)
        => ReStartListeningAsync(localPort, IPAddress.Any, cts);

    /// <summary>
    /// Starts listening with the provided settings.
    /// </summary>
    /// <param name="localPort">The local port to listen on.</param>
    /// <param name="localAddress">The local ip address to bind to on.</param>
    /// <param name="cts">The CancellationToken that stops listening.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean
    /// that is true if the listener has successfully started or restarted with the given settings; otherwise false.</returns>
    Task<bool> ReStartListeningAsync(int localPort, IPAddress localAddress, CancellationToken cts);

    /// <summary>
    /// Stops listening for messages on the local UdpConnection
    /// </summary>
    /// <param name="token">A cancellation token that can be used to interrupt the method.</param>
    Task StopListeningAsync(CancellationToken token);
    
    /// <summary>
    /// Gets the current packet count and then increments it.
    /// </summary>
    /// <returns>The current packet count before it was incremented.</returns>
    public ushort GetAndIncrementPacketCount();
    
    public Task SendDatagramAsync(ushort packetCount, DatagramType type, ReadOnlyMemory<byte> dataSpan,
        CancellationToken cts);

    public Task SendDatagramAsync(ushort packetCount, DatagramType type, ReadOnlyMemory<byte> dataSpan,
        HostAndPort host, CancellationToken cts);

    public Task<ReceivedDatagram?> SendAndReceiveDatagramAsync(DatagramType type, ReadOnlyMemory<byte> dataSpan,
        TimeSpan timeout, CancellationToken cts);

    public Task<ReceivedDatagram?> SendAndReceiveDatagramAsync(DatagramType type, ReadOnlyMemory<byte> dataSpan,
        HostAndPort host, TimeSpan timeout, CancellationToken cts);

    public IAsyncEnumerable<ReceivedDatagram> SendAndReceiveDatagramMultipleAsync(DatagramType type,
        ReadOnlyMemory<byte> dataSpan,
        HostAndPort host, TimeSpan timeout, CancellationToken cts);
}