using System.Globalization;
using System.Text;

namespace LedCube.Animation.FileFormat.Test.Fixtures;

/// <summary>Builds raw manifest JSON strings so validation tests can craft malformed payloads.</summary>
internal static class ManifestJson
{
    public static string Build(
        int formatVersion = 1,
        string name = "Test",
        int sizeX = 4,
        int sizeY = 4,
        int sizeZ = 4,
        string ledFormat = "Binary",
        int frameCount = 10,
        long frameTimeUs = 20000,
        bool? loop = null,
        string keyframes = "[{\"at\":0,\"id\":0}]",
        string? extraFieldJson = null)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        sb.Append(CultureInfo.InvariantCulture, $"\"formatVersion\":{formatVersion},");
        sb.Append(CultureInfo.InvariantCulture, $"\"name\":{Quote(name)},");
        sb.Append(CultureInfo.InvariantCulture, $"\"size\":{{\"x\":{sizeX},\"y\":{sizeY},\"z\":{sizeZ}}},");
        sb.Append(CultureInfo.InvariantCulture, $"\"ledFormat\":{Quote(ledFormat)},");
        sb.Append(CultureInfo.InvariantCulture, $"\"frameCount\":{frameCount},");
        sb.Append(CultureInfo.InvariantCulture, $"\"frameTimeUs\":{frameTimeUs},");
        if (loop is not null)
        {
            sb.Append(CultureInfo.InvariantCulture, $"\"loop\":{(loop.Value ? "true" : "false")},");
        }

        if (extraFieldJson is not null)
        {
            sb.Append(extraFieldJson).Append(',');
        }

        sb.Append(CultureInfo.InvariantCulture, $"\"keyframes\":{keyframes}");
        sb.Append('}');
        return sb.ToString();
    }

    private static string Quote(string value) =>
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
