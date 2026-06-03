using Avalonia;
using Avalonia.Headless;
using LedCube.Core.UI.Test;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace LedCube.Core.UI.Test;

/// <summary>
/// Entry point the Avalonia.Headless.XUnit runner uses to spin up a headless Avalonia
/// app for every [AvaloniaFact]/[AvaloniaTheory]. UseHeadlessDrawing=false + UseSkia
/// gives real rendering so CaptureRenderedFrame() can produce PNG screenshots.
/// </summary>
public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
