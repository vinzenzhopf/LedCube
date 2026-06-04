using LedCube.PluginBase;

namespace LedCube.Plugins.Animation.BouncingBall;

public class BouncingBallPlugin : IPlugin
{
    string IPlugin.Name => "Bouncing Ball Animation";
    string IPlugin.Description => "A ball that bounces around inside the cube.";
}
