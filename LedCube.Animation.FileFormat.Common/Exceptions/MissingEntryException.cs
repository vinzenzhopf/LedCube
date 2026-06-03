namespace LedCube.Animation.FileFormat.Common.Exceptions;

/// <summary>
/// Thrown when a required ZIP entry (e.g. <c>manifest.json</c> or <c>frames.bin</c>) is absent.
/// </summary>
public sealed class MissingEntryException : InvalidFileFormatException
{
    public string EntryName { get; }

    public MissingEntryException(string entryName)
        : base($"Required archive entry '{entryName}' is missing.")
    {
        EntryName = entryName;
    }
}
