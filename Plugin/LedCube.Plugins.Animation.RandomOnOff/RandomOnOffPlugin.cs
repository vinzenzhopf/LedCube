using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.RandomOnOff;

public class RandomOnOffPlugin : IPlugin
{
    string IPlugin.Name => "Random On/Off Animation";
    string IPlugin.Description => "Randomly fills the cube, then randomly clears it, on repeat.";
}
