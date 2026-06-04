using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.BouncingCubes;

public class BouncingCubesPlugin : IPlugin
{
    string IPlugin.Name => "Bouncing Cubes Animation";
    string IPlugin.Description => "Several wireframe cubes that bounce off the walls and off each other.";
}
