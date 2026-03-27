# LedCube.Streamer App

## Planned Features

### Networking
- Connect to a LED Cube via Network
- Broadcast Discovery in the Local Network

### Playback
- **Play** — start or resume the current animation
- **Pause** — freeze at current frame; animation time is paused; resumes from next frame on Play
- **Stop** — reset the current animation to its beginning (does not advance playlist)
- **Restart Playlist** — reset to the first playlist entry and restart
- **Speed Up/Down** — scale animation playback speed
- **Scrubbing** — seek within the current entry; only enabled for fixed-length animations; disabled for dynamic/endless animations
- Forward keyboard input to the *currently playing* animation only (for interactive animations, e.g. Snake3D)

### Animations
- Load animations from plugins
- Per-instance plugin configuration (each playlist entry has its own independent config)
- Save/load animation configurations as part of the playlist file

### Playlist
- Add, remove, reorder entries
- Per-entry settings:
  - **Duration** — fixed play time, or `null` to let the animation signal its own end or wait for manual advance
  - **Repeat** — play N times, or infinite; after repeats exhaust, auto-advance to next entry
  - **Advance mode** — auto (after duration × repeats, or on animation-end signal) or manual (user triggers next)
- Playlist-level repeat — loop entire playlist
- Save/load playlist to file

### Animation End Signals
- A dynamic animation may signal that it has ended (e.g. Snake3D game over)
- On end signal: if advance mode is auto, advance to next entry; if manual, wait for user input
