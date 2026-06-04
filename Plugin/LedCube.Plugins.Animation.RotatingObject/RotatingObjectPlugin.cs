using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.RotatingObject;

public class RotatingObjectPlugin : IPlugin
{
    string IPlugin.Name => "Rotating Object Animation";
    string IPlugin.Description => "A solid shape that tumbles inside the cube, rendered via SDF.";
}
