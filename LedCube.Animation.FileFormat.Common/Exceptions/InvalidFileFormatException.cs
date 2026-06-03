using System;

namespace LedCube.Animation.FileFormat.Common.Exceptions;

/// <summary>
/// Thrown when a file is structurally present but violates the format's rules
/// (missing required fields, failed constraint validation, corrupt payload sizes, ...).
/// </summary>
public class InvalidFileFormatException : FileFormatException
{
    public InvalidFileFormatException(string message) : base(message)
    {
    }

    public InvalidFileFormatException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
