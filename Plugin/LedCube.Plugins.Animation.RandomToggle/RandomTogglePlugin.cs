using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.RandomToggle;

public class RandomTogglePlugin : IPlugin
{
    string IPlugin.Name => "Random Toggle Animation";
    string IPlugin.Description => "Continuously toggles random LEDs on and off.";
}
