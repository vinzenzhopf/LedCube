# LedCube.Core.UI.Test

Headless Avalonia UI tests for `LedCube.Core.UI`. Runs real controls (XAML load, bindings,
styles, input, rendering) without a display, using `Avalonia.Headless` + Skia.

## How it works
- `TestAppBuilder.cs` configures a headless Avalonia app (`UseHeadlessDrawing = false` + `UseSkia`
  enables real rendering). The `[assembly: AvaloniaTestApplication(...)]` attribute wires it in.
- `App.axaml` mirrors `LedCube.Streamer.UI/App.axaml` so controls resolve the same theme/styles.
- Uses **xunit v3** (required by `Avalonia.Headless.XUnit` 12) — not v2 like the other test projects.

## Writing a test
Use `[AvaloniaFact]` / `[AvaloniaTheory]` (not `[Fact]`) so the body runs on the UI thread.
```csharp
[AvaloniaFact]
public void MyControl_DoesThing()
{
    var control = new MyControl { DataContext = vm };
    var window = new Window { Content = control };
    window.Show();
    Dispatcher.UIThread.RunJobs();        // pump layout/bindings
    // assert against the visual/logical tree, then optionally:
    window.CaptureRenderedFrame()!.Save("shot.png");   // screenshot for inspection
}
```
ViewModel-only logic can also be tested here; inject the fakes in `Fakes/`.

Screenshots land in `bin/Debug/net10.0/screenshots/`.
