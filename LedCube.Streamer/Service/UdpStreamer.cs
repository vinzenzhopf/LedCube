using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using LedCube.Streamer.Datagram;
using Microsoft.Extensions.Logging;

namespace LedCube.Streamer.Service;

public record DatagramListener(Task Listener, CancellationTokenSource PackageReceivedToken)
{
    public ReceivedDatagram? Data { get; set; }
}

public class UdpStreamer : IUdpStreamer
{
    private ILogger Logger { get; }
    private readonly UdpClient _udpClient;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationTokenSource _listenerCancellationTokenSource;
    private Task? _listeningTask;
    
    private ConcurrentDictionary<int, DatagramListener> _datagramListeners;
    
    public bool IsConnected { get; private set; }
    public HostAndPort RemoteHost { get; private set; }
    public RemoteInfo RemoteInfo { get; }
    public string CurrentAnimation { get; }
    public StreamerSettings Settings { get; private set; }
    public IFrameData FrameData { get; set; }
    public bool FrameTransmissionActive { get; }
    public int CurrentPingTimeMs { get; }

    public UdpStreamer(LoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger(GetType());
    }

    public Task<bool> Connect(string hostname, int port)
    {
        _udpClient.Connect(hostname, port);
        IsConnected = true;
        RemoteHost = new HostAndPort(hostname, port);
        return Task.FromResult(true);
    }

    public async Task<bool> ReStartListeningAsync(StreamerSettings settings, CancellationToken stoppingToken)
    {
        // Stop any ongoing listening task
        if (_listeningTask is not null)
        {
            try
            {
                _listenerCancellationTokenSource.Cancel();
                await _listeningTask;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "RestartListening: Could not stop listener.");
            }
        }
        Settings = settings;
        _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, Settings.LocalPort));

        _listenerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var token = _listenerCancellationTokenSource.Token;
        _listeningTask = Task.Run(() => ReceiveMessagesAsync(token), token);
        return true;
    }
    
    private async Task ReceiveMessagesAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync(token);
                var datagram = result.ResolveDatagramContent();
                
                if(_datagramListeners.TryGetValue(datagram.Header.PacketCount, out var listener))
                {
                    Logger.LogTrace("UDP Message received: Type={type}, Counter={counter} - Data={data}",
                        datagram.Header.PayloadType, datagram.Header.PacketCount, datagram.Payload);
                    listener.Data = datagram;
                    listener.PackageReceivedToken.Cancel();
                }
                else
                {
                    Logger.LogTrace("UDP Unpaired datagram received: Type={type}, Counter={counter} - Data={data}",
                        datagram.Header.PayloadType, datagram.Header.PacketCount, datagram.Payload);    
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
    
    private async Task SendDatagramAsync(ushort packetCount, DatagramType type, ReadOnlyMemory<byte> dataSpan, CancellationToken cts)
    {
        var outputBuffer = BuildOutputBuffer(packetCount, type, dataSpan.Span);
        await _udpClient.SendAsync(outputBuffer, RemoteHost.Hostname, RemoteHost.Port, cts).ConfigureAwait(false);
    }
    
    public Task<List<HostAndPort>> SendBroadcastSearch(int remotePort, TimeSpan timeout)
    {
        throw new NotImplementedException();
    }

    public Task<bool> StartAnimationAsync(int frameTimeUs, string animationName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EndAnimationAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> StartSendingAsync(int updateTimeUs)
    {
        throw new NotImplementedException();
    }

    public bool StopSendingAsync()
    {
        throw new NotImplementedException();
    }
}