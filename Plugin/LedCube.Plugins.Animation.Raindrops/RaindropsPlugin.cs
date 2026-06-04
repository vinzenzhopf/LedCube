using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.Raindrops;

public class RaindropsPlugin : IPlugin
{
    string IPlugin.Name => "Raindrops Animation";
    string IPlugin.Description => "Drops fall from the top and gradually wet the floor.";
}
