# Timeline Control Requirements

The Timeline Control is there to select one or more frames from an FrameSeries or Animation.

The Following Requirements must be fulfilled:
- Timeline, running from left to right.
- Needs to be scrollable. (MouseWheel & Scrollbar)
- Must be zoomable, from individual frames to the whole animation up to multiple minutes. (Shift+MouseWheel & Buttons or Dropdown)
- Major & Minor TickMarks with Labels below (minor every frame, major every 10 frames)
- Number of Frame needs to be changeable
- It needs one or two Markers that select the current frame(s)
  - Draging with left mouse Button should move the Markers
  - Shift + Drag should place the Markers to select a frame range
  - Left click on (or near) a Tick-Mark should select the frame
  - Markers should be draggable and should snap to the tick marks
  - Markers should be removable