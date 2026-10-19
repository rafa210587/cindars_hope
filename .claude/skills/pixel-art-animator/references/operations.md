# Frame operations with Aseprite

Load for actual sequence edits. Use the installed CLI verified by aseprite-authoring, preserving originals.
Official API: [Sprite](https://www.aseprite.org/api/sprite), [Frame](https://www.aseprite.org/api/frame),
[Tag](https://www.aseprite.org/api/tag), [Cel](https://www.aseprite.org/api/cel).

- Lua frame numbers are one-based. Resolve current frame references after insertion/deletion.
- External tool indices and export filename numbering can differ. Confirm the adapter/CLI
  contract and convert at the boundary; never infer index base from an example filename.
- `frame.duration` is in **seconds**, while exported JSON frame durations are in **milliseconds**.
  Convert explicitly: 120 ms becomes `0.120`; never copy an MCP millisecond value directly into Lua.
- `sprite:newFrame(frameNumber)` duplicates the chosen frame; inspect resulting cels and tags.
  `sprite:newEmptyFrame()` creates an empty frame. Do not confuse duplicates with new poses.
- `sprite:newTag(fromFrame, toFrame)` defines an inclusive range. Give it the consumer's exact name.
  Set `tag.aniDir` with the documented AniDir enum. Do not assume a tag changes Unity playback.
- A held pose can use a longer duration; do not add identical frames merely to create a hold unless
  the consumer requires a fixed FPS sheet.
- Tweening translation/opacity can be appropriate for that intended effect, but does not prove
  a new articulated pose. Locomotion still needs observable contact, support and weight transfer.
- Linked cels share an image: later pixel edits affect every linked instance. Use a deliberate
  `Image(cel.image)` copy before changing only one pose; preserve intentional sharing for static layers.
  Do not link eyes/face across frames when that would suppress a blink or expression change.
- Tag endpoints, slice keys and external event-frame indices can drift after edits. Compare them
  with the recorded baseline rather than assuming Aseprite updates external references.

Minimal timing/tag fragment for an existing candidate with two frames (hypothetical, not a motion recipe):

```lua
local sprite = assert(app.activeSprite)
assert(#sprite.frames >= 2)
sprite.frames[1].duration = 0.120
sprite.frames[2].duration = 0.180
local tag = sprite:newTag(1, 2)
tag.name = "timing-study"
tag.aniDir = AniDir.FORWARD
```

The fragment neither draws motion nor chooses production timing. Save under a new path, reopen,
export preview + JSON, assert 120/180 ms, and inspect playback before accepting the result.
