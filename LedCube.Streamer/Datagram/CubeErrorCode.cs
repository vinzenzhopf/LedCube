namespace LedCube.Streamer.Datagram;

public enum CubeErrorCode : UInt16
{
    Ok = 0x00,
    PackageOrder = 0x10,
    FrameOrder = 0x11,
    UnknownPackage = 0x20
};