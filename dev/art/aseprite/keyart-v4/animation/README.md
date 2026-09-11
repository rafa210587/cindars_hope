# Farm door and water animation — art handoff

2026-09-10. Authored with Aseprite 1.3.18.5-x64 CLI/Lua. Original PNGs remain unchanged. No consumer C#, importer metadata or Unity execution in this slice.

## Door contract

- Production sheet: `Assets/_Game/Art/Generated/World/props/farmhouse_door_open_v4.png`, 135×27, five horizontal 27×27 frames, 80 ms each. Opening order 0→4; closing reverses that order.
- Production cover: `Assets/_Game/Art/Generated/World/props/farmhouse_door_threshold_v4.png`, 27×27.
- Canvas corresponds to house top-origin `(108,143)`. Hinge/contact is local top-origin `(3,22)`, house `(111,165)`. Unity normalized pivot `(0.111111,0.185185)` for both assets.
- Painted aperture is house inclusive x111..131/y146..164. Cover is opaque only within this 21×19 rectangle; it must render behind the leaf and above the unchanged house. Opening the overlay reveals a native-palette dark interior. Cover cannot erase the underlying door unless rendered above the house.
- Leaf preserves the original pair of painted panels as one left-hinged timber leaf. Its widths are21/17/11/5/2px. The free edge descends0/1/2/3/3px; hinge remains fixed. Whole canvas/PPU must stay stable across frames.
- The house staircase support `(122,192)` differs from this leaf contact. Consumer owner maps these independently; never put the leaf at the staircase bottom.
- Layered sources: `farmhouse_door_open_v4.aseprite` and `farmhouse_door_threshold_v4.aseprite`. Opening tag and hinge slice are included; hidden baseline preserves the unmodified source crop.
- `farmhouse-door-context.aseprite` and GIFs show opening/closing on the entire source house, with preview-only endpoint holds. The production sheet retains80ms/frame.

## Water contract

- Production sheet: `Assets/_Game/Art/Generated/World/tiles/ground_water_loop_v4.png`, 384×64, six horizontal64×64frames at200ms each (5FPS), repeating0→5→0.
- Consumers use16native16×16slices perframe at16PPU, synchronized. Do not treat the entire384×64sheet as one texture frame.
- All4096RGBA pixels of frame0 exactly equal `ground_water_keyart_v4.png`. Every pixel in every frame belongs to the source palette; all remain opaque.
- Long periodic row bands shift the existing texture horizontally by up to2px. Neighboring frames change coherently by at most two pixels; no added noise, palette flicker, alpha or moving shoreline geometry.
- Layered source: `ground_water_loop_v4.aseprite`; hidden original frame, six timed frames and `surface_loop` tag. Native and4×GIF previews included.

## Evidence and limits

- Reopened both native animation files: frame counts, canvas, tag and timing assertions PASS (`author-log.txt`).
- Pixel equality, opacity, palette and non-identical transition assertions PASS (`verification.txt`).
- Opened and visually reviewed `door-poses-large.png`, `door-context-poses-large.png` and `water-poses-large.png`. Door context shows fixed batten/steps and covered painted door in the open pose. Static strips establish geometry and registration only.
- Timed GIFs exported with real multiple frames. Continuous playback/fluency review: NOT RUN in this art slice. In-game appearance, timing and collision behavior remain with root integration validation.
- Unity validation: NOT RUN. Reason: root is sole Unity launcher; art-only ownership. Command attempted: none. Residual risk: Unity import, slicing and runtime alignment not validated here.

## SHA256 freeze

| File | SHA256 |
|---|---|
| Original farmhouse_keyart_v4.png | B8B90714711A195C3BA5D853C85A22D0A27D27B9D9ED60C65B3386A9E4983399 |
| Original ground_water_keyart_v4.png | 4928352E7EB0B72D72672A50E3E8CD66F559ADA6A9F34888296911B892579E67 |
| farmhouse_door_open_v4.png | CE14264E8C412E8C6B13363322F012D25545B3B3411A4B8A61A49C39BE479913 |
| farmhouse_door_threshold_v4.png | DA1220239E6496F21B9A519CB13718614E12862C7506F1D38835E2C0698EA545 |
| ground_water_loop_v4.png | 2358CFDD7EEFD7EE5C94E4C854992BA20E37A7B6CE6BFB1F61E224B481796E49 |

`author-animation.lua` recreates these new candidates from the unchanged sources; `verify-animation.lua` checks exported pixels and renders the context strip. All image editing/export was performed in Aseprite.
