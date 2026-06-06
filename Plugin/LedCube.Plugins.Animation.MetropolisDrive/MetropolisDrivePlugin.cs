using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.MetropolisDrive;

public class MetropolisDrivePlugin : IPlugin
{
    string IPlugin.Name => "Metropolis Drive";
    string IPlugin.Description => "A quiet drive on a Metropolis road.";
}