# TimelineControl

A custom WPF control for displaying and interacting with an animation timeline.

**Projects:**
- Control: `LedCube.Core.UI.TimelineControl/LedCube.Core.UI.TimelineControl.csproj`
- Demo: `LedCube.Core.UI.TimelineControl.Demo/LedCube.Core.UI.TimelineControl.Demo.csproj`
- Tests: `LedCube.Core.UI.TimelineControl.Test/LedCube.Core.UI.TimelineControl.Test.csproj` (to be created)

## Modes

### Live Mode
For streaming/playback of dynamic animations with no fixed length.
- No total frame count; timeline has no fixed right edge
- Playhead is fixed at the left edge and advances as frames arrive
- Read-only: no scrubbing, no selection, no marker dragging
- Timeline auto-scrolls to keep the playhead visible
- Markers are still writable from code (e.g. animation sends progress events)

### Edit Mode
For editing animations with a known frame count.
- Total frame count is set via a property; displayed and editable inline (e.g. spinbox in ruler area)
- Full scroll, zoom, selection, loop region, and marker interaction
- Playhead is draggable; clicking the ruler repositions it and clears the current selection

## Ruler & Tick Marks

- Horizontal ruler across the top; timeline runs left to right
- **Frame numbers** always shown on major tick marks
- **Time labels** (e.g. `0:01.200`) shown additionally when `FrameTime` is set
- Tick mark density adapts to zoom level:
  - Minor ticks: every frame (hidden when too dense at current zoom)
  - Major ticks: every N frames (configurable; default 10)
- Snapping is always on — the environment is discrete (frame-based), nothing exists between frames

## Scroll & Zoom

- **Scroll:** MouseWheel or horizontal scrollbar (auto-shown when content exceeds viewport)
- **Zoom:** Shift+MouseWheel, centered on the mouse cursor position
  - Range: individual frames clearly visible → entire animation fits in view
- In Live Mode, scrollbar is hidden; timeline auto-scrolls to follow the playhead

## Playhead

A vertical line indicating the current frame.

- **Edit Mode:** draggable; click anywhere on the ruler to jump; snaps to nearest frame; clears current selection on click
- **Live Mode:** position controlled by code only; fixed display at left edge in the visible window
- Raises `PlayheadChanged` when moved by the user

## Selection

Represents a single contiguous range of frames chosen for editing.

- **Click + drag** on the timeline sets the selection range
- Shown as a highlighted region
- Clicking without dragging moves the playhead and clears the selection
- Code-settable via `SelectionStart` / `SelectionEnd`
- Raises `SelectionChanged` when changed by the user
- Disabled (hidden) in Live Mode

## Loop Region

A special range that restricts playback to a sub-section of the animation.

- Shown as a distinct region on the ruler (different color from selection)
- Draggable In and Out handles to adjust the range
- Can be enabled/disabled via a property or toggle
- Code-settable via `LoopStart` / `LoopEnd`
- Independent of the selection range

## Markers

Markers annotate the timeline. Multiple markers may overlap on the same frame.

### Point Marker
- Marks a single frame
- Properties: `Frame`, `Label`, `Color`, `IsDraggable`, `ClampBehavior`

### Range Marker
- Marks a span of frames with a tinted color band
- Properties: `StartFrame`, `EndFrame`, `Label`, `Color`, `IsDraggable`, `ClampBehavior`
- Drag interaction (when `IsDraggable = true`):
  - Drag on **end handle** → moves only that end (resize)
  - Drag on **middle** of the range → moves the whole range

**Draggability:**
- `IsDraggable = true` — user keyframes; can be repositioned by dragging
- `IsDraggable = false` — informational markers (e.g. auto-generated from animation); display only

**ClampBehavior** (applied when `TotalFrames` is reduced):
- `Drop` — marker is removed if it falls outside the new range
- `Clamp` — marker is clamped to the new last frame

Markers are managed via an `ObservableCollection<MarkerBase>`; changes reflect immediately.

**Visual feedback:** a ghost line / position preview is shown during drag operations.

## Keyboard Shortcuts

Core shortcuts (long-term: configurable via app settings):

| Key | Action |
|-----|--------|
| `,` | Step back one frame |
| `.` | Step forward one frame |
| `Home` | Jump to first frame |
| `End` | Jump to last frame |
| Direct number entry | Jump to typed frame number |

## Events

| Event | Fires when |
|-------|-----------|
| `PlayheadChanged` | Playhead moved by user |
| `SelectionChanged` | Selection changed by user |
| `MarkerDragStarted` | User begins dragging a marker |
| `MarkerDragging` | Marker position changes during drag (continuous) |
| `MarkerDragCompleted` | User releases marker drag |

## Object Model

```
TimelineControl
  ├── Mode              : TimelineMode { Live, Edit }
  ├── TotalFrames       : int (Edit Mode only)
  ├── FrameTime         : TimeSpan? (enables time labels)
  ├── CurrentFrame      : int
  ├── SelectionStart    : int?
  ├── SelectionEnd      : int?
  ├── LoopStart         : int?
  ├── LoopEnd           : int?
  ├── LoopEnabled       : bool
  └── Markers           : ObservableCollection<MarkerBase>

MarkerBase
  ├── Label             : string
  ├── Color             : Color
  ├── IsDraggable       : bool
  └── ClampBehavior     : ClampBehavior { Drop, Clamp }

PointMarker : MarkerBase
  └── Frame             : int

RangeMarker : MarkerBase
  ├── StartFrame        : int
  └── EndFrame          : int
```

## Architecture

Three layers — model and drawing are fully platform-agnostic; only the host is WPF-specific.

### Model Layer — pure C#, no UI dependencies

**`TimelineLayout`** — immutable value: all coordinate math for a given zoom/scroll/viewport state.
```
TimelineLayout
  ├── ZoomScale         : double        (pixels per frame)
  ├── ScrollOffsetPx    : double
  ├── TotalFrames       : int
  ├── ViewportWidth     : double
  ├── FrameToPixel(frame) → double
  ├── PixelToFrame(x)   → int          (always snaps — discrete environment)
  ├── VisibleFrameRange → (first, last)
  └── TotalWidthPx      → double
```

**`TimelineState`** — all mutable runtime state; observed by the host for change detection.
```
TimelineState
  ├── Mode              : TimelineMode { Live, Edit }
  ├── TotalFrames       : int
  ├── FrameTime         : TimeSpan?
  ├── CurrentFrame      : int
  ├── SelectionStart    : int?
  ├── SelectionEnd      : int?
  ├── LoopStart         : int?
  ├── LoopEnd           : int?
  ├── LoopEnabled       : bool
  ├── ZoomScale         : double
  ├── ScrollOffsetPx    : double
  ├── Markers           : ObservableCollection<MarkerBase>
  └── ActiveDrag        : DragOperation?   (internal — playhead, selection edge, marker handle)
```

**Marker model:**
```
MarkerBase
  ├── Label, Color, IsDraggable, ClampBehavior

PointMarker : MarkerBase
  └── Frame : int

RangeMarker : MarkerBase
  ├── StartFrame, EndFrame : int
  └── Drag: end handle → resize one end; middle → move whole range
```

### Drawing Layer — SkiaSharp (`SKCanvas`)

**`TimelineRenderer`** — stateless; called with the current layout + state, draws everything onto an `SKCanvas`. Broken into focused methods:

- `DrawBackground` — fill, baseline
- `DrawTicks` — minor/major tick marks, only within visible frame range
- `DrawRuler` — frame number labels, time labels (if `FrameTime` set)
- `DrawLoopRegion` — tinted band + in/out handles
- `DrawSelection` — highlighted selection band
- `DrawMarkers` — point and range markers, stacked
- `DrawPlayhead` — vertical line at current frame
- `DrawDragGhost` — ghost line/preview during active drag

**`RenderResources`** — cached `SKPaint` instances (colors, fonts, stroke widths). Allocated once, reused every frame. No per-draw allocations.

### Host Layer — thin, platform-specific

**WPF:** `TimelineControl` wraps `SKElement` (from `SkiaSharp.Views.WPF`).
- Exposes DependencyProperties matching `TimelineState` fields for MVVM binding
- `PaintSurface` callback: builds a `TimelineLayout` from current state + control size, calls `TimelineRenderer.Draw(canvas, layout, state)`
- Mouse/keyboard handlers update `TimelineState`, call `InvalidateVisual()`
- Fires routed events: `PlayheadChanged`, `SelectionChanged`, `MarkerDragStarted`, `MarkerDragging`, `MarkerDragCompleted`

**Avalonia (future):** replace `SKElement` → `SKCanvasView`, DependencyProperties → AvaloniaProperties, WPF input events → Avalonia input events. Model and drawing layers unchanged.

### Why SkiaSharp

WPF's `DrawingContext` and Avalonia's `DrawingContext` are similar but not identical — porting would require touching every drawing call. SkiaSharp's `SKCanvas` API is identical on both platforms; only the ~100-line host changes on a port.

## Performance Notes

- `TimelineRenderer` is immediate-mode; only the visible frame range is rendered
- `RenderResources` avoids per-frame allocations
- In Live Mode, only the playhead region needs invalidation on frame advance

## Testing

- `TimelineLayout` and `TimelineState` are pure C# — unit-testable without any WPF host
- Coordinate math, snap logic, zoom calculations, marker clamping: all covered in the Test project
- Demo project serves as a visual/manual test harness
