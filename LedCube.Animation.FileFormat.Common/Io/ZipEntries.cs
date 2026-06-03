using System;
using System.IO;
using System.IO.Compression;
using LedCube.Animation.FileFormat.Common.Exceptions;

namespace LedCube.Animation.FileFormat.Common.Io;

/// <summary>
/// Thin helpers for reading and writing byte payloads to ZIP container entries.
/// </summary>
public static class ZipEntries
{
    /// <summary>Reads a required entry's uncompressed bytes, throwing if it is absent.</summary>
    public static byte[] ReadRequired(ZipArchive archive, string name)
    {
        var entry = archive.GetEntry(name) ?? throw new MissingEntryException(name);
        return ReadAll(entry);
    }

    /// <summary>Reads an optional entry's uncompressed bytes, returning null if absent.</summary>
    public static byte[]? ReadOptional(ZipArchive archive, string name)
    {
        var entry = archive.GetEntry(name);
        return entry is null ? null : ReadAll(entry);
    }

    public static byte[] ReadAll(ZipArchiveEntry entry)
    {
        var capacity = entry.Length is > 0 and <= int.MaxValue ? (int)entry.Length : 0;
        using var stream = entry.Open();
        using var ms = new MemoryStream(capacity);
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>Creates an entry and writes <paramref name="data"/> using the given compression level.</summary>
    public static void Write(ZipArchive archive, string name, ReadOnlySpan<byte> data, CompressionLevel level)
    {
        var entry = archive.CreateEntry(name, level);
        using var stream = entry.Open();
        stream.Write(data);
    }
}
