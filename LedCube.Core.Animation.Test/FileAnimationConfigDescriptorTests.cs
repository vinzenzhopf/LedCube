using System.Linq;
using LedCube.PluginBase;
using LedCube.Plugins.Animation.FileAnimation;

namespace LedCube.Core.Animation.Test;

public class FileAnimationConfigDescriptorTests
{
    [Fact]
    public void Info_ExposesFilePathDescriptor()
    {
        var descriptor = FileAnimationGenerator.Info.ConfigDescriptors!.Single(d => d.Key == "FilePath");

        Assert.Equal(AnimationConfigType.FilePath, descriptor.Type);
        Assert.NotNull(descriptor.FileExtensions);
        Assert.Contains("lcanimraw", descriptor.FileExtensions!);
    }
}
