using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LedCube.Animation.FileFormat.Common.Exceptions;

namespace LedCube.Animation.FileFormat.Common.Io;

/// <summary>
/// Shared JSON configuration and helpers for the LedCube file-format manifests.
/// All formats use the same camelCase, null-omitting, indented conventions.
/// </summary>
public static class FormatJson
{
    /// <summary>The canonical serializer options for every LedCube manifest.</summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
    };

    public static T Deserialize<T>(ReadOnlyMemory<byte> utf8Json)
    {
        T? result;
        try
        {
            result = JsonSerializer.Deserialize<T>(utf8Json.Span, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidFileFormatException($"Manifest JSON could not be parsed: {ex.Message}", ex);
        }

        if (result is null)
        {
            throw new InvalidFileFormatException($"Manifest JSON deserialized to null for type {typeof(T).Name}.");
        }

        return result;
    }

    public static byte[] SerializeToUtf8Bytes<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, Options);

    /// <summary>
    /// Reads only the <c>formatVersion</c> field, so the reader can dispatch to the
    /// right versioned parser before doing a full deserialize.
    /// </summary>
    public static int PeekFormatVersion(ReadOnlyMemory<byte> utf8Json)
    {
        try
        {
            using var doc = JsonDocument.Parse(utf8Json);
            if (!doc.RootElement.TryGetProperty("formatVersion", out var version)
                || version.ValueKind != JsonValueKind.Number
                || !version.TryGetInt32(out var value))
            {
                throw new InvalidFileFormatException(
                    "Manifest is missing the required integer 'formatVersion' field.");
            }

            return value;
        }
        catch (JsonException ex)
        {
            throw new InvalidFileFormatException($"Manifest JSON could not be parsed: {ex.Message}", ex);
        }
    }
}
