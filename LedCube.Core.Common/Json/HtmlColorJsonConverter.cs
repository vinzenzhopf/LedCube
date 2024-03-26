using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LedCube.Core.Common.Json;

public class HtmlColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(
        ref Utf8JsonReader reader, 
        Type typeToConvert, 
        JsonSerializerOptions options) => 
        ColorTranslator.FromHtml(reader.GetString()!);

    public override void Write(
        Utf8JsonWriter writer, 
        Color value, 
        JsonSerializerOptions options) =>
        writer.WriteStringValue(ColorTranslator.ToHtml(value));
}