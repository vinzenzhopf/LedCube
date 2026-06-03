using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace LedCube.Animation.FileFormat.Test.Fixtures;

/// <summary>Hand-assembles and inspects raw <c>.lcanimraw</c> ZIP containers for low-level tests.</summary>
internal static class RawZip
{
    public static MemoryStream FromEntries(params (string Name, byte[] Data)[] entries)
    {
        var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, data) in entries)
            {
                var entry = archive.CreateEntry(name, CompressionLevel.Optimal);
                using var stream = entry.Open();
                stream.Write(data, 0, data.Length);
            }
        }

        ms.Position = 0;
        return ms;
    }

    public static (string Name, byte[] Data) Text(string name, string content) =>
        (name, Encoding.UTF8.GetBytes(content));

    public static (string Name, byte[] Data) Bytes(string name, byte[] data) => (name, data);

    public static byte[] ReadEntry(byte[] zipBytes, string name)
    {
        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        var entry = archive.GetEntry(name)
                    ?? throw new InvalidOperationException($"ZIP has no entry '{name}'.");
        using var stream = entry.Open();
        using var outMs = new MemoryStream();
        stream.CopyTo(outMs);
        return outMs.ToArray();
    }

    public static (long Compressed, long Uncompressed) EntrySizes(byte[] zipBytes, string name)
    {
        using var ms = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        var entry = archive.GetEntry(name)
                    ?? throw new InvalidOperationException($"ZIP has no entry '{name}'.");
        return (entry.CompressedLength, entry.Length);
    }
}
