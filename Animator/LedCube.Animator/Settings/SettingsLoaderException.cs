using System;
using System.Runtime.Serialization;

namespace LedCube.Animator.Settings;

public class SettingsLoaderException : Exception
{
    public SettingsLoaderException()
    {
    }

    protected SettingsLoaderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public SettingsLoaderException(string? message) : base(message)
    {
    }

    public SettingsLoaderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}