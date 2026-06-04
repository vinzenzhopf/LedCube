# TODO / Roadmap

## Streamer / Networking

- [ ] Network streaming: stabilize and fully integrate UDP streamer

## UI (LedCube.Streamer.UI)

- [x] Port to Avalonia 11.2 + FluentAvaloniaUI (replaces MahApps.Metro)
- [x] Establish style system (`General.axaml` tokens, `Components.axaml` icon-button/toolbar/muted classes, 
  `Controls.axaml` with group-box HeaderedContentControl template and DataGrid theming)
- [x] LogAppender: DataGrid with resizable columns, level filter via flyout, debounced flush 
  (`ConcurrentQueue` → 100 ms `DispatcherTimer`), `MaxEntries = 10 000`
- [x] Move log severity brushes into theme-aware resources (`General.axaml` 
  ThemeDictionaries + `LogLevelToBrushConverter`; `LogEntry` no longer carries an `IBrush`)
- [x] Extract stat label/value pairs in `StreamingControl` into a reusable `StatRow` `TemplatedControl`
- [ ] Show AnimationFile Infos in the Playlist View of selected Animation, including name, description, author, optionally image and so on.
- [x] Move forward through the playlist when the end of an animation is reached
- [x] When manually Reving through the Playlist with the Rewind or FastForward Buttons in 
  the PlaybackControl, the selected index in the Playlist is changed, but not the active playing entry. 
  The Selection in the Playlist should not be updated that way.
  (Rewind/FastForward now load+play the prev/next entry relative to the *playing* entry, leaving selection untouched.)
- [x] Respect the per-entry Repeat setting when playing a playlist
  - The per-entry `RepeatCount` is honoured by `PlaybackService` (0 = repeat that entry indefinitely; N = play N times before advancing).
- [x] Stop the Playing if the end of an Playlist is reached
  - Covered by the `StopAtEnd` playlist repeat mode (default).
- [x] Playlist repeat mode — cycle button in PlaybackControl: StopAtEnd, LoopWholePlaylist (default),
  RepeatCurrentEntry, FairRandomPlay (fair shuffle — all entries play before repeating), TrueRandomPlay.
  - [ ] Persist the selected playlist repeat mode across sessions (currently in-memory only).
- [x] The FPS counter in the PlaybackControl needs to be formatted to 2 decimal places

### Animation List
- Instead of referencing AnimationsFiles directy with Path, i would like to have a List of available AnimationFiles, like in an Music Player.
- [ ] Define Paths in the Settings, where AnimationFiles can be found and discoverd.
- [ ] Discover these Paths, maybe on startup, can be triggered by an tool bar entry. Build an Index of all AnimationFiles found.
- [ ] Show the AnimationFiles in a List, with all their infos, like in an music player. (Maybe adda third Column in the MainView for that)
- [ ] Allow the adding of those animation files to the Playlist.
- [ ] Make the Animation List search-/filterable

### Playlist File Format
- [ ] Nail down the specification for Playlist Files in [File Formats](FileFormats.md)
- [ ] Implement the File Format for the Playlist as outlined in the spec.
- [ ] Allow the user to save the current Playlist state into the Playlist file.
- [ ] Introduce Playlist Metadata like Name, Description, Author, Image.
  - [ ] Add an UI Control for editing the Metadata

### Media Directory
- [ ] Allow the user to select a directory containing 'media' files 
  - with one folder for animations, and one folder for playlists
  - ref to #AnimationList issue 1
  - automatically store the Playlists in this folder
- [ ] Add Separate List with discovered Animations from that Directory
  - Let the User freely browse through the Playlists and select one to play
  - Just like an Music Player

## Animator (LedCube.Animator)

- [ ] **TimelineControl** — scrubbing, multi-frame selection, comfortable frame editing
- [ ] **TimelineControl render thread-safety** — `TimelineRenderer` reads the live `TimelineState`
  on the Skia render thread while it is mutated on the UI thread (bindings like `FrameTime`,
  `LoopStart/End` flipping to null during playlist auto-advance). This caused render-loop
  `NullReference`/`Nullable must have a value` exceptions (swallowed by Avalonia's compositor, only
  visible in the debugger). Worked around with per-field snapshots in `DrawRulerLabels`/`DrawLoopRegion`;
  the proper fix is to pass an immutable render snapshot of `TimelineState` into `Draw` so the render
  thread never reads the live mutable object.
- [ ] **3D View** — fix and update; render cube properly, fix camera controls
- [ ] **Selection modes** — multiple selection modes for picking/editing LEDs in 3D view
- [ ] **2D plane selection** — finish plane-wise selection component
- [ ] **Edit/selection stack** — command pattern for undo/redo
- [ ] **Streamer module** — stream directly from Animator to cube
- [ ] General: get Animator to a working/usable level

### Export as Baked Animation
- [ ] Export the current TimelineState as a baked AnimationFile

### Animation Project File Format
- [ ] Nail down the specification for AnimationProject Files in [File Formats](FileFormats.md)
- [ ] Implement the File Format for the Animator as outlined in the spec.
- [ ] Integrate Saving the current Animator state into the AnimationProject file.

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

- [x] Headless UI test harness (`LedCube.Core.UI.Test`, Avalonia.Headless.XUnit) —
  covers `PlaybackControl` bindings/commands and `PlaylistService` repeat-mode
  auto-advance / navigation (with in-memory playback/playlist/plugin fakes).
- [ ] Add unit tests for core domain logic (`LedCube.Core.Common`)
  - `CubeData.Serialize` bit-packing covered (`CubeDataSerializeTests`); fixed copy-paste bugs in
    `CubeDataTests` parametrization (16-cube and Z-long buffer were never actually exercised).
- [ ] Add tests for plugin lifecycle (`FrameGeneratorBase`, `PluginManager`)
- [ ] Add tests for UDP communication / frame serialization
  - Frame/datagram serialization covered: round-trips for all payload structs + header
    (`DatagramRoundtripTests`) and `CubeDatagramUtils.ResolveDatagramContent` dispatch
    (`CubeDatagramUtilsTests`). Socket-level `UdpCommunication` send/receive still only has
    manual hardware integration tests.
- [ ] Integration tests for playback loop
  - The `PlaybackService` frame loop itself (timer-driven draw / repeat / finish) is still uncovered.

## Animations

Implemented as plugins: `GameOfLife`, `LedWalker`, `Snake3D`, `AudioSpectrum`, `TextWriter`,
`SdfTest`, `FileAnimation`, plus the cube16x ports `FullOn`, `PlaneWalker`, `FallingLeds`,
`Raindrops`, `RandomOnOff`, `RandomToggle`, `BouncingCube`, `Fireworks`, and the SDF / physics
animations `Wave`, `Whisk`, `RotatingObject`, `BouncingBall`, `BouncingCubes`.

- [x] FFT Audio Spectrum animation (`AudioSpectrum` plugin — FftSharp FFT + NAudio WASAPI capture, loopback/mic selectable, 3D spectrum waterfall)
- [ ] Rolling text animation (`TextWriter` plugin — scrolling text display)
- [ ] Keyboard input integration for interactive animations
- [ ] Improve `PlaneWalker` (more sweep patterns / perspectives)
- [ ] **Pong 3D** — Pong in three dimensions, driven by a keyboard / controller input.

### Planned (from the cube16x roadmap)

- [x] **Wave** — a configurable waveform that travels across the cube; LEDs under the function are lit, above are off (`Wave` plugin).
- [x] **Whisk** — a cross of two intersecting planes that rotates around an axis (`Whisk` plugin, SDF).
- [x] **Rotating objects** — rotate arbitrary objects / SDFs within the cube (`RotatingObject` plugin: box-frame / octahedron / torus / box).
- [x] **Bouncing ball** — a round object that bounces around inside the cube, optional gravity (`BouncingBall` plugin, SDF).
- [x] **Bouncing cubes** — multiple cubes that collide and bounce off one another (`BouncingCubes` plugin, SDF + elastic collisions).

