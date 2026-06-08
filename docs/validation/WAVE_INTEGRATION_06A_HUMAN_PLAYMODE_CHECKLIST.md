# WAVE_INTEGRATION_06A Human Play Mode Checklist

Date created: 2026-06-08
Branch: dev
Purpose: Validate debug loadout provisioner and confirm WAVE04/05/06 playable slices are testable

## Prerequisites

- [ ] Pull latest `dev` branch
- [ ] Open Unity (2D URP LTS project)
- [ ] Open FarmScene (`Assets/_Game/Scenes/FarmScene.unity`)
- [ ] Console shows 0 red errors before Play Mode

## Step 1 — Regenerate FarmScene

- [ ] Run menu: `CindarsHope > Advanced > Legacy > Scenes > Create MVP FarmScene`
- [ ] Console shows "FarmScene created" (or similar success message) — no red errors
- [ ] Scene Hierarchy shows:
  - [ ] `_Bootstrap` GameObject present
  - [ ] `Player` present
  - [ ] `FarmPlots` parent with FarmPlot_00, FarmPlot_01, FarmPlot_02, FarmPlot_03 children
  - [ ] `FarmResourceInteractables` parent with TreeResource_01, RockResource_01, ForageResource_01 children
  - [ ] `FishingSpot` (or `FishingSpotZone`) present near (7.8, -2.8)

## Step 2 — Validate Resource Interactables

- [ ] Run menu: `CindarsHope > Repair and Validate > Validate Farm Resource Interactables`
- [ ] Console shows: `[ValidateFarmResource] All 3 FarmResourceInteractable(s) validated. No issues found.`
- [ ] No red errors

## Step 3 — Enter Play Mode

- [ ] Press Play button in Unity Editor
- [ ] Console shows 0 red errors on startup
- [ ] Player spawns at expected position (around (-2, 0))
- [ ] Camera follows player

## Step 4 — Provision Debug Loadout

- [ ] With game running, open menu: `CindarsHope > Integration > Debug > Provision Farm Smoke Loadout`
- [ ] Console shows: `[DebugLoadout] Farm Smoke Loadout complete.`
- [ ] Console shows `Added (N):` list — note which items were added
- [ ] Console shows `Skipped/unknown (N):` list — record for audit
- [ ] Console shows `Hotbar:` line — note which slots were set
- [ ] No red errors

Expected minimum items in Added list:
- item_seed_carrot x20
- item_tool_fishing_rod_basic x1
- item_material_wood x10
- item_material_stone x10
- item_fish_common x5
- item_consumable_potion_hp_small x5

Expected minimum in Skipped list (acceptable):
- item_tool_hoe_basic
- item_tool_watering_can_basic
- item_tool_axe_basic
- item_tool_pickaxe_basic
- item_wood, item_stone (legacy aliases)

## Step 5 — Confirm Hotbar

- [ ] Hotbar UI shows items in slots 4 (fishing rod) and 5 (carrot seed)
- [ ] Slots 0–3 may be empty if tool IDs are unknown — this is ACCEPTABLE for 06A
- [ ] No red errors from HotbarState

## Step 6 — Test WAVE05: FarmPlot

- [ ] Walk player to FarmPlot_00
- [ ] Interaction prompt appears ("Interagir" or crop action text)
- [ ] Press E / interact key
- [ ] Console shows farm plot interaction (till, water, plant, or grow step)
- [ ] Plant carrot seed:
  - [ ] FarmPlot shows planting visual (or moves to watered/planted state)
  - [ ] item_seed_carrot count in inventory decreases by 1
- [ ] No red errors during farm plot interaction

## Step 7 — Test WAVE06: Tree Resource

- [ ] Walk player to TreeResource_01 (around x:7.5, y:3.5 — east zone)
- [ ] Interaction prompt appears ("Interagir")
- [ ] Press E / interact key
- [ ] Console shows: `[FarmResource] Tree: added 2x item_material_wood to inventory.`
- [ ] item_material_wood count increases in inventory
- [ ] TreeResource_01 visual changes (darker color = depleted)
- [ ] Press E again — interaction prompt is gone (Depleted state, CanInteract = false)
- [ ] No red errors

## Step 8 — Test WAVE06: Rock Resource

- [ ] Walk player to RockResource_01 (around x:-9.0, y:4.5 — west rock zone)
- [ ] Press E — Console shows: `[FarmResource] Rock: added 2x item_material_stone to inventory.`
- [ ] item_material_stone count increases
- [ ] Rock visual changes to depleted
- [ ] No red errors

## Step 9 — Test WAVE06: Forage Resource

- [ ] Walk player to ForageResource_01 (around x:-8.0, y:-2.5 — forage zone)
- [ ] Press E — Console shows: `[FarmResource] Forage: added 1x item_herbs to inventory.`
  - Note: item_herbs may be in ItemDatabase as item_consumable or different ID; if AddItem returns false, console will show warning instead
- [ ] Forage visual changes to depleted
- [ ] No red errors

## Step 10 — Test WAVE06: Lake / FishingSpot

- [ ] Walk player to FishingSpot (around x:7.8, y:-2.8 — south-east)
- [ ] Interaction prompt "Pescar" appears
- [ ] Press E — fishing routine starts (cast line, wait, reel)
- [ ] item_fish_common added to inventory on success
- [ ] No red errors

## Step 11 — Verify AddItem Safety

- [ ] Fill inventory completely (run Provision All Known Items if needed)
- [ ] Walk to a non-depleted resource (or reset with ResetResource)
- [ ] Press E to interact
- [ ] Console shows: `[FarmResource] <type>: AddItem returned false for ... Resource stays Available.` (NOT depleted)
- [ ] Resource visual does NOT change to depleted
- [ ] Resource can be interacted again after freeing inventory space

## Step 12 — Exit Play Mode

- [ ] Exit Play Mode (Stop button)
- [ ] Console shows 0 red errors from shutdown
- [ ] Run `git status --short` — confirm only expected files modified

---

## Pass Criteria

All items below must be checked to advance to WAVE_INTEGRATION_07:

- [ ] Debug loadout provisions minimum items (seeds + fishing rod + materials)
- [ ] WAVE05 FarmPlot interaction works (at least one step: till/water/plant/harvest)
- [ ] WAVE06 TreeResource_01 depletes only on AddItem success
- [ ] WAVE06 RockResource_01 depletes only on AddItem success
- [ ] WAVE06 ForageResource_01 depletes only on AddItem success (or logs item_herbs not found — acceptable if ID mismatch)
- [ ] WAVE06 FishingSpot "Pescar" interaction works
- [ ] No red errors in any step

## Partial Pass (acceptable for WAVE_INTEGRATION_06A gate)

If tool IDs (hoe/axe/pickaxe/watering can) are all skipped:
- Document in audit: WAVE_INTEGRATION_06B needed to register missing tool item IDs
- FarmPlot tests that require EquipmentManager.HasTool(Hoe) may be blocked
- This is acceptable — WAVE06 resource smoke tests do NOT require tool check

## Failure Criteria (blocks WAVE_INTEGRATION_07)

- [ ] TreeResource_01/RockResource_01/ForageResource_01 deplete before AddItem confirms success
- [ ] Any resource visual changes to Depleted when inventory is full
- [ ] GameBootstrap.Instance is null in Play Mode (FarmScene not regenerated)
- [ ] Provisioner throws red exception (unexpected AddItem API change)
- [ ] FishingSpot "Pescar" prompt missing (scene not regenerated or FishingSpot wiring broken)

---

## Executor Notes (fill in during Play Mode)

Date executed: ___________
Unity version: ___________
Added items: ___________
Skipped items: ___________
WAVE05 result: ___________
WAVE06 Tree result: ___________
WAVE06 Rock result: ___________
WAVE06 Forage result: ___________
WAVE06 Lake/Fishing result: ___________
AddItem safety check: ___________
Red errors: ___________
Overall result: PASS / PARTIAL_PASS / FAIL
Can start WAVE_INTEGRATION_07: YES / NO
