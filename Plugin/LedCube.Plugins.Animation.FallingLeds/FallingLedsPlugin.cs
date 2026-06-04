using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.FallingLeds;

public class FallingLedsPlugin : IPlugin
{
    string IPlugin.Name => "Falling LEDs Animation";
    string IPlugin.Description => "A surface of LEDs that drains to the floor, then rises again.";
}
