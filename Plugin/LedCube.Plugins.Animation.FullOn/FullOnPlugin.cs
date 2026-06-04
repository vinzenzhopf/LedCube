using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.FullOn;

public class FullOnPlugin : IPlugin
{
    string IPlugin.Name => "Full On Animation";
    string IPlugin.Description => "Turns every LED of the cube on.";
}
