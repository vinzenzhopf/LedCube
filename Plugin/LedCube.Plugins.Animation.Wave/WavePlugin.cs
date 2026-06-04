using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.Wave;

public class WavePlugin : IPlugin
{
    string IPlugin.Name => "Wave Animation";
    string IPlugin.Description => "A travelling wave; LEDs below the surface are lit, above are off.";
}
