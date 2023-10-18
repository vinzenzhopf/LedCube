using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using LedCube.Streamer.Datagram;
using Microsoft.Extensions.Logging;

namespace LedCube.Streamer.UdpCom;

public class UdpCommunication : IUdpCommunication
{
    private ILogger Logger { get; }
    private CancellationTokenSource? _listenerCancellationTokenSource;
    private Task? _listeningTask;

    protected UdpClient Client { get; private set; } = new UdpClient();

    private readonly object _packetCountLock = new object();
    private ushort _packetCount = 0;
    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    public ushort CurrentPacketCount => _packetCount;
    
    private readonly Stopwatch _stopwatch = new();
    private readonly ConcurrentDictionary<int, DatagramResponse> _datagramListeners = new();
    
    public bool IsConnected { get; private set; }
    public HostAndPort? RemoteHost { get; private set; }

    public event UnlistedMessageReceived? UnlistedMessageReceived;

    public UdpCommunication(ILoggerFactory loggerFactory)
    {
        _listenerCancellationTokenSource = null;
        RemoteHost = new HostAndPort("localhost", 4242);
        Logger = loggerFactory.CreateLogger(GetType());
    }
    
    protected virtual void OnUnlistedMessageReceived(UnlistedMessageArgs args)
    {
        UnlistedMessageReceived?.Invoke(this, args);
    }

    /// <summary>
    /// Connects the client to a remote UDP host
    /// </summary>
    /// <param name="hostname">The hostname of the remote host.</param>
    /// <param name="port">The port number of the remote host.</param>
    /// <param name="token">A cancellation token to interrupt the connection procedure</param>
    /// <returns>A Task representing the operation, with a boolea+n result indicating the success of the connection operation.</returns>
    /// <remarks>
    /// Once the client is connected, it updates the IsConnected property to true and sets the RemoteHost.
    /// </remarks>
    public Task<bool> ConnectAsync(string hostname, int port, CancellationToken token)
    {
        return ConnectAsync(new HostAndPort(hostname, port), token);
    }

    /// <summary>
    /// Connects the client to a remote UDP host
    /// </summary>
    /// <param name="hostAndPort">The hostname and port of the remote host.</param>
    /// <param name="token">A cancellation token to interrupt the connection procedure</param>
    /// <returns>A Task representing the operation, with a boolean result indicating the success of the connection operation.</returns>
    /// <remarks>
    /// Once the client is connected, it updates the IsConnected property to true and sets the RemoteHost.
    /// </remarks>
    public Task<bool> ConnectAsync(HostAndPort hostAndPort, CancellationToken token)
    {
        Client.Connect(hostAndPort.Hostname, hostAndPort.Port);
        RemoteHost = hostAndPort;
        IsConnected = true;
        _stopwatch.Reset();
        _stopwatch.Start();
        return Task.FromResult(true);
    }

    /// <summary>
    /// Disconnects the client from a remote host
    /// </summary>
    /// <param name="token">A cancellation token that can be used to interrupt the method.</param>
    /// <returns>A Task representing the operation</returns>
    public Task DisconnectAsync(CancellationToken token)
    {
        Client.Close();
        RemoteHost = null;
        IsConnected = false;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to restart the UDP listener on a specified port.
    /// </summary>
    /// <param name="localPort">The local port on which to start listening for incoming UDP datagrams.</param>
    /// <param name="localAddress">The local ip address to bind to on.</param>
    /// <param name="cts">A cancellation token that can be used to request the stoppage of the listening task.</param>
    /// <returns>A Task representing the operation, with a boolean result indicating the success of the restart operation.</returns>
    /// <remarks>
    /// If there is an ongoing listening task when this method is called, it will first be cancelled before a new one is started.
    /// </remarks>
    public async Task<bool> ReStartListeningAsync(int localPort, IPAddress localAddress, CancellationToken cts)
    {
        var tks = CancellationTokenSource.CreateLinkedTokenSource(cts);
        try
        {
            // Stop any ongoing listening task
            _listenerCancellationTokenSource?.Cancel();
            Interlocked.Exchange(ref _listenerCancellationTokenSource, tks)?.Cancel();
            if (_listeningTask is not null)
            {
                await _listeningTask;
                _listeningTask = null;
            }
            Client.Dispose();
        }
        catch (Exception e)
        {
            Logger.LogError(e, "RestartListening: Could not stop listener.");
        }
        Client = new UdpClient();
        Client.Client.Bind(new IPEndPoint(localAddress, localPort));
        
        _listeningTask = Task.Run(() => ReceiveMessagesAsync(tks.Token), tks.Token);
        return true;
    }

    /// <summary>
    /// Stops listening for messages on the local UdpConnection
    /// </summary>
    /// <param name="token">A cancellation token that can be used to interrupt the method.</param>
    public async Task StopListeningAsync(CancellationToken token)
    {
        Interlocked.Exchange(ref _listenerCancellationTokenSource, null)?.Cancel();
        if (_listeningTask is not null)
        {
            await _listeningTask;
            _listeningTask = null;
        }
    }
    
    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = await Client.ReceiveAsync(token);
                var datagram = CubeDatagramUtils.ResolveDatagramContent(result);

                if (_datagramListeners.TryGetValue(datagram.Header.PacketCount, out var listener))
                {
                    Logger.LogTrace("UDP Message received: Type={type}, Counter={counter} - Data={data}",
                        datagram.Header.PayloadType, datagram.Header.PacketCount, datagram.Payload);
                    datagram.ReceivedTicks = _stopwatch.ElapsedTicks;
                    listener.Data = datagram;
                    listener.PackageReceivedToken!.Cancel();
                }
                else
                {
                    Logger.LogTrace("UDP Unpaired datagram received: Type={type}, Counter={counter} - Data={data}",
                        datagram.Header.PayloadType, datagram.Header.PacketCount, datagram.Payload);
                    datagram.ReceivedTicks = _stopwatch.ElapsedTicks;
                    OnUnlistedMessageReceived(new UnlistedMessageArgs(datagram));
                }
            }
            catch (OperationCanceledException e)
            {
                Logger.LogInformation(e, "Message listener stopped.");
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode == 995)
                {
                    Logger.LogInformation(e, "Message listening stopped (SocketError 995).");
                }
                else
                {
                    Logger.LogError(e, "SocketException while receiving response from {host}", RemoteHost);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while receiving response from {host}", RemoteHost);
            }
        }
    }

    private ReadOnlyMemory<byte> BuildOutputBuffer(ushort packetCount, DatagramType type, ReadOnlySpan<byte> dataSpan)
    {
        var header = new CubeDatagramHeader()
        {
            PacketCount = packetCount,
            PayloadType = type
        };
        var headerSpan = CubeDatagramHeader.WriteToSpan(header);
        
        var totalLength = headerSpan.Length + dataSpan.Length;
        var buffer = new byte[totalLength].AsMemory();
        headerSpan.CopyTo(buffer.Span);
        dataSpan.CopyTo(buffer.Span[headerSpan.Length..]);
        return buffer[..totalLength];
    }

    /// <summary>
    /// This method increments the packet counter and returns the new value.
    /// </summary>
    /// <returns>The incremented packet counter.</returns>
    public ushort GetAndIncrementPacketCount()
    {
        lock(_packetCountLock)
        {
            return ++_packetCount;
        }
    }

    /// <summary>
    /// Sends a datagram asynchronously.
    /// </summary>
    /// <param name="packetCount">The number of the current packet to be sent.</param>
    /// <param name="type">The type of the datagram.</param>
    /// <param name="dataSpan">The data to be included in the datagram as a read-only memory byte.</param>
    /// <param name="cts">The CancellationToken that propagates notification that operations should be canceled.</param>
    /// <remarks>
    /// The method builds the outgoing datagram and sends its using the SendAsync method of the UdpClient.
    /// </remarks>
    public Task SendDatagramAsync(ushort packetCount, DatagramType type, ReadOnlyMemory<byte> dataSpan,
        CancellationToken cts)
        => SendDatagramAsync(packetCount, type, dataSpan, RemoteHost, cts);
    
    /// <summary>
    /// Sends a datagram asynchronously.
    /// </summary>
    /// <param name="packetCount">The number of the current packet to be sent.</param>
    /// <param name="type">The type of the datagram.</param>
    /// <param name="dataSpan">The data to be included in the datagram as a read-only memory byte.</param>
    /// <param name="host">The target host and port to which the datagram will be sent.</param>
    /// <param name="cts">The CancellationToken that propagates notification that operations should be canceled.</param>
    /// <remarks>
    /// The method builds the outgoing datagram and sends its using the SendAsync method of the UdpClient.
    /// </remarks>
    public async Task SendDatagramAsync(ushort packetCount, DatagramType type, ReadOnlyMemory<byte> dataSpan, 
        HostAndPort host, CancellationToken cts)
    {
        var outputBuffer = BuildOutputBuffer(packetCount, type, dataSpan.Span);
        await Client.SendAsync(outputBuffer, host.Hostname, host.Port, cts).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the provided datagram and waits for a response.
    /// </summary>
    /// <param name="type">The type of the datagram.</param>
    /// <param name="dataSpan">The data to be sent in the datagram.</param>
    /// <param name="timeout">The time span to wait for a response before timeout.</param>
    /// <param name="cts">Cancellation token source to cancel the operation.</param>
    /// <returns>The received datagram, or null if the request timeouts or an error occurs.</returns>
    /// <remarks>
    /// This async function firstly checks if the connection exists. If the connection does not exist, it throws an exception.
    /// Then, builds the message and sends it to the server asyncronously.
    /// It waits for the response until the specified timeout.
    /// If the response is not received within the timeout, it throws a timeout exception.
    /// If a message could be received it records the ticks when the datagram was sent and received.
    /// Finally, it returns the data of received datagram for further processing.
    /// </remarks>
    /// <exception cref="Exception">Thrown when the connection does not exist.</exception>
    /// <exception cref="TimeoutException">Thrown when the request timeouts and no response is received within the specified time span.</exception>
    public  Task<ReceivedDatagram?> SendAndReceiveDatagramAsync(DatagramType type, ReadOnlyMemory<byte> dataSpan,
        TimeSpan timeout, CancellationToken cts)
        => SendAndReceiveDatagramAsync(type, dataSpan, RemoteHost, timeout, cts);
    
    /// <summary>
    /// Sends the provided datagram and waits for a response.
    /// </summary>
    /// <param name="type">The type of the datagram.</param>
    /// <param name="dataSpan">The data to be sent in the datagram.</param>
    /// <param name="host">The targeted host to send the datagram.</param>
    /// <param name="timeout">The time span to wait for a response before timeout.</param>
    /// <param name="cts">A <see cref="CancellationToken"/> that can cancel the operation.</param>
    /// <returns>The received datagram, or null if the request timeouts or an error occurs.</returns>
    /// <remarks>
    /// This async function firstly checks if the connection exists. If the connection does not exist, it throws an exception.
    /// Then, builds the message and sends it to the server asyncronously.
    /// It waits for the response until the specified timeout.
    /// If the response is not received within the timeout, it throws a timeout exception.
    /// If a message could be received it records the ticks when the datagram was sent and received.
    /// Finally, it returns the data of received datagram for further processing.
    /// </remarks>
    /// <exception cref="Exception">Thrown when the connection does not exist.</exception>
    /// <exception cref="TimeoutException">Thrown when the request timeouts and no response is received within the specified time span.</exception>
    public async Task<ReceivedDatagram?> SendAndReceiveDatagramAsync(DatagramType type, ReadOnlyMemory<byte> dataSpan, 
        HostAndPort host, TimeSpan timeout, CancellationToken cts)
    {
        if (!IsConnected)
        {
            throw new Exception("Not connected.");
        }
        var listenerToken = CancellationTokenSource.CreateLinkedTokenSource(cts);
        var entry = new DatagramResponse()
        {
            PackageReceivedToken = listenerToken
        };
        var packetCount = GetAndIncrementPacketCount();
        try
        {
            _datagramListeners[packetCount] = entry;
            var outputBuffer = BuildOutputBuffer(packetCount, type, dataSpan.Span);
            long sendTicks = _stopwatch.ElapsedTicks;
            await Client.SendAsync(outputBuffer, cts).ConfigureAwait(false);
            try
            {
                await Task.Delay(timeout, entry.PackageReceivedToken.Token);
                throw new TimeoutException("Request timeout occurred. No Response datagram received in Time");
            }
            catch (OperationCanceledException)
            {
                entry.PackageReceivedToken = null;
                if (entry.Data is not null)
                {
                    entry.Data.SendTicks = sendTicks;
                    entry.Data.ReceivedTicks = _stopwatch.ElapsedTicks;
                }
            }
        }
        finally
        {
            listenerToken.Dispose();
            _datagramListeners.TryRemove(packetCount, out _);
        }
        return entry.Data;
    }

    /// <summary>
    /// Sends a datagram asynchronously and collects any responses received within a specified timeout period.
    /// </summary>
    /// <param name="type">The type of the datagram being sent.</param>
    /// <param name="dataSpan">The data to be sent.</param>
    /// <param name="host">The host to which the datagram is sent.</param>
    /// <param name="timeout">The period within which responses are awaited before the method returns.</param>
    /// <param name="cts">A <see cref="CancellationToken"/> that can cancel the operation.</param>
    /// <returns>
    /// A list of <see cref="ReceivedDatagram"/> objects that were received in response to the sent datagram.
    /// </returns>
    /// <remarks>
    /// The method registers a local method as message receiver and sends the datagram asyncronously.
    /// Then it waits for a specified period for any responses.
    /// Any responses received are added to the response list and are returned, after the timeout occures.
    /// </remarks>
    public async IAsyncEnumerable<ReceivedDatagram> SendAndReceiveDatagramMultipleAsync(DatagramType type, ReadOnlyMemory<byte> dataSpan,
        HostAndPort host, TimeSpan timeout, CancellationToken cts)
    {
        var packetCount = GetAndIncrementPacketCount();
        var responses = new ConcurrentQueue<ReceivedDatagram>();
        try
        {
            UnlistedMessageReceived += OnMessageReceived;
            await SendDatagramAsync(packetCount, type, dataSpan, host, cts).ConfigureAwait(false);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while ((stopwatch.ElapsedMilliseconds < timeout.TotalMilliseconds || !responses.IsEmpty) &&
                   !cts.IsCancellationRequested)
            {
                if (responses.TryDequeue(out var datagram))
                {
                    yield return datagram;
                }
                await Task.Delay(5, cts);
            }
        }
        finally
        {
            UnlistedMessageReceived -= OnMessageReceived;
        }

        yield break;

        void OnMessageReceived(object sender, UnlistedMessageArgs args)
        {
            if (args.Data.Header.PacketCount != packetCount) 
                return;
            responses.Enqueue(args.Data);
        }
    }

    public void Dispose()
    {
        _listenerCancellationTokenSource?.Cancel();
        Client.Close();
        Client.Dispose();
    }
}
