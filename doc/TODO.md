# TODO / Roadmap

## Streamer / Networking

- [ ] Network streaming: stabilize and fully integrate UDP streamer

## UI (LedCube.Streamer.UI)

- [x] Port to Avalonia 11.2 + FluentAvaloniaUI (replaces MahApps.Metro)
- [x] Establish style system (`General.axaml` tokens, `Components.axaml`
  icon-button/toolbar/muted classes, `Controls.axaml` with group-box
  HeaderedContentControl template and DataGrid theming)
- [x] LogAppender: DataGrid with resizable columns, level filter via
  flyout, debounced flush (`ConcurrentQueue` → 100 ms `DispatcherTimer`),
  `MaxEntries = 10 000`
- [ ] Move log severity brushes (`Brushes.Red`, `Brushes.DarkRed`, …
  in `LogAppenderControlSink`) into theme-aware resources
- [ ] Improve/Fix animation list/selection UX
- [ ] Extract stat label/value pairs in `StreamingControl` into a
  reusable component style (~18 inline duplicates today)

## Animator (LedCube.Animator)

- [ ] **TimelineControl** — scrubbing, multi-frame selection, comfortable frame editing
- [ ] **3D View** — fix and update; render cube properly, fix camera controls
- [ ] **Selection modes** — multiple selection modes for picking/editing LEDs in 3D view
- [ ] **2D plane selection** — finish plane-wise selection component
- [ ] **Edit/selection stack** — command pattern for undo/redo
- [ ] **Streamer module** — stream directly from Animator to cube
- [ ] General: get Animator to a working/usable level

## Plugin System

- [ ] True dynamic plugin loading — replace explicit project references
  with an MSBuild target that builds and copies plugin DLLs automatically,
  so any plugin dropped into the output folder is discovered without
  modifying the host app's `.csproj`

## Infrastructure / Misc

- [x] Review and tighten nullable usage across Solution
- [x] Fix/Improve App Configuration Handling
- [x] Remove/Combine Streamer.UI and Streamer.SmallUI
- [x] Port from WPF to Avalonia (all apps + TimelineControl.Demo;
  zero `<UseWPF>` projects remain)
- [ ] Delete orphaned `LedCube.Streamer.SmallUI/` directory (no csproj,
  not in solution, but source files linger)
- [ ] Resolve Changes from AnimationTestUI
  - AnimationList vs AnimationTest control, PreviewKeyDown/Up event handlers? This was probably used for the Snake Animation?)
- [ ] Add/Improve documentation

## Testing

- [ ] Add unit tests for core domain logic (`LedCube.Core.Common`)
- [ ] Add tests for plugin lifecycle (`FrameGeneratorBase`, `PluginManager`)
- [ ] Add tests for UDP communication / frame serialization
- [ ] Integration tests for playback loop

## Animations

- [ ] FFT Audio Spectrum animation (`AudioSpectrum` plugin — finish/improve)
- [ ] Rolling text animation (`TextWriter` plugin — scrolling text display)
- [ ] Extended animations (new ideas, more variety)
- [ ] Keyboard input integration for interactive animations
