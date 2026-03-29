namespace LedCube.PluginBase;

public record AnimationConfigDescriptor(
    string Key,
    string DisplayName,
    AnimationConfigType Type,
    object? DefaultValue = null,
    object? MinValue = null,
    object? MaxValue = null,
    string[]? EnumValues = null,
    string? Description = null);
