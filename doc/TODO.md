# TODO / Roadmap

## Streamer / Networking

- [ ] Network streaming: stabilize and fully integrate UDP streamer

## UI (LedCube.Streamer.UI)

- [ ] Re-evaluate MahApps.Metro — still viable? Consider alternatives (e.g. ModernWpf, HandyControl, Fluent UI)
- [ ] General UI cleanup and polish
- [ ] Improve animation list/selection UX

## Animator (LedCube.Animator)

- [ ] **TimelineControl** — scrubbing, multi-frame selection, comfortable frame editing
- [ ] **3D View** — fix and update; render cube properly, fix camera controls
- [ ] **Selection modes** — multiple selection modes for picking/editing LEDs in 3D view
- [ ] **2D plane selection** — finish plane-wise selection component
- [ ] **Edit/selection stack** — command pattern for undo/redo
- [ ] **Streamer module** — stream directly from Animator to cube
- [ ] General: get Animator to a working/usable level

## Infrastructure / Misc

- [ ] Review and tighten nullable usage across Solution
- [ ] Fix/Improve App Configuration Handling
- [ ] Remove/Combine Streamer.UI and Streamer.SmallUI
- [ ] Resolve Changes from AnimationTestUI
  - AnimationList vs AnimationTest control, PreviewKeyDown/Up event handlers? This was probably used for the Snake Animation?)
- [ ] Add/Improve documentation
- [ ] Port to Avalonia

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