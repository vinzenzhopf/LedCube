# LED Cube

> Early development stage.

A .NET 10 solution for creating, editing, and streaming animations to a physical LED cube over UDP. Supports up to 16×16×16 LEDs.

## Projects

| Project                    | Type        | Description                                                              |
|----------------------------|-------------|--------------------------------------------------------------------------|
| `LedCube.Core.Common`      | Library     | Domain models: `ICubeData`, `CubeRepository`, `CubeConfig`               |
| `LedCube.Core.UI`          | UI-Library  | Shared WPF controls, `PlaybackService`, MVVM base                        |
| `LedCube.Streamer`         | Library     | UDP communication with the physical cube                                 |
| `LedCube.Animator`         | WPF-App     | WPF editor for creating and saving static animations                     |
| `LedCube.Streamer.UI`      | WPF-App     | Main WPF application — loads plugins, controls playback, streams to cube |
| `LedCube.Streamer.DebugUI` | WPF-App     | Small App test and Debug the UDP-Communiation with the LED cube          |
| `LedCube.Streamer.Console` | Console-App | Headless console runner                                                  |
| `LedCube.Sdf.Core`         | Library     | Signed Distance Field library for procedural 3D rendering                |
| `LedCube.PluginBase`       | Library     | `IPlugin` / `IFrameGenerator` contracts                                  |
| `LedCube.PluginHost`       | Library     | Runtime plugin discovery and loading                                     |

## Plugins

Animations are loaded as plugins at runtime. Included:

- `GameOfLife` — Conway's Game of Life in 3D
- `LedWalker` — LED walker effect
- `Snake3D` — 3D snake game
- `AudioSpectrum` — FFT audio visualizer
- `TextWriter` — Scrolling text
- `SdfTest` — SDF shape renderer

Each plugin implements `IPlugin` and one or more `IFrameGenerator` subclasses. `DrawFrame` is called every frame with a `FrameContext` containing the target buffer and timing info.

## Data Flow

```
IFrameGenerator.DrawFrame → ICubeData → CubeStreamerService → UDP → Physical Cube
```

Frame data is serialized bit-packed (1 bit per LED) and sent via UDP. The streamer handles discovery, animation start/end handshake, and frame acknowledgement.

## Build & Run

```bash
dotnet build LedCube.sln
dotnet run --project LedCube.Streamer.UI/LedCube.Streamer.UI.csproj
dotnet test LedCube.sln
```
