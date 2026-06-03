namespace LedCube.PluginBase;

public record AnimationConfigDescriptor(
    string Key,
    string DisplayName,
    AnimationConfigType Type,
    object? DefaultValue = null,
    object? MinValue = null,
    object? MaxValue = null,
    string[]? EnumValues = null,
    string? Description = null,
    // Used by AnimationConfigType.FilePath: file extensions to filter on (without the dot,
    // e.g. ["lcanimraw"]). Null/empty means any file.
    string[]? FileExtensions = null);
