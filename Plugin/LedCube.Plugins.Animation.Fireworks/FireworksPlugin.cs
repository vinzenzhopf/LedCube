using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.Fireworks;

public class FireworksPlugin : IPlugin
{
    string IPlugin.Name => "Fireworks Animation";
    string IPlugin.Description => "Rockets launch from the floor and burst into falling sparks.";
}
