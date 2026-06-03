namespace LedCube.PluginBase;

public enum AnimationConfigType
{
    String,
    Int,
    Float,
    Bool,
    Enum,

    /// <summary>A file-system path. The UI renders a text box plus a "browse" file picker.</summary>
    FilePath
}
