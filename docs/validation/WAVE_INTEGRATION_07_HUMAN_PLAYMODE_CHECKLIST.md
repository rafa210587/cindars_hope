# WAVE_INTEGRATION_07 Human Play Mode Checklist

Date created: 2026-06-08
Branch: dev
Purpose: Validate economy loop — collect item → SellPoint → sell → gold awarded

## Prerequisites

- [ ] WAVE_INTEGRATION_06A Play Mode checklist has passed (or Partial Pass accepted)
- [ ] Pull latest `dev` branch
- [ ] Open Unity (2D URP LTS project)
- [ ] Open FarmScene (`Assets/_Game/Scenes/FarmScene.unity`)
- [ ] Console shows 0 red errors before Play Mode

---

## Step 1 — Regenerate FarmScene

- [ ] Run menu: `CindarsHope > Advanced > Legacy > Scenes > Create MVP FarmScene`
- [ ] Console shows "MVP FarmScene created at..." — no red errors
- [ ] Scene Hierarchy shows `SellPoint` GameObject (distinct from SeedShopPoint)
- [ ] Select `SellPoint` in Hierarchy:
  - [ ] Position X: ~3.5, Y: ~7.5 (north-center zone)
  - [ ] Has `BoxCollider2D` (isTrigger = true)
  - [ ] Has `SellPoint` component with `_inventoryManager` and `_playerManager` wired

---

## Step 2 — Enter Play Mode

- [ ] Press Play button
- [ ] Console shows 0 red errors on startup
- [ ] Player spawns successfully
- [ ] Camera follows player

---

## Step 3 — Provision Debug Loadout

- [ ] Run menu: `CindarsHope > Integration > Debug > Provision Farm Smoke Loadout`
- [ ] Console shows: `[DebugLoadout] Farm Smoke Loadout complete.`
- [ ] item_seed_carrot and item_crop_carrot are in inventory (or provision item_crop_carrot manually)
- [ ] item_material_wood, item_material_stone are in inventory

---

## Step 4 — Walk to SellPoint Zone

- [ ] Walk player north toward Y ~7.5 (north-center of farm)
- [ ] SellPoint object is visible (teal/blue square placeholder sprite)
- [ ] Interaction prompt "Vender" appears when player is near SellPoint
- [ ] No red errors while approaching

---

## Step 5 — Sell Items

- [ ] Press E (or interact key) near SellPoint
- [ ] Console shows SellPoint output — something like "Selling X items for Y gold" or similar
- [ ] Gold counter in HUD increases
- [ ] Inventory shows items removed (crops/materials gone)
- [ ] Feedback panel shows sale message
- [ ] No red errors

Expected behavior:
- item_crop_carrot sold (BaseValue > 0)
- item_material_wood sold (BaseValue > 0)
- item_material_stone sold (BaseValue > 0)
- item_seed_carrot sold (BaseValue > 0) if present
- item_fish_common sold (BaseValue > 0) if present

---

## Step 6 — Verify Gold Increment

- [ ] Note gold before selling (from HUD or console)
- [ ] Sell one item type at a time if needed for precision
- [ ] Gold after = Gold before + (BaseValue * amount)
- [ ] GoldChangedEvent received by HUD (gold counter updates without scene reload)

---

## Step 7 — Verify Essential Tool Protection

- [ ] Provision `item_shop_tool_hoe_basic` (or any item with "tool_hoe" in ID)
- [ ] Interact with SellPoint while hoe is in inventory
- [ ] Hoe remains in inventory after sell (SellableItemPolicy blocked it)
- [ ] Gold calculation does NOT include hoe value
- [ ] No red errors

---

## Step 8 — Empty Inventory Check

- [ ] Sell all items until inventory is empty
- [ ] Interact with SellPoint on empty inventory
- [ ] No red errors (SellPoint gracefully handles empty case)
- [ ] No gold change if no sellable items

---

## Step 9 — Exit Play Mode

- [ ] Press Stop
- [ ] Console shows 0 red errors from shutdown
- [ ] Run `git status --short` — confirm only expected files modified

---

## Pass Criteria

All items below must be checked to advance to WAVE_INTEGRATION_08:

- [ ] SellPoint placed at (3.5, 7.5) in scene after regeneration
- [ ] Interaction prompt "Vender" appears when player is near
- [ ] Crops/materials/fish sold and removed from inventory
- [ ] Gold increments by correct amount (BaseValue * count)
- [ ] GoldChangedEvent / HUD updates on sale
- [ ] Essential tools (hoe variants) NOT sold
- [ ] No red errors in any step

---

## Failure Criteria (blocks WAVE_INTEGRATION_08)

- [ ] SellPoint not present in scene after FarmScene regeneration
- [ ] SellPoint placed at wrong position (not north-center)
- [ ] `_inventoryManager` field null on SellPoint (wiring failed)
- [ ] `_playerManager` field null on SellPoint (wiring failed)
- [ ] Gold does not change after selling items
- [ ] Items remain in inventory after successful sale
- [ ] Red exception during sale

---

## Executor Notes (fill in during Play Mode)

Date executed: ___________
Unity version: ___________
SellPoint position confirmed: ___________
Items sold: ___________
Gold before: ___________ / Gold after: ___________
Tool protection confirmed: ___________
Red errors: ___________
Overall result: PASS / PARTIAL_PASS / FAIL
Can start WAVE_INTEGRATION_08: YES / NO
