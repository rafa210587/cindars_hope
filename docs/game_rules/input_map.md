---
doc_type: game_rule
status: accepted
domain: input-controls
source_adrs:
  - ADR-0013
source_documents:
  - Assets/_Game/Scripts/** (KeyCode. scan, 2026-06-19)
  - .specs/a_implementar/fable/fable_67_spec_canonical_governance_input_map_adr.md
last_reviewed: 2026-06-19
---

# Input Map (Keyboard/Mouse) — Cindar's Hope

## Purpose

Single canonical source of truth for the keyboard input the game actually consumes.
Derived from a mechanical scan of `KeyCode.` usages across `Assets/_Game/Scripts/**`
(≈137 occurrences in 33 files on 2026-06-19). Every binding below is grounded in the
`.cs` file that reads it — there is no "designed but unwired" key in the canonical
section.

Input v1 is keyboard + mouse only (see ADR-0013). This document does not introduce
rebind/accessibility — it records what exists so future specs stop choosing keys blind.

> **Why this exists:** before this map a key only "existed" inside the single `.cs`
> that consumed it. Specs of UI/ability picked keys in the dark, with real collision
> risk. F62 (in-game hints + Controls screen) consumes this document as its canonical
> source of control text.

---

## CANONICAL RULE — Consult and update this map

- **Any spec that adds or changes a key binding MUST first consult this map**, then
  **update it in the same diff** that touches the consuming `.cs`.
- A new binding is canonical only when (a) a runtime `.cs` reads it AND (b) it appears
  in the Gameplay or Panels table below.
- Debug/dev keys (separate section) are **non-contract** — never cite them in player
  hints, the Controls screen, or acceptance criteria.
- Conflicts (same key, overlapping live contexts) are listed under "Findings". This map
  records them; it does **not** resolve them. Resolution is a future code spec.

---

## Canonical bindings — Gameplay (world / combat)

| Key | Action | Context | Consuming file |
|---|---|---|---|
| W / Up Arrow | Move up | Overworld movement | `Player/PlayerController.cs`, `Player/Movement/PlayerMovementActionInput.cs` |
| S / Down Arrow | Move down | Overworld movement | `Player/PlayerController.cs`, `Player/Movement/PlayerMovementActionInput.cs` |
| A / Left Arrow | Move left | Overworld movement | `Player/PlayerController.cs`, `Player/Movement/PlayerMovementActionInput.cs` |
| D / Right Arrow | Move right | Overworld movement | `Player/PlayerController.cs`, `Player/Movement/PlayerMovementActionInput.cs` |
| E | Interact / use / pick up | Overworld (no modal open) | `Interaction/InteractionSystem.cs` (`_interactKey = E`) |
| E | Light attack | Combat (no modal open) | `Combat/PlayerAttackController.cs` |
| E (hold) | Right-hand charged attack (release fires) | Combat | `Combat/PlayerAttackController.cs` |
| Q | Heavy attack | Combat (no modal open) | `Combat/PlayerAttackController.cs` |
| Q (hold) | Left-hand charged attack (release fires) | Combat | `Combat/PlayerAttackController.cs` |
| Space | Dodge | Movement actions (no modal open) | `Player/PlayerDodgeController.cs`, `Player/Movement/PlayerMovementActionInput.cs` |
| Left Shift (hold) | Block / guard (slow while held) | Movement actions | `Player/Movement/PlayerMovementActionInput.cs` |
| Double-tap W/A/S/D or arrows | Directional dodge/dash trigger | Movement actions | `Player/Movement/DirectionalDoubleTapDetector.cs` |
| H | Consume food | Overworld (no modal open) | `Player/FoodConsumer.cs` (`_consumeKey = H`) |
| 1 / 2 / 3 / 4 | Use active skill slot 1-4 | Combat/overworld (no modal open) | `Skills/Runtime/Effects/ActiveSkillExecutionController.cs` (`Alpha1..Alpha4`) |
| R / T / Y / G | Equip selected skill into active slot 1-4 | Active-slot equip / skill tree | `Skills/ActiveSkillSlots.cs` (`R,T,Y,G`); also `UI/Skills/SkillTreePanel.cs`, `UI/Skills/SkillTreeGameplayPanelController.cs` |

## Canonical bindings — Panels / Modals (open, close, navigate)

| Key | Action | Context | Consuming file |
|---|---|---|---|
| I | Toggle Inventory panel | Overworld (no modal) | `UI/Input/GameplayInputRouter.cs`, `UI/InventoryPanelController.cs` |
| K | Toggle Character/Equipment panel | Overworld (no modal) | `UI/Input/GameplayInputRouter.cs`, `UI/Character/CharacterEquipmentPanelController.cs` |
| L | Toggle Attributes view (equipment panel) | Overworld (no modal) | `UI/Character/CharacterEquipmentPanelController.cs` |
| U | Toggle Skill Tree panel | Overworld (no modal) | `UI/Input/GameplayInputRouter.cs`, `UI/Skills/SkillTreeInputHandler.cs`, `UI/Skills/SkillTreeGameplayPanelController.cs`, `UI/Skills/SkillTreePanel.cs` |
| J | Toggle Quest Log panel | Overworld (no modal) | `UI/Quests/Runtime/QuestLogRuntimeBinder.cs` |
| C | Open pocket crafting | Overworld (no modal) | `UI/Crafting/CraftingModal.cs` (`_pocketCraftKey = C`) |
| Esc | Close current modal / back | Any open modal | `UI/Input/GameplayInputRouter.cs` and per-panel: Inventory, Equipment, SkillTree, QuestLog, Crafting, Shop, Dialogue, Fonte, NPC, Cave checkpoint |
| W/S or Up/Down Arrow | Navigate options (vertical) | Inside a modal | `CraftingModal`, `CharacterEquipmentPanelController`, `InventoryPanelController`, `UI/Shop/ShopMenuModal.cs`, `UI/Dialogue/DialogueModal.cs`, `UI/Skills/SkillTreePanel.cs`, `UI/Skills/SkillTreeGameplayPanelController.cs`, `UI/Cave/CaveCheckpointSideMenuController.cs`, `Cave/Runtime/CaveCheckpointSelectionUI.cs` |
| A/D or Left/Right Arrow | Navigate options (horizontal) | Inside a modal | `CraftingModal`, `InventoryPanelController`, `SkillTreeGameplayPanelController` |
| Return / Enter / Space / E | Confirm / select / advance | Inside a modal | `CraftingModal`, `CharacterEquipmentPanelController`, `InventoryPanelController`, `ShopMenuModal`, `DialogueModal`, `SkillTreePanel` (`Return`/`KeypadEnter`), `SkillTreeGameplayPanelController`, `CaveCheckpointSideMenuController`, `CaveCheckpointSelectionUI` |
| Q / E | Previous / next skill tree | Inside Skill Tree panel | `UI/Skills/SkillTreePanel.cs` |

> Note: in farm crop interaction (`Farm/FarmPlot.cs`) the same modal-style keys apply —
> W/S to pick action, E/Return/Space to confirm, Esc to cancel.

---

## DEBUG / DEV KEYS — NON-CANONICAL (not a contract)

> These keys exist for development and smoke testing only. They are **not** part of the
> player control contract, **must not** appear in hints or the Controls screen, and may
> change or be removed without a superseding decision.

| Key | Action | Consuming file | Note |
|---|---|---|---|
| Tab | Advance day (force) | `Core/Time/DayAdvanceInput.cs` (`_advanceDayKey = Tab`) | Time-skip dev utility; suppressed while a modal is open |
| F5 | Save game | `Save/SaveInput.cs` (`_saveKey = F5`) | Dev quick-save |
| F9 | Load game | `Save/SaveInput.cs` (`_loadKey = F9`) | Dev quick-load |
| B | Cycle debug tool | `Equipment/EquipmentManager.cs` (`CycleDebugTool()`) | Dev tool cycling; suppressed while a modal is open |
| O | Grant debug XP | `UI/DebugHud.cs` (`GrantDebugXp()`) | Dev XP grant |
| 1-6 (Alpha1..Alpha6) | Select hotbar slot 0-5 | `UI/Hotbar/HotbarDebugInput.cs` | Debug hotbar selector (class name `HotbarDebugInput`); overlaps with canonical 1-4 active-slot use — see Findings |
| P / F2 | Skip to next cave level/gate | `Cave/Runtime/CaveDebugLevelSkipController.cs` (`_nextGateKey = P`, `_alternateNextGateKey = F2`) | Cave debug level skip; defaults also written by `Editor/SceneCreation/CreateMvpCaveScene.cs` |
| Left Shift + R | Regenerate current cave level | `Cave/CaveLevelRuntimeController.cs` | Explicit debug run regeneration (matches ADR-0005 allowed regen trigger) |

---

## Findings — collisions / ambiguities (recorded, NOT resolved)

These were detected during the scan. Resolution (if any) is a future **code** spec; this
governance spec only records them.

1. **Hotbar 1-4 vs. active-skill 1-4 (real overlap).** `ActiveSkillExecutionController`
   uses `Alpha1..Alpha4` to *use* active skill slots, while `HotbarDebugInput` uses
   `Alpha1..Alpha6` to *select* hotbar slots. Both are number-row bindings. `HotbarDebugInput`
   is debug-flagged; the canonical meaning of 1-4 is "use active skill slot".
2. **Two Skill Tree panel implementations bind U + Esc.** Both `UI/Skills/SkillTreePanel.cs`
   and `UI/Skills/SkillTreeGameplayPanelController.cs` (plus `SkillTreeInputHandler.cs`)
   read `U`/`Escape`. `SkillTreeGameplayPanelController` is the runtime singleton wired in
   WAVE_INTEGRATION_10; the other is a parallel/older view. Only one should own the binding.
3. **R/T/Y/G consumed by three classes.** `ActiveSkillSlots` (runtime equip), `SkillTreePanel`
   and `SkillTreeGameplayPanelController` all read R/T/Y/G for slot assignment. Functionally
   the same intent, but three consumers means a future single-owner cleanup is advisable.
4. **Q/E and Space are context-overloaded.** Q/E = attacks in combat but prev-tree/confirm
   inside modals; Space = dodge in the world but confirm inside modals and charge-release in
   combat. These are gated by "modal open?" checks today, so they are context-separated rather
   than hard collisions — listed for awareness.
5. **Tab is day-advance, not a panel key.** fable_67's scope text speculatively grouped `Tab`
   under panels; the only live consumer (`DayAdvanceInput`) uses Tab to force-advance the day.
   Classified here as a debug/dev utility, not a panel toggle.

---

## Related ADRs

- [ADR-0013: Input Keyboard/Mouse Only v1](../decisions/ADR-0013-input-keyboard-mouse-only-v1.md)
- [ADR-0011: Pixel Art Scale](../decisions/ADR-0011-pixel-art-scale-32px-per-tile.md) (HUD/screen context)

---

*Last Reviewed: 2026-06-19 (fable_67)*
*Source: mechanical `KeyCode.` scan of `Assets/_Game/Scripts/**` (≈137 occurrences, 33 files)*
*Consumed by: F62 (in-game hints + Controls screen) and every future UI/ability key spec*
