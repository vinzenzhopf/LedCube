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
