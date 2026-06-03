using System;
using System.IO;
using System.Linq;
using System.Reflection;
using LedCube.Animation.DemoGen;

// Output dir: first arg, or ./demos-out next to the working directory.
var outputDir = args.Length > 0 ? args[0] : Path.Combine(Directory.GetCurrentDirectory(), "demos-out");
Directory.CreateDirectory(outputDir);

var demos = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => typeof(IDemo).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
    .Select(t => (IDemo)Activator.CreateInstance(t)!)
    .OrderBy(d => d.Name)
    .ToList();

if (demos.Count == 0)
{
    Console.Error.WriteLine("No IDemo implementations found.");
    return 1;
}

Console.WriteLine($"Writing {demos.Count} demo(s) to {outputDir}");
foreach (var demo in demos)
{
    var animation = demo.Build();
    var path = Path.Combine(outputDir, $"{demo.Name}.lcanimraw");
    using (var stream = File.Create(path))
    {
        LedCube.Animation.FileFormat.AnimationRaw.Io.LcAnimRawWriter.Write(stream, animation);
    }

    var bytes = new FileInfo(path).Length;
    Console.WriteLine($"  {demo.Name,-20} {animation.Manifest.FrameCount,4} frames  {bytes,7} bytes");
}

return 0;
