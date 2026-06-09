# WAVE_INTEGRATION_09 — Human Play Mode Checklist

**Wave:** WAVE_INTEGRATION_09 — Inventory Tooltip Equipment Runtime Binding  
**Date:** 2026-06-08  
**Status:** PENDING_HUMAN_EXECUTION  
**Executor:** [Human name]  
**Execution date:** [Date]

---

## Prerequisites

Before running this checklist:

1. Open Unity Editor (Unity LTS)
2. Run menu: **CindarsHope / Create MVP Farm Scene**
   - Wait for scene generation to complete
   - Verify Console shows: "MVP FarmScene created at Assets/_Game/Scenes/FarmScene.unity"
3. Verify scene contains GameObject named `GameplayInputRouter`
4. Open Play Mode (press Play button)
5. Provision test items via: **CindarsHope / Integration / Debug / Provision Farm Smoke Loadout**
   - This gives the player starting items to verify inventory contents

---

## Section 1 — Inventory Open/Close

| # | Test | Expected | Result |
|---|------|----------|--------|
| 1.1 | Press **I** key while in gameplay | Inventory panel opens (IMGUI window centered) | [ ] PASS / [ ] FAIL |
| 1.2 | Inventory shows slot grid (5 rows x 6 columns) | Grid visible with slot numbers | [ ] PASS / [ ] FAIL |
| 1.3 | Press **Esc** while inventory is open | Inventory closes | [ ] PASS / [ ] FAIL |
| 1.4 | Press **I** again | Inventory reopens | [ ] PASS / [ ] FAIL |
| 1.5 | Click **Fechar** button | Inventory closes | [ ] PASS / [ ] FAIL |

---

## Section 2 — Real Items Display

| # | Test | Expected | Result |
|---|------|----------|--------|
| 2.1 | Open inventory after provisioning loadout | Slots show item IDs (e.g., "hoe_basic x1", "seeds_potato x5") | [ ] PASS / [ ] FAIL |
| 2.2 | Header shows "(X/30)" | Non-zero filled count shown | [ ] PASS / [ ] FAIL |
| 2.3 | Harvest a crop (interact with FarmPlot) | Inventory count increases | [ ] PASS / [ ] FAIL |
| 2.4 | Open inventory after harvest | New item appears in slots | [ ] PASS / [ ] FAIL |

---

## Section 3 — Minimal Tooltip

| # | Test | Expected | Result |
|---|------|----------|--------|
| 3.1 | Open inventory, see "Selected: empty" when no slot selected | Shows "Selected: empty" | [ ] PASS / [ ] FAIL |
| 3.2 | Use W/A/S/D or click a slot with an item | "Selected: [itemId] x[amount]" appears below grid | [ ] PASS / [ ] FAIL |
| 3.3 | Navigate to another item slot | Selected details update | [ ] PASS / [ ] FAIL |

---

## Section 4 — Equipment Panel

| # | Test | Expected | Result |
|---|------|----------|--------|
| 4.1 | Press **L** key during gameplay | Equipment panel opens showing equipment slots | [ ] PASS / [ ] FAIL |
| 4.2 | Equipment panel shows RightHand, LeftHand, Chest, Accessory slots | Slots listed with current state | [ ] PASS / [ ] FAIL |
| 4.3 | Press **Esc** while equipment panel open | Panel closes | [ ] PASS / [ ] FAIL |
| 4.4 | Press **K** key | Attributes panel opens (Level, XP, attribute points) | [ ] PASS / [ ] FAIL |
| 4.5 | Click **Fechar (Esc)** button | Panel closes | [ ] PASS / [ ] FAIL |

---

## Section 5 — Equip Item from Inventory

| # | Test | Expected | Result |
|---|------|----------|--------|
| 5.1 | Open inventory (I), navigate to an equippable item | Item shown in slot |  [ ] PASS / [ ] FAIL / [ ] N/A |
| 5.2 | Press E or Enter to open actions menu | Actions: Use/Equipar/Drop/Destroy/Split/Cancel appear | [ ] PASS / [ ] FAIL / [ ] N/A |
| 5.3 | Select "Equipar" | Message "Item equipado em [slot]" | [ ] PASS / [ ] FAIL / [ ] N/A |
| 5.4 | Open equipment panel (L) | Slot now shows equipped item | [ ] PASS / [ ] FAIL / [ ] N/A |

---

## Section 6 — Input/Movement Blocking

| # | Test | Expected | Result |
|---|------|----------|--------|
| 6.1 | Open inventory (I), try to move player (WASD) | Player does NOT move | [ ] PASS / [ ] FAIL |
| 6.2 | Close inventory (Esc), try to move player | Player moves normally | [ ] PASS / [ ] FAIL |
| 6.3 | Open inventory, try to harvest/interact | Interaction blocked or not triggered | [ ] PASS / [ ] FAIL |

---

## Section 7 — No Duplication / Stability

| # | Test | Expected | Result |
|---|------|----------|--------|
| 7.1 | Open inventory, close, open again multiple times | No second panel appears; no duplication | [ ] PASS / [ ] FAIL |
| 7.2 | Open both inventory and equipment panel in succession | Only one IMGUI panel visible at a time (inventory takes priority when pushed to modal) | [ ] PASS / [ ] FAIL |
| 7.3 | Load into FarmScene, check Console for errors | No NullReferenceException, no "not found" errors for InventoryPanelController | [ ] PASS / [ ] FAIL |

---

## Section 8 — GameplayInputRouter

| # | Test | Expected | Result |
|---|------|----------|--------|
| 8.1 | Check Hierarchy for `GameplayInputRouter` GameObject | Found in scene | [ ] PASS / [ ] FAIL |
| 8.2 | Open inventory, press I again | Does NOT toggle twice (modal already open, GameplayInputRouter blocks) | [ ] PASS / [ ] FAIL |

---

## Section 9 — Validator (Optional)

| # | Test | Expected | Result |
|---|------|----------|--------|
| 9.1 | Exit Play Mode, run **CindarsHope / Validate / Validate Inventory Runtime** | "PASS: Inventory runtime binding validated successfully." dialog | [ ] PASS / [ ] FAIL / [ ] SKIP |

---

## Known Issues / UI Debt (Do Not Fail)

The following are known DEBT items from WAVE_INTEGRATION_09 — do not mark as failures:

- Tooltip shows raw itemId instead of display name (ItemTooltipViewModel not wired to panel)
- Equipment panel (L key) does not block player movement via ModalManager (only inventory panel does)
- GameplayInputRouter publishes InventoryPanelOpenedEvent but InventoryPanelController does not subscribe (handles its own I key)

---

## Result Summary

| Section | PASS | FAIL | N/A |
|---------|------|------|-----|
| 1 — Inventory Open/Close | | | |
| 2 — Real Items Display | | | |
| 3 — Minimal Tooltip | | | |
| 4 — Equipment Panel | | | |
| 5 — Equip Item | | | |
| 6 — Input Blocking | | | |
| 7 — No Duplication | | | |
| 8 — GameplayInputRouter | | | |

**Overall Result:** [ ] PLAYMODE_PASS / [ ] PLAYMODE_PARTIAL / [ ] PLAYMODE_FAIL

**Notes:**

---

*Created: 2026-06-08 (WAVE_INTEGRATION_09)*
