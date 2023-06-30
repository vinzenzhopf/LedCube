using System.Net;

namespace LedCube.Streamer.Datagram;

public record ReceivedDatagram(IPEndPoint Remote, CubeDatagramHeader Header, object? Payload);