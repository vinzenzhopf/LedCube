# LedCube File Formats

This doc defines the on-disk formats the LedCube apps read and write.

| Extension     | Purpose                                          | Status     |
|---------------|--------------------------------------------------|------------|
| `.lcanimraw`  | Baked animation — frames ready for playback      | Spec v1    |
| `.lcanim`     | Animation project — editor's source-of-truth     | Deferred   |
| `.lcplst`     | Playlist referencing baked animations            | Sketched   |

All three are ZIP containers with a JSON manifest plus binary payload(s).

The **baked** format (`.lcanimraw`) is the public, distributable, fully-specified artifact — what gets streamed to the cube. The **project** format (`.lcanim`) is the editor's working file; its schema will be designed against the real editor's needs, after the editor has matured. The **playlist** format references baked animations.

## Hosting library

Four sibling packages under the `LedCube.Animation.FileFormat.*` namespace, each owning one file extension. A shared `Common` package holds primitives used by more than one format.

```
LedCube.Animation.FileFormat.Common          — shared primitives, ZIP/JSON helpers
LedCube.Animation.FileFormat.AnimationRaw    — .lcanimraw (root type: Animation)
LedCube.Animation.FileFormat.Animation       — .lcanim    (root type: AnimationProject, deferred)
LedCube.Animation.FileFormat.Playlist        — .lcplst    (root type: Playlist)
```

### Dependency graph

```
                  ┌──────────────────────────────────────────┐
                  │ LedCube.Animation.FileFormat.Common      │
                  └──────────────────────────────────────────┘
                              ▲      ▲      ▲
              ┌───────────────┘      │      └───────────────┐
              │                      │                      │
   ┌──────────────────────┐ ┌─────────────────────┐ ┌─────────────────────────┐
   │ FileFormat.          │ │ FileFormat.         │ │ FileFormat.             │
   │   AnimationRaw       │ │   Playlist          │ │   Animation             │
   │   (.lcanimraw)       │ │   (.lcplst)         │ │   (.lcanim)             │
   └──────────────────────┘ └─────────────────────┘ └─────────────────────────┘
              ▲                      │                      │
              │                      │                      │
              └──────────────────────┘                      │
              Playlist may embed                            │
              .lcanimraw entries                            │
                                                            │
              ┌─────────────────────────────────────────────┘
              │  AnimationProject.Bake() → Animation
              ▼
   ┌──────────────────────┐
   │ FileFormat.          │
   │   AnimationRaw       │
   └──────────────────────┘
```

Hard rules:

- `Common` references no other LedCube package except `LedCube.Core.Common` (for `Point3D`).
- `AnimationRaw` references `Common` only — never `Animation` or `Playlist`.
- `Animation` references `Common` + `AnimationRaw` (for `Bake()`).
- `Playlist` references `Common` + `AnimationRaw` (for embedded entries).
- `Animation` ⊥ `Playlist`.

Consumers stack cleanly:

- **Streamer / FileAnimation plugin** → `AnimationRaw` + `Common`. Never sees project or playlist types.
- **Animator** → all three formats + `Common`.
- **Unified app (later)** → superset, same rules.

### Rendering lives elsewhere

The format packages contain **only model + IO**. Interpolation, transition math, scratch-buffer management, scene evaluation — all that is consumed downstream in **`LedCube.Core.Animation`** (separate package, designed later). The format spec defines observable output (what pixels appear at time `t`); how a player computes them is an implementation detail.

## `.lcanimraw` — baked animation (v1)

### Container

A standard ZIP archive (Deflate). Required entries:

```
my-animation.lcanimraw
├── manifest.json
└── frames.bin
```

Optional entries:

```
├── thumbnail.png          PNG, recommended 128×128, used for browse UIs
└── meta/...               reserved namespace for future per-frame data
                           (audio cues, palette, etc.)
```

Unknown entries must be preserved by readers that re-save the file. Unknown manifest fields are preserved (round-tripped).

### `manifest.json`

```json
{
  "formatVersion": 1,
  "name": "Sunrise",
  "author": "vinzenz",
  "description": "Slow color wash from blue to gold.",
  "createdUtc": "2026-05-31T14:00:00Z",
  "size":   { "x": 16, "y": 16, "z": 16 },
  "ledFormat": "Rgb",
  "frameCount": 1500,
  "frameTimeUs": 20000,
  "loop": true,
  "keyframes": [
    { "at": 0,    "id": 0 },
    { "at": 100,  "id": 1 },
    { "at": 350,  "id": 2 },
    { "at": 1200, "id": 0 }
  ]
}
```

| Field            | Type      | Required | Notes                                                              |
|------------------|-----------|----------|--------------------------------------------------------------------|
| `formatVersion`  | int       | yes      | Bump on **breaking** changes only. v1 = this document.             |
| `name`           | string    | yes      | Display name. Not unique, not used for lookup.                     |
| `author`         | string    | no       | Free-text.                                                         |
| `description`   | string    | no       | Free-text.                                                         |
| `createdUtc`     | ISO-8601  | no       | UTC timestamp.                                                     |
| `size`           | object    | yes      | `{x,y,z}`, all ≥ 1. Arbitrary; not restricted to 16³.              |
| `ledFormat`      | enum str  | yes      | `Binary` (1 bit), `Grayscale` (8 bit), `Rgb` (24 bit).             |
| `frameCount`     | int       | yes      | **Timeline length in frames** (not the number of unique frames).   |
| `frameTimeUs`    | uint      | yes      | Authoring frame period in microseconds. Player may override.       |
| `loop`           | bool      | no       | Player hint. Default `false`.                                      |
| `keyframes`      | array     | yes      | Schedule. See [Keyframes and the frame pool](#keyframes-and-the-frame-pool). |

### LED encoding (`ledFormat`)

Number of bytes per frame = `frameStride`, computed as:

```
N = size.x * size.y * size.z

Binary     →  frameStride = ceil(N / 8)
Grayscale  →  frameStride = N
Rgb        →  frameStride = N * 3
```

Per-LED layout:

- **Binary**: 1 bit. Bytes are little-endian bit-packed — LED `i` lives at byte `i/8`, bit `i%8` (LSB-first, bit 0 = first LED in the byte). Trailing bits in the last byte are zero. This matches the existing `CubeData<T>.Serialize` wire format.
- **Grayscale**: 1 byte, `0`=off, `255`=full brightness. Linear light intensity (see [Color space](#color-space)).
- **Rgb**: 3 bytes per LED in `R, G, B` order. No alpha. No padding. Linear light per channel.

### Index ordering

LED at coordinate `(x, y, z)` lives at index:

```
i = x + y * size.x + z * size.x * size.y
```

i.e. **X varies fastest**, then Y, then Z (Z-major slices, row-major within each slice). Matches `CubeDataBuffer16.CoordinatesToIndex`, so no permutation is required for the default 16³ cube.

### Keyframes and the frame pool

The animation is a **timeline of `frameCount` slots** driven by a **pool of unique frames** plus a **schedule** that selects which pool entry is active at any given slot. Identical content shared across many timeline positions is stored once.

#### `frames.bin`

Pool of unique frames. Raw concatenated frame payloads, no per-frame header. Frame with id `K` lives at byte offset `K * frameStride`. Pool size (number of unique frames) is implicit: `uncompressedSize / frameStride`. The ZIP entry **must** be Deflate-compressed unless the producer has a specific reason not to.

A reader fails fast if `uncompressedSize % frameStride != 0`.

#### `keyframes` schedule

Each entry: `{ "at": <int>, "id": <int> }`.

- `at` — timeline position (0-based frame index) at which this pool frame becomes active.
- `id` — index into the frame pool (`frames.bin`).

Active frame at timeline position `t` is the pool entry referenced by the **largest** keyframe with `at ≤ t`. Between keyframes, the previous content **holds verbatim** — there is no interpolation in the baked format. A player produces the same pixel output for every position in `[keyframes[i].at, keyframes[i+1].at)`.

If the authoring project used transitions (fades, blends), the baker is responsible for resolving them into discrete pool frames + keyframes. See [`.lcanim` — animation project](#lcanim--animation-project-deferred).

#### Constraints (reader MUST validate, writer MUST guarantee)

1. `keyframes` is non-empty.
2. `keyframes[0].at == 0` — a frame must be defined for position 0.
3. `keyframes` is strictly ascending by `at` (no duplicate timeline positions).
4. Every `at ∈ [0, frameCount)`.
5. Every `id ∈ [0, poolSize)` where `poolSize = uncompressedSize(frames.bin) / frameStride`.
6. `frameCount ≥ 1`.

A static animation is exactly one keyframe: `[{ at: 0, id: 0 }]` with `frames.bin` holding a single frame and `frameCount` controlling the hold duration.

After the last keyframe ends, content **holds** until `frameCount`, then either stops (if `loop=false`) or snaps to `keyframes[0]` (if `loop=true`).

#### Color space

`Grayscale` and `Rgb` byte values are **linear light intensity** in v1. A renderer applying sRGB / display gamma is a downstream concern — the format does not encode display intent. Future versions MAY add an explicit `colorSpace` field if non-linear authoring becomes a requirement.

### Reader API (`LcAnimRawReader`)

Returns an `Animation` (root type, in `LedCube.Animation.FileFormat.AnimationRaw`):

- `Manifest` — deserialized `AnimationManifest`
- `Frames` — `IReadOnlyList<Frame>`, pool indexed by `id`
- `Keyframes` — `IReadOnlyList<Keyframe>`, sorted by `At`
- `KeyframeIndexAt(int t)` — `O(log K)` binary search; returns the index of the keyframe active at position `t`. Pure lookup, no rendering.

`Animation` is **pure data + cheap lookups**. Anything that computes pixel values lives in `LedCube.Core.Animation` (see [Rendering lives elsewhere](#rendering-lives-elsewhere)).

### Writer API (`LcAnimRawWriter`)

Takes an `Animation` and a stream. The writer is responsible for **dedup**: identical `Frame.Data` contents collapse to a single pool entry, with `Keyframe.Id` references updated accordingly. Authors edit conceptually-dense timelines; the writer collapses repeats automatically.

### Versioning

`formatVersion` follows the cross-format [Versioning & migrations](#versioning--migrations) policy. v1 = this document. New optional fields and ZIP entries do not bump the version; layout changes and new required fields do.

### Cube-size mismatch (player concern, not format)

The format always stores the authored size. The player decides what to do when the cube it's driving is a different size — this is governed by the FileAnimation plugin's `OnSizeMismatch` config option (`Reject` / `Center`), not the format. The format itself encodes no opinion.

## `.lcanim` — animation project (deferred)

The editor's source-of-truth format. Contains everything needed to **reopen and continue editing** an animation: primitive shapes, SDF references, layer compositions, parametric animations, **transitions** (fades / blends / easing between keyframes), history hooks, project metadata. Bakes down to `.lcanimraw` for playback via `AnimationProject.Bake()`.

Schema design is **deferred** until the Animator has matured enough to know what its model needs to persist. Premature design would lock in choices against imagined rather than real requirements.

What's locked today:

- Extension: `.lcanim`.
- Root type: `AnimationProject` (in `LedCube.Animation.FileFormat.Animation`).
- Same ZIP-with-manifest container shape as `.lcanimraw`.
- Same `formatVersion` policy.
- `AnimationProject.Bake(targetSize, targetLedFormat, targetFrameTimeUs, …)` produces an `Animation` (the `.lcanimraw` root type) suitable for writing via `LcAnimRawWriter`. **Bake parameters live on the call, not in the project file** — the same project re-bakes to many `.lcanimraw` variants (different cube sizes, frame rates, pixel formats) without modification.

Open question (to decide alongside the editor's data model):

- Does the project format carry its **own natural size** (and `Bake()` defaults to it when no target is given), or is it size-agnostic (primitives are described in cube-fraction or world coordinates, sized only at bake time)? The first is easier to author against; the second composes better when re-baking for different cube hardware.

### What the project format owns (that the baked format doesn't)

Anything that requires interpretation at render time is the project format's job, because the baked format is purely "frames at times":

- **Transitions** between keyframes — `none`, `linear`, easing, dithered-blend for binary, etc. The baker resolves these into discrete pool frames in the output. A 2-second `linear` fade in a project file becomes ~100 unique pool frames in the bake (at 50 fps), with one keyframe per resolved frame. Pool dedup catches whatever the interp produces identical (usually nothing in the middle of a fade, lots at the start/end).
- Primitive shapes, SDF references, parametric / scripted animations.
- Layer compositions and blend modes.
- Authoring metadata (history, brush state, tool settings).

What's not yet defined: every field of the project manifest, the serialization of these types, and the bake algorithm. These follow the editor.

## `.lcplst` — playlist (sketched)

A playlist file the Streamer can open instead of (or in addition to) the in-memory playlist. Same container shape as `.lcanimraw`.

```
my-show.lcplst
├── manifest.json
└── animations/             (optional) embedded .lcanimraw files for self-contained playlists
    ├── sunrise.lcanimraw
    └── ...
```

```json
{
  "formatVersion": 1,
  "name": "Evening show",
  "author": "vinzenz",
  "description": "Sunset to deep-night sequence.",
  "createdUtc": "2026-05-31T14:00:00Z",
  "entries": [
    {
      "id": "1f9c…",
      "instanceName": "Sunrise",
      "source": { "kind": "external", "path": "anims/sunrise.lcanimraw" },
      "repeatCount": 1,
      "frameTimeUsOverride": null
    },
    {
      "id": "3e7b…",
      "instanceName": "Heartbeat",
      "source": { "kind": "embedded", "name": "heartbeat.lcanimraw" },
      "repeatCount": 0,
      "frameTimeUsOverride": 25000
    },
    {
      "id": "8a02…",
      "instanceName": "Snake",
      "source": { "kind": "plugin", "typeName": "LedCube.Plugins.Animation.Snake3D.Snake3DAnimation" },
      "repeatCount": 0,
      "frameTimeUsOverride": null,
      "config": { "Speed": 1.5 }
    }
  ]
}
```

### Source kinds

- `external` — `{ "kind": "external", "path": "<string>" }`. Points to a `.lcanimraw` file. **Paths use forward slashes**; relative paths resolve from the playlist file's directory; absolute paths are honoured as-is. Writers should prefer relative paths for portability.
- `embedded` — `{ "kind": "embedded", "name": "<string>" }`. References an entry under `animations/<name>` in the same `.lcplst` archive. Used for self-contained playlists that can be moved as a single file.
- `plugin` — `{ "kind": "plugin", "typeName": "<string>" }`. References a code-defined `IFrameGenerator` by full type name; the `config` field on the entry carries its `AnimationConfig` dictionary.

### Entry semantics

- **Array order = play order.** No explicit sort key; the JSON array order is authoritative.
- `instanceName` — display label only. Not unique, not used for lookup.
- `id` — opaque string (UUID recommended). Stable across playlist edits so external tools can reference entries.
- `repeatCount` — `0` = infinite, `1` = play once, `N` = play N times. Mirrors in-memory `PlaylistEntry`.
- `frameTimeUsOverride` — optional, `null` to use the entry's native frame time. **Applies to all source kinds**, not just `external`.
- `config` — only meaningful for `kind: plugin`; ignored for other kinds.

### Top-level fields

Same metadata conventions as `.lcanimraw`: `name` required; `author`, `description`, `createdUtc` optional and round-tripped. `entries` may be empty (`[]`) — a valid empty playlist.

### Versioning

Same `formatVersion` policy as `.lcanimraw`: bumps only on breaking changes; additive fields are silent; unknown content is preserved on round-trip.

This section is **not yet implemented**. Field names may still change in v0 of the implementation. Detailed validation rules and the `LcPlstReader` / `LcPlstWriter` land with the playlist-file feature.

## Versioning & migrations

Same policy and reader-internal implementation pattern across all three formats. `formatVersion` is **per-format**: `.lcanimraw`, `.lcanim`, and `.lcplst` each own their own migration chain. A v3 baked file referenced by a v1 playlist works fine.

### Version policy

`formatVersion` bumps **only on breaking changes** — new required fields, layout changes, new mandatory ZIP entries. Additive changes don't bump:

- New optional manifest fields → just default to null/missing in older readers.
- New optional ZIP entries → preserved on round-trip, ignored if unknown.
- New enum values in extensible fields → readers fall back per the field's documented rule.

Round-trip rule: readers MUST preserve unknown content on save, so a newer-format file passing through an older reader doesn't lose data.

### Reader pipeline (internal to each format library)

Per-version parsing is **internal to each format library**. Consumers only ever see the current in-memory model — no consumer code branches on file version.

Layered as:

```
Public API           — LcXxxReader / Writer. Always reads/writes the current model.
   │                   No version is exposed to callers.
   ▼
Migration chain      — pure functions: V1Dto → V2Dto → ... → CurrentDto.
   │                   One step per breaking version bump. Each runs at most once per load.
   ▼
Versioned DTOs       — frozen JSON-shape records. ManifestV1Dto written once, never modified.
                       New version = new DTO + new deserializer + new migration. Old code unchanged.
```

Reader dispatch is a single switch on the `formatVersion` field (peeked from the manifest before full parse):

```csharp
return formatVersion switch
{
    1 => V1.ReadV1.Read(s).MigrateToV2().ToCurrent(),
    2 => V2.ReadV2.Read(s).ToCurrent(),
    _ when formatVersion > CurrentVersion
        => throw new UnsupportedFormatVersionException(formatVersion, CurrentVersion),
    _ => throw new UnsupportedFormatVersionException(formatVersion, CurrentVersion),
};
```

### Writer

Always writes the current version. There is no "save as v1" mode — round-tripping an old file upgrades it on disk. Keeps the write path single-purpose and the migration chain unidirectional.

### Future version handling

A reader encountering `formatVersion > CurrentVersion` rejects with a clear "this file was written by a newer version of LedCube" error. Best-effort reading of future versions is not attempted — the breaking-change semantic in the version policy means it's not safe to guess.

### Additive-change loophole

Optional fields can be added to an existing DTO without bumping `formatVersion` — old files (without the field) deserialize with the default value. Document the version-of-introduction in a code comment on the DTO field so future maintainers can tell what's truly original v1 vs what was added later within v1's lifetime.

### Per-format file layout

```
LedCube.Animation.FileFormat.<Format>/
├── Model/                              current in-memory model (what consumers see)
└── Io/
    ├── Lc<Format>Reader.cs             public: Read(Stream) → current model
    ├── Lc<Format>Writer.cs             public: Write(Stream, model), always current version
    └── Versions/                       internal
        ├── V1/
        │   ├── ManifestV1Dto.cs        frozen JSON shape
        │   ├── ReadV1.cs               stream → V1 DTO
        │   └── (MigrateV1ToV2.cs)      added when V2 lands; pure function
        └── (V2/, V3/ as the format evolves)
```

## Testing

All three format libraries MUST have comprehensive test coverage before being marked stable. The format packages are the foundation everything else depends on; bugs here propagate to every consumer.

### Test project

A single test project for all formats: **`LedCube.Animation.FileFormat.Test`**. Organised by folder, one per package:

```
LedCube.Animation.FileFormat.Test/
├── Common/                tests for the Common package
├── AnimationRaw/          tests for .lcanimraw
├── Animation/             (when format lands)
├── Playlist/              (when format lands)
└── Fixtures/              shared model builders, sample-file helpers, JSON snippet factories
```

### Test categories

Every format package needs coverage in each of these categories. Names below are illustrative.

#### Round-trip

Build a model in code → write to `MemoryStream` → read back → assert structurally equal. The single most valuable category — catches the largest class of bugs cheaply.

```
Roundtrip_StaticBinary_16x16x16            one keyframe, default cube
Roundtrip_StaticGrayscale_8x8x8            non-default size + non-binary format
Roundtrip_StaticRgb_16x16x16
Roundtrip_MultiKeyframe_NoReuse            N keyframes, all unique frames
Roundtrip_MultiKeyframe_WithReuse          N keyframes, repeated frame ids → dedup
Roundtrip_AllOptionalManifestFields_Present
Roundtrip_AllOptionalManifestFields_Absent
Roundtrip_LoopTrue / Roundtrip_LoopFalse
Roundtrip_ThumbnailEntry_Preserved
Roundtrip_UnknownManifestField_Preserved
Roundtrip_UnknownZipEntry_Preserved
```

#### Validation (reader rejections)

One test per numbered constraint in the format's spec. Build a deliberately-malformed payload, assert the right exception type with a meaningful message.

```
Validation_EmptyKeyframes_Throws
Validation_FirstKeyframeNotAtZero_Throws
Validation_NonAscendingKeyframes_Throws
Validation_DuplicateKeyframeAt_Throws
Validation_KeyframeAtBeyondFrameCount_Throws
Validation_KeyframeIdOutOfPoolRange_Throws
Validation_ZeroFrameCount_Throws
Validation_FramesBinSizeNotMultipleOfStride_Throws
Validation_FutureFormatVersion_Throws        UnsupportedFormatVersionException specifically
Validation_MissingManifestEntry_Throws
Validation_MissingFramesEntry_Throws
```

#### Writer behaviour

Properties of the output we want to enforce regardless of input.

```
Writer_AlwaysWritesCurrentVersion
Writer_DedupsIdenticalFrames                identical bytes → single pool entry
Writer_UpdatesIdReferencesAfterDedup        keyframe ids point at the deduped pool slot
Writer_DeflateCompressesFramesBin           ZIP entry is Deflated, not Stored
```

#### Reader API contract

Cheap-lookup helpers on the returned model.

```
Reader_KeyframeIndexAt_AtKeyframeBoundary   t == keyframes[i].at → returns i
Reader_KeyframeIndexAt_BetweenKeyframes
Reader_KeyframeIndexAt_AtZero
Reader_KeyframeIndexAt_AtLastFrame
Reader_KeyframeIndexAt_OutOfRange           defined behaviour (throw or clamp — pick one, test it)
Reader_FramesIsReadOnly                     mutating attempts throw or compile-fail
```

#### Edge cases

```
Edge_MinimalValidFile_SingleKeyframeSingleFrame
Edge_MaxDimensions_LargeCube                exercise stride math at the upper end
Edge_AllZeroFrameData                       sparsity-heavy compression path
Edge_BinaryStride_TrailingBitsZeroed        N not divisible by 8; last byte's high bits are 0
Edge_SinglePoolFrame_ReusedByManyKeyframes  K keyframes, 1 pool frame
```

#### Migration (when V2+ exists)

For each pre-current version:
- Load a real V_n payload, assert the resulting model equals a freshly-built current-model with the same content.
- Test each migration step as a pure function (`Migrate_V1ToV2_PreservesAllFields`, etc.) — no streams, no IO.
- Verify the full read pipeline runs all hops (`Pipeline_V1File_ReachesCurrent_ThroughEveryMigration`).

### Approach

- Use [xUnit](https://xunit.net/) (matches the existing `LedCube.Test` project).
- Fixtures over inheritance — small builder methods in `Fixtures/` rather than abstract base classes.
- No file IO in tests; use `MemoryStream` exclusively. Round-trips run in microseconds, suites stay fast.
- For binary diffs, prefer asserting at the model level (`Assert.Equal(expected, actual)` on records) over byte-comparing streams — model-level failures pinpoint the bad field.

## App ownership today (transitional)

Long-term plan: one unified `LedCube.UI` app contains both authoring and streaming. For now they are separate processes that read/write the same files:

| App                | Reads                              | Writes                             | Notes                              |
|--------------------|------------------------------------|------------------------------------|------------------------------------|
| `LedCube.Animator` | `.lcanim`, `.lcanimraw`            | `.lcanim`, `.lcanimraw`            | Edits projects; bakes to playable. |
| `LedCube.Streamer` | `.lcanimraw`, `.lcplst` (later)    | (none)                             | Plays via the FileAnimation plugin.|
| Future unified app | all three                          | all three                          |                                    |

Both apps reference `LedCube.Animation.FileFormat.*` for IO — no format code is duplicated. Both apps depend on `LedCube.Core.Animation` for rendering / interpolation.
