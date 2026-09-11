# Local prompts and Vaalara identity

## Sources and consumer
Read only the target section in `docs/design/art/ART_DIRECTION_ILLUSTRATOR_GUIDE.md` or,
for cave creatures, `docs/design/art/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md`.
Consult target lore/catalog only if needed to resolve identity not defined by the art.

The existing pipeline includes `tools/aseprite/prompts.json`, `tools/aseprite/comfy_gen.ps1`
and `tools/aseprite/postprocess.py`. Inspect the actual schema/configuration before writing/running;
do not assume the active model, hardware, price or capabilities from this reference.

## Tag-pipeline syntax
When the configured model uses tags, keep `positive`/`negative` in English.
For aziib, the historical tokens `pixel art, pixel world, clean pixel sprite, crisp pixels,
no antialiasing` are a starting point; they do not guarantee absence of smoothing.
Prioritize subject, explicit colors, equipment and identity before secondary details.

Example correction for an approved green orc with an axe:
- Positive: `orc barbarian, fully green skin on body and face, wielding a two-handed battle axe,
  fur and iron armor, single full-body character, pixel art, crisp pixels`.
- Conditional negative: `human skin, pink skin, unarmed, extra limbs`.
Add palette, pose and theme only when supported by the target reference.

Anchors such as `orc`, `duergar`, `drow`, `dragonborn`, `slime` or `skeleton knight` help
describe compatible creatures. Do not require D&D in every prompt;
vocabulary similarity does not change Vaalara's race, religion or canon.

## Category constraints
- Character: clothing, skin, hair/hairstyle, age/presentation and props from the reference;
  do not require different gender or anatomy to compensate for a presumed universal generator bias.
- Object: `single object`, material, orientation/plane and margin. Exclude people/scenery if unwanted.
- Tile: continuous area and repeatable edges, without imposing a white/transparent background.
- Negative for observed unwanted content: `text, watermark, hud, scenery, blurry,
  extra limbs` according to the target and generator support. Do not use a negative field if the channel does not support it.
- Palette: English color name with hex when useful; the result requires comparison
  even when the prompt specifies hex. Avoid relying on a race's implicit color.

## Catalog writing
Preserve IDs, dimensions and existing fields; check actual examples before adding an entry.
Do not change gameplay values or bestiary assets as a side effect of prompt authoring.
