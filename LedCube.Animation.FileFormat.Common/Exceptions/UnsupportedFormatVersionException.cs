namespace LedCube.Animation.FileFormat.Common.Exceptions;

/// <summary>
/// Thrown when a file declares a <c>formatVersion</c> newer than this build can read.
/// Best-effort reading of future versions is intentionally not attempted.
/// </summary>
public sealed class UnsupportedFormatVersionException : FileFormatException
{
    public int FileVersion { get; }
    public int MaxSupportedVersion { get; }

    public UnsupportedFormatVersionException(int fileVersion, int maxSupportedVersion)
        : base($"This file was written by a newer version of LedCube " +
               $"(format version {fileVersion}); this build supports up to version {maxSupportedVersion}.")
    {
        FileVersion = fileVersion;
        MaxSupportedVersion = maxSupportedVersion;
    }
}
