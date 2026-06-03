using System.Text;
using LedCube.Animation.FileFormat.Common.Exceptions;
using LedCube.Animation.FileFormat.Common.Io;

namespace LedCube.Animation.FileFormat.Test.Common;

public class FormatJsonTests
{
    [Fact]
    public void PeekFormatVersion_ReadsValue()
    {
        var json = Encoding.UTF8.GetBytes("{\"formatVersion\":3,\"name\":\"x\"}");
        Assert.Equal(3, FormatJson.PeekFormatVersion(json));
    }

    [Fact]
    public void PeekFormatVersion_MissingField_Throws()
    {
        var json = Encoding.UTF8.GetBytes("{\"name\":\"x\"}");
        Assert.Throws<InvalidFileFormatException>(() => FormatJson.PeekFormatVersion(json));
    }

    [Fact]
    public void PeekFormatVersion_NonNumber_Throws()
    {
        var json = Encoding.UTF8.GetBytes("{\"formatVersion\":\"two\"}");
        Assert.Throws<InvalidFileFormatException>(() => FormatJson.PeekFormatVersion(json));
    }

    [Fact]
    public void PeekFormatVersion_MalformedJson_Throws()
    {
        var json = Encoding.UTF8.GetBytes("{ not json ");
        Assert.Throws<InvalidFileFormatException>(() => FormatJson.PeekFormatVersion(json));
    }
}
