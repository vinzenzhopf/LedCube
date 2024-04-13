using System.Diagnostics;
using System.Net;
using System.Threading;
using LedCube.Streamer.Datagram;

namespace LedCube.Streamer.UdpCom;

public interface IFrameData
{
    public byte[] GetData();
}

public record HostAndPort(string Hostname, int Port);

public record ReceivedDatagram(IPEndPoint Remote, CubeDatagramHeader Header, object? Payload)
{
    public long SendTicks { get; set; }
    public long ReceivedTicks { get; set; }

    public long PingTicks => ReceivedTicks - SendTicks;

    public double Ping => (double) PingTicks / Stopwatch.Frequency;
}

public record DatagramResponse()
{
    public CancellationTokenSource? PackageReceivedToken { get; set; }
    public ReceivedDatagram? Data { get; set; }
}

public record UnlistedMessageArgs(ReceivedDatagram Data);

public delegate void UnlistedMessageReceived(object? sender, UnlistedMessageArgs args);