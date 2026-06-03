using System.IO;
using System.IO.Compression;
using System.Text;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Common.Io;

namespace LedCube.Animation.FileFormat.Test.Common;

public class ZipEntriesTests
{
    [Fact]
    public void Write_Then_ReadRequired_RoundTrips()
    {
        var payload = Encoding.UTF8.GetBytes("hello cube");
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            ZipEntries.Write(archive, "data.bin", payload, CompressionLevel.Optimal);
        }

        ms.Position = 0;
        using var read = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.Equal(payload, ZipEntries.ReadRequired(read, "data.bin"));
    }

    [Fact]
    public void ReadOptional_MissingEntry_ReturnsNull()
    {
        using var ms = new MemoryStream();
        using (var _ = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
        }

        ms.Position = 0;
        using var read = new ZipArchive(ms, ZipArchiveMode.Read);
        Assert.Null(ZipEntries.ReadOptional(read, "absent.bin"));
    }

    [Fact]
    public void ReadRequired_MissingEntry_Throws()
    {
        using var ms = new MemoryStream();
        using (var _ = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
        }

        ms.Position = 0;
        using var read = new ZipArchive(ms, ZipArchiveMode.Read);
        var ex = Assert.Throws<MissingEntryException>(() => ZipEntries.ReadRequired(read, "absent.bin"));
        Assert.Equal("absent.bin", ex.EntryName);
    }
}
