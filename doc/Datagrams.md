# LedCube UDP Communication Protocol

All communication is over **UDP** (default port **4242**). The Streamer sends a datagram and waits for an acknowledgement. All integers are little-endian.

## Packet Structure

Every datagram starts with a 4-byte header, followed by the payload:

| Offset | Size | Field         | Type   | Description                               |
|--------|------|---------------|--------|-------------------------------------------|
| 0      | 2    | `PayloadType` | UInt16 | Datagram type                             |
| 2      | 2    | `PacketCount` | UInt16 | Rolling counter, used to match responses  |

## Datagram Types

| Value  | Name                | Direction        | Description                        |
|--------|---------------------|------------------|------------------------------------|
| `0x10` | `Discovery`         | Streamer → Bcast | Broadcast search for cubes on LAN  |
| `0x20` | `Info`              | Streamer → Cube  | Request cube status                |
| `0x21` | `InfoResponse`      | Cube → Streamer  | Cube status response               |
| `0x22` | `ErrorResponse`     | Cube → Streamer  | Error response                     |
| `0x30` | `AnimationStart`    | Streamer → Cube  | Begin animation session            |
| `0x31` | `AnimationStartAck` | Cube → Streamer  | Acknowledge animation start        |
| `0x32` | `AnimationEnd`      | Streamer → Cube  | End animation session              |
| `0x33` | `AnimationEndAck`   | Cube → Streamer  | Acknowledge animation end          |
| `0x40` | `FrameData`         | Streamer → Cube  | Send one rendered frame            |
| `0x41` | `FrameDataAck`      | Cube → Streamer  | Acknowledge received frame         |

## Communication Flow

```
Streamer                        Cube
  |-- Discovery (broadcast) --> |  (discover cubes on LAN)
  |<- InfoResponse ------------ |
  |                             |
  |-- Info -------------------> |  (status poll, every ~500ms)
  |<- InfoResponse ------------ |
  |                             |
  |-- AnimationStart ---------> |
  |<- AnimationStartAck ------- |
  |                             |
  |-- FrameData (n) ----------> |  (repeated at FrameTime interval)
  |<- FrameDataAck ------------ |
  |                             |
  |-- AnimationEnd -----------> |
  |<- AnimationEndAck --------- |
```

## Payloads

### Discovery (0x10)

Sent as a UDP broadcast to `255.255.255.255`. Any cube on the network responds with an `InfoResponse`.

| Offset | Size | Field     | Type               | Description       |
|--------|------|-----------|--------------------|-------------------|
| 0      | 32   | `Version` | CString (Ascii32)  | Protocol version  |

### Info (0x20)

Sent directly to a known cube to request its current status.

| Offset | Size | Field     | Type               | Description       |
|--------|------|-----------|--------------------|-------------------|
| 0      | 32   | `Version` | CString (Ascii32)  | Protocol version  |

### InfoResponse (0x21)

| Offset | Size | Field             | Type            | Description                         |
|--------|------|-------------------|-----------------|-------------------------------------|
| 0      | 32   | `Version`         | CString (Ascii32)| Protocol version of the cube        |
| 32     | 4    | `LastFrameTimeUs` | UInt32          | Actual render time of last frame (µs)|
| 36     | 4    | `CurrentTicks`    | UInt32          | Cube internal tick counter          |
| 40     | 2    | `ErrorCode`       | UInt16 (enum)   | Last error (see Error Codes)        |
| 42     | 1    | `Status`          | UInt8 (flags)   | Animation status flags              |

### AnimationStart (0x30)

| Offset | Size | Field           | Type               | Description                         |
|--------|---------|-----------------|--------------------|-------------------------------------|
| 0      | 4    | `FrameTimeUs`   | UInt32             | Desired frame interval in µs        |
| 4      | 64   | `AnimationName` | CString (Ascii64)  | Name of the animation               |
| 68     | 4    | `CurrentTicks`  | UInt32             | Streamer tick counter               |

### AnimationStartAck (0x31)

| Offset | Size | Field          | Type   | Description              |
|--------|------|----------------|--------|--------------------------|
| 0      | 4    | `CurrentTicks` | UInt32 | Cube tick counter        |

### AnimationEnd (0x32)

| Offset | Size | Field          | Type   | Description              |
|--------|------|----------------|--------|--------------------------|
| 0      | 4    | `CurrentTicks` | UInt32 | Streamer tick counter    |

### AnimationEndAck (0x33)

| Offset | Size | Field          | Type   | Description              |
|--------|------|----------------|--------|--------------------------|
| 0      | 4    | `CurrentTicks` | UInt32 | Cube tick counter        |

### FrameData (0x40)

| Offset | Size | Field           | Type   | Description                               |
|--------|------|-----------------|--------|-------------------------------------------|
| 0      | 4    | `FrameNumber`   | UInt32 | Monotonically increasing frame index      |
| 4      | 4    | `FrameTimeUs`   | UInt32 | Desired frame interval in µs              |
| 8      | 4    | `CurrentTicks`  | UInt32 | Streamer tick counter                     |
| 12     | 512  | `Data`          | bytes  | Bit-packed LED state (1 bit per LED)      |

The `Data` field encodes the state of all LEDs as a flat bit array. For a 16×16×16 cube that is 4096 LEDs = 512 bytes.

### FrameDataAck (0x41)

| Offset | Size | Field             | Type          | Description                              |
|--------|------|-------------------|---------------|------------------------------------------|
| 0      | 4    | `FrameNumber`     | UInt32        | Echo of the received frame number        |
| 4      | 4    | `LastFrameTimeUs` | UInt32        | Actual render time of last frame (µs)    |
| 8      | 4    | `CurrentTicks`    | UInt32        | Cube tick counter at acknowledgement     |
| 12     | 4    | `ReceivedTicks`   | UInt32        | Cube tick counter when frame was received|
| 16     | 1    | `Status`          | UInt8 (flags) | Animation status flags                   |

## Enumerations

### CubeErrorCode (UInt16)
| Value  | Name             | Description                          |
|--------|------------------|--------------------------------------|
| `0x00` | `Ok`             | No error                             |
| `0x10` | `PackageOrder`   | Datagrams received out of order      |
| `0x11` | `FrameOrder`     | Frames received out of order         |
| `0x20` | `UnknownPackage` | Unrecognised datagram type           |

### AnimationStatus (UInt8, bit flags)
| Bit    | Name           | Description                             |
|--------|----------------|-----------------------------------------|
| `0x01` | `Running`      | Animation is active                     |
| `0x04` | `PackageLost`  | Packet lost / wrong receive order       |
| `0x08` | `FrameDropped` | Frame dropped (too many frames to show) |
| `0x10` | `FrameRedrawn` | Frame redrawn (streamer too slow)       |
