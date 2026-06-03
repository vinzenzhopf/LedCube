# Player Architecture

Services own state and business logic. ViewModels are thin UI adapters. Services communicate via direct dependency injection; cross-cutting concerns (selection changes, playback events) use `CommunityToolkit.Mvvm` messages via `WeakReferenceMessenger`.

## Components

### PlaylistEntry
Pure data record holding everything needed to load and configure one animation instance:
- `FrameGeneratorInfo`, `TypeInfo` — which plugin/animation to use
- `InstanceName` — user-defined label
- `Config` — `AnimationConfig` key/value pairs for plugin configuration
- `RepeatCount` — how many times to play (0 = infinite)
- `FrameTimeOverride` — optional per-entry frame timing override

### PlaylistService
Source of truth for the playlist. Holds `IPlaybackService` directly.
- `Entries` — `ReadOnlyObservableCollection<PlaylistEntry>`; all mutations go through service methods (`Add`, `Insert`, `Remove`, `Move`)
- `SelectedEntry` — the entry currently selected for config editing (read-only externally, set via `Select`)
- `Select` — changes selection only; does not affect playback
- `PlayNext`, `PlayPrevious` — load and play the entry after/before the currently *playing* one (wraps around); anchored to `IPlaybackService.CurrentEntry` and never touch the selection. Manual skipping is always sequential regardless of `RepeatMode`.
- `RepeatMode` / `CycleRepeatMode` — playlist auto-advance policy (`PlaylistRepeatMode`), cycled by the PlaybackControl repeat button. In-memory only (not yet persisted).
- When `SelectedEntry` changes: sends `PlaylistSelectionChangedMessage` so config UI updates
- On `PlaybackFinishedMessage`: auto-advances per `RepeatMode` (see below); auto-advance never changes the selection

### PlaylistRepeatMode
Governs what happens when an entry finishes (after its own per-entry `RepeatCount` is exhausted):
- `StopAtEnd` — play through once, stop at the last entry
- `LoopWholePlaylist` (default) — advance sequentially, wrap last → first
- `RepeatCurrentEntry` — keep replaying the current entry
- `FairRandomPlay` — fair shuffle: every entry plays once before any repeats
- `TrueRandomPlay` — pick a fully random entry each time

### PlaybackService
Drives the animation frame loop as a `BackgroundService`. Holds no reference to `PlaylistService`.
- `UpdateFrameGeneratorAsync(entry)` — loads, configures, and initializes the `IFrameGenerator` for the given entry; sets `CurrentEntry` and fires `FrameTime` change
- `StartPlayback`, `StopPlayback`, `PausePlayback`, `ContinuePlayback`, `SeekToFrame` — control playback state
- `CurrentEntry` — the entry currently loaded (independent of which entry is selected)
- `FrameTime` — effective frame time after loading (override or generator default)
- Honours the entry's per-entry `RepeatCount` (read live on each finish: 0 = repeat forever, N = play N times) before advancing
- `FileAnimationGenerator` plays its file with `loopOverride: false`, so a baked `loop: true` flag does not loop internally forever — the animation reports finished after one pass and repetition is governed by `RepeatCount` / `RepeatMode`
- Fires `PlaybackFinishedMessage` when an animation signals `DrawingResult.Finished` and its repeats are exhausted

### PlaylistControlViewModel
Thin UI adapter over `PlaylistService` and `IPlaybackService`.
- Wraps `PlaylistService.Entries` into `PlaylistEntryControlViewModel` items via an internal entry-to-VM map
- Single-click selection calls `PlaylistService.Select(entry)` — updates config, does not affect playback
- Double-click invokes `PlayEntryCommand`: selects the entry, loads it via `UpdateFrameGeneratorAsync`, then starts playback
- Tracks both `CurrentEntry` and `PlaybackState` from `IPlaybackService` to keep `DisplayState` up to date on each entry VM

### PlaylistEntryControlViewModel
Display wrapper for a single `PlaylistEntry`. Exposes `DisplayState` which drives the state indicator:
- `None` — not currently loaded
- `Active` — loaded but stopped (dot indicator)
- `Playing` — currently playing (play icon, accent colour)
- `Paused` — paused (pause icon)

### PluginConfigControlViewModel
Receives `PlaylistSelectionChangedMessage` and rebuilds the config UI for the selected `PlaylistEntry`. Reads `PlaylistEntry.Info.ConfigDescriptors` and `PlaylistEntry.Config` directly — no dependency on any ViewModel.

### PlaybackControlViewModel
Exposes playback commands and current state to the UI. Holds `IPlaybackService` and `IPlaylistService`.
- Mirrors `PlaybackState`, `CurrentFrame`, `CurrentTime`, `FrameTime`, and `CurrentEntry` from `IPlaybackService` via `PropertyChanged` subscription
- All playback commands (Play/Continue, Pause, Stop, Restart) are disabled when no entry is loaded
- Forward/Backward skip the active playback to the next/previous playlist entry (via `PlayNext`/`PlayPrevious`); they do not change the selection; disabled when fewer than 2 entries
- A repeat button cycles `PlaylistService.RepeatMode` (`CycleRepeatModeCommand`); its icon/tooltip mirror the current mode
- Constructs a local `AnimationViewModel` from the loaded `PlaylistEntry` for display; updates `FrameTime` once the generator has initialised

## Selection vs Playback

Selection and playback are independent. A single click selects an entry — this updates the config panel but leaves playback unchanged. Double-clicking (or using auto-advance) loads and starts an entry. The currently loaded entry is tracked via `PlaybackService.CurrentEntry`, which differs from `PlaylistService.SelectedEntry` whenever the user browses config without changing what is playing.

## Data Flow

```
User single-clicks entry
  → PlaylistControlViewModel calls PlaylistService.Select(entry)
  → PlaylistService sends PlaylistSelectionChangedMessage
  → PluginConfigControlViewModel rebuilds config UI
  (playback unchanged)

User double-clicks entry
  → PlaylistControlViewModel.PlayEntryCommand
  → PlaylistService.Select(entry)         [selection + config]
  → PlaybackService.UpdateFrameGeneratorAsync(entry)
  → PlaybackService.StartPlayback()

PlaybackService.CurrentEntry or PlaybackState changes
  → PlaylistControlViewModel updates DisplayState on all entry VMs
  → PlaybackControlViewModel updates Animation / FrameTime display

Animation ends (DrawingResult.Finished, repeats exhausted)
  → PlaybackService sends PlaybackFinishedMessage
  → PlaylistService advances to next entry, loads and plays it
```

## Messages

| Message | Sender | Receivers |
|---|---|---|
| `PlaylistSelectionChangedMessage` | `PlaylistService` | `PluginConfigControlViewModel` |
| `PlaybackFinishedMessage` | `PlaybackService` | `PlaylistService` |

## Constraints

- `PlaybackService` holds no ViewModel references — its interface uses `PlaylistEntry`, not `PlaylistEntryControlViewModel`
- `PlaylistService` is the sole owner of `PlaylistEntry` objects; ViewModels hold display wrappers only
- `IPlaylistService.Entries` is read-only; all mutations go through explicit service methods
- `PlaybackService` must be registered as both a singleton and a hosted service so its background frame loop runs
