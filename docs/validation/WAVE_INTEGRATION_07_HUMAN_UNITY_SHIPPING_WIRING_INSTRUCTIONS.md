# WAVE_INTEGRATION_07 — Human Unity Shipping Wiring Instructions

Date: 2026-06-08
Branch: dev

---

## What This Document Is

Step-by-step instructions for a human to regenerate FarmScene with the SellPoint economy integration and verify wiring in the Unity Editor.

---

## Prerequisites

- Unity Editor open with project loaded
- `dev` branch pulled
- No compilation errors in Console

---

## Step 1 — Regenerate FarmScene

Run the generator:

```
Menu bar → CindarsHope → Advanced → Legacy → Scenes → Create MVP FarmScene
```

Expected console output:
```
MVP FarmScene created at Assets/_Game/Scenes/FarmScene.unity.
```

Expected Hierarchy changes:
- `SellPoint` GameObject appears (distinct from `SeedShopPoint`)
- `SellPoint` is at world position (3.5, 7.5) — use Transform inspector to confirm

---

## Step 2 — Verify SellPoint Component Wiring

1. Select `SellPoint` in Hierarchy
2. In Inspector, find the `SellPoint` script component
3. Confirm:
   - `_inventoryManager` field: points to `_Bootstrap > InventoryManager` component
   - `_playerManager` field: points to `_Bootstrap > PlayerManager` component
4. If either field is null: re-run the generator (wiring is done by SerializedObject at generation time)

---

## Step 3 — Verify SellPoint Physical Setup

1. Select `SellPoint` in Hierarchy
2. Confirm:
   - `BoxCollider2D` component present, `Is Trigger` = true
   - `SpriteRenderer` component present, color = teal/cyan (0.25, 0.75, 0.85)
   - Position X ≈ 3.5, Y ≈ 7.5 (north-center zone)

---

## Step 4 — Verify Zone Location in Scene

The SellPoint should be in the north section of the farm, above the crop field zone.

For reference (from zone map):
- Crop field zone: (-1.5, -1.0) to (3.5, 3.0)
- SellPoint zone: (1.75, 6.25) to (5.25, 8.75) [center at (3.5, 7.5)]
- Player spawn: (-2, 0)

---

## Step 5 — Save the Scene

After confirming all wiring, save the scene:

```
Ctrl+S
```

Or: `File → Save Scene`

---

## Step 6 — Run Economy Loop in Play Mode

See `docs/validation/WAVE_INTEGRATION_07_HUMAN_PLAYMODE_CHECKLIST.md` for the full Play Mode validation sequence.

---

## Troubleshooting

### SellPoint does not appear in Hierarchy

- Verify you ran `CindarsHope > Advanced > Legacy > Scenes > Create MVP FarmScene`
- Not `CindarsHope > Repair and Validate > ...` (those are validators, not generators)

### SellPoint._inventoryManager is null

- Re-run the generator — it re-creates the entire scene
- Ensure no compile errors before running the generator

### SellPoint at wrong position

- `CreateSellPoint()` was updated to (3.5, 7.5) in commit for WAVE_INTEGRATION_07
- If position is still (-4.75, -1.75), you may have an older version — pull latest dev

### "Vender" prompt does not appear

- Ensure player has `InteractionSystem` component (created by `CreatePlayer()`)
- Ensure `BoxCollider2D` on SellPoint is `isTrigger = true`
- Check player overlap radius in InteractionSystem settings

### Gold does not increase after selling

- Confirm `_playerManager` is wired on SellPoint (not null)
- Check Console for "Selling X items for 0 gold" — means BaseValue is 0 on items
- Run `CindarsHope/Repair and Validate/Validate Item Database` if available
