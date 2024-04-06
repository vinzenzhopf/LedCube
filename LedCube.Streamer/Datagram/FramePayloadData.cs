using System.Runtime.CompilerServices;

namespace LedCube.Streamer.Datagram;

[InlineArray(Size)]
public struct FramePayloadData
{
    internal const int Size = 512;
    
    private byte _element0;
}