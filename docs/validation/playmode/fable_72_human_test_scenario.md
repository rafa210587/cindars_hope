# Human Play Mode Scenario — fable_72 NPC Gift Giving (Taste Runtime)

> **Spec:** `fable_72_spec_npc_gift_giving_taste_runtime`
> **Status:** DEFERRED_TO_FINAL_VALIDATION (Play Mode not run in implementation session)
> **Prerequisite:** F26 (FriendshipService) bootstrapped; `GiftGivingService` bootstrap active;
> TownScene with at least one shop NPC (e.g. Sylveth, Brumdar, Renko); InventoryManager populated
> with mapped gift items (see `GiftTasteMatrixData` for ids, e.g. `item_material_iron_ore`,
> `item_crop_alihana_tear`, `item_consumable_food_vale_wine`, an `item_undocumented_*`/blackmarket id).

> **Note on current UI debt:** the item-selector UI is deferred. Until a picker is wired, exercise
> the flow programmatically via a debug call to `GiftGivingService.Instance.TryGiveGift(npcId, itemId)`
> (e.g. a temporary debug menu/button), OR validate via the EditMode tests. The "Dar presente"
> dialogue choice currently surfaces an entry-point toast only.

---

## Setup

1. Enter Play Mode in TownScene.
2. Confirm `FriendshipService` and `GiftGivingService` exist (console logs from their bootstraps).
3. Give the player a stack of a mapped loved/liked/hated item via the debug loadout provisioner.

## Scenario A — Loved gift raises friendship a lot (+12)

1. With Sylveth (loves `item_crop_alihana_tear`), call `TryGiveGift("npc_sylveth", "item_crop_alihana_tear")`.
2. EXPECT: outcome `Accepted`; friendship points for `npc_sylveth` increase by **12**; **1** unit of
   the item is consumed; a `NpcGiftReactionEvent` is published (Taste=Loved, AppliedDelta=12).
3. EXPECT: if the point total crosses a level threshold, F26 publishes `FriendshipLevelChangedEvent`
   (rising). This spec does NOT re-emit it.

## Scenario B — Hated gift LOWERS friendship (-6)

1. Pick an NPC + an item that is hated for them (e.g. a blackmarket/undocumented item for `npc_corvus`).
2. First raise that NPC's points above 6 (e.g. a few accepted gifts on prior days, or quest).
3. Call `TryGiveGift` with the hated item.
4. EXPECT: outcome `Accepted`; points DECREASE by **6** (clamped at 0 by F26 — never negative);
   1 unit consumed; `NpcGiftReactionEvent` (Taste=Hated, AppliedDelta=-6). If it crosses a threshold
   downward, F26 publishes `FriendshipLevelChangedEvent` (falling).

## Scenario C — Daily limit (second gift same day refused, item kept)

1. Give any accepted gift to an NPC today (Scenario A).
2. Immediately try a second gift to the SAME NPC the same day.
3. EXPECT: outcome `RefusedDailyLimit`; friendship UNCHANGED; the item is **NOT** consumed
   (still in inventory); no `NpcGiftReactionEvent`.
4. Advance one in-game day (sleep). Try again.
5. EXPECT: accepted again; delta applied; 1 unit consumed.

## Scenario D — Non-Giftable item refused silently (no loss)

1. Try `TryGiveGift` with an item id NOT mapped in `GiftTasteMatrixData` (e.g. a tool/equipment id).
2. EXPECT: outcome `RefusedNotGiftable`; friendship UNCHANGED; item **NOT** consumed; no event.

## Scenario E — Neutral fallback for an NPC without mapped taste

1. Use a mapped Giftable item with an NPC NOT in the matrix (if any test NPC is unmapped).
2. EXPECT: outcome `Accepted`; Taste=Neutral; delta **+2**; 1 unit consumed.

## Scenario F — Dialogue hook present and non-regressive

1. Interact with a shop NPC; open the root menu.
2. EXPECT: choices include "Conversar", "Comprar", "Vender", and the new "Dar presente"; buy/sell/talk
   all still work exactly as before (no regression).
3. Choose "Dar presente": EXPECT the entry-point toast ("Escolha um presente no inventario…").

---

## Pass criteria

- A–E friendship deltas and consumption match the spec (loved +12 / liked +6 / neutral +2 /
  disliked -2 / hated -6; clamp at 0 handled by F26).
- Daily cap blocks the 2nd same-day gift without consuming the item; next day allows.
- Non-Giftable is a silent no-op (no loss, no consume, no event).
- Buy/sell/talk flows unchanged; "Dar presente" choice present.

## Editor validator (run once in Editor)

- Menu: `CindarsHope/Validate/Validate Gift Taste Matrix`.
- EXPECT: log "0 erros"; WARNINGs are acceptable (A4 debt: LovedItemIds not yet materialized as
  tagged items). A non-zero error count is a failure (e.g. a roster NPC without preferences or a
  tag outside the §3 vocabulary).
