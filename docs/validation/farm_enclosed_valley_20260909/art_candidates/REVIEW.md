# Farm valley animal candidates — 2026-09-09

Status: ART REVIEW ONLY. Generated through built-in imagegen from the approved valley reference. No files imported into Assets; no gameplay, economy, save or Unity changes made for these images.

Reference: docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png.

## Prompts

Cow v1: Extract/reconstruct the upper black-and-cream cow in the southwest pasture as one transparent game sprite. Match chunky crisp native pixel art, dark brown outlines, cream body with charcoal patches, warm earthy shading, pink muzzle and four grounded short legs. Face left in elevated three-quarter side view; about 48x32 meaningful pixels enlarged nearest-neighbor. No terrain, fence, grass, shadow outside body, checkerboard, labels or second animal.

Cow v2 edit: Preserve cow silhouette, patches, colors, pose and hooves. Remove all soft exterior brown glow. Require alpha255 within animal and alpha0 outside stepped pixel silhouette; no haze, vignette, shadow, smoothing or background fill.

Sheep v1: Extract/reconstruct one sheep from southwest pasture, left-facing elevated three-quarter side view. Warm ivory wool, tan face, short dark hooves, squat oval body, no horns; about32x24 meaningful pixels enlarged nearest-neighbor. One whole animal only, transparent padding. Hard opaque pixel silhouette; absolutely no glow, shadow, halo, vignette, grass, ground, text or checkerboard.

## Review

- Both species now read laterally, closer to the reference than the existing frontal art.
- Cow versions retain a visible soft brown halo despite targeted correction. Do not import unchanged.
- Sheep has a cleaner visible edge, but alpha statistics still reveal faint pixels beyond the visible silhouette. Do not equate visual black preview with an opaque background or clean alpha.
- `alpha_audit.json` records dimensions, actual alpha extrema, nonzero bounds, fully transparent and fully opaque pixel counts. Images are oversized pixel-like renders, not verified exact native pixel grids.
- No claim of runtime fidelity, game-scale inspection, animation or acceptance is made. A later asset preparation/import review must resolve alpha and native scale before integration.
