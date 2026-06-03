using System;

namespace LedCube.Animation.FileFormat.Common.Exceptions;

/// <summary>
/// Base type for all errors raised while reading or writing a LedCube file format.
/// </summary>
public class FileFormatException : Exception
{
    public FileFormatException(string message) : base(message)
    {
    }

    public FileFormatException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
