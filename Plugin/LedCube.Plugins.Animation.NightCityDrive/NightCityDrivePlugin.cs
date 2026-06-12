using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.NightCityDrive;

public class NightCityDrivePlugin : IPlugin
{
    string IPlugin.Name => "Night City Drive";
    string IPlugin.Description => "A drive through a night city of block clusters, turning corners.";
}
