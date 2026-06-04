using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.BouncingCube;

public class BouncingCubePlugin : IPlugin
{
    string IPlugin.Name => "Bouncing Cube Animation";
    string IPlugin.Description => "A wireframe cube that breathes and tumbles, rendered via SDF.";
}
