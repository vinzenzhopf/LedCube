using System;

namespace LedCube.Streamer.Datagram;

public enum DatagramType : UInt16
{
    Discovery = 0x10,
    Info = 0x20,
    InfoResponse = 0x21,
    ErrorResponse = 0x22,
    AnimationStart = 0x30,
    AnimationStartAck = 0x31,
    AnimationEnd = 0x32,
    AnimationEndAck = 0x33,
    FrameData = 0x40,
    FrameDataAck = 0x41,
}