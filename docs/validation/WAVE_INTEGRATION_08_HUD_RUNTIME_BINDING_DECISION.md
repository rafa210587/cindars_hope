# WAVE_INTEGRATION_08 — HUD Gameplay Runtime Binding Decision

Date: 2026-06-08
Branch: dev
Status: VALIDATED

---

## Target FarmScene

| Field | Value |
|---|---|
| Path | Assets/_Game/Scenes/FarmScene.unity |
| Generator | CindarsHope > Advanced > Legacy > Scenes > Create MVP FarmScene |
| DebugHud already in scene | YES — CreateDebugHud() called in CreateScene() |

---

## UI technology strategy

**REUSE_EXISTING_DEBUG_HUD (IMGUI via OnGUI)**

The DebugHud.cs component already exists, uses Unity's legacy IMGUI system (OnGUI), and is already wired into FarmScene via `CreateDebugHud()` in CreateMvpFarmScene.cs.

It covers:
- Interaction prompt (via `InteractionPromptChangedEvent` + `InteractionSystem.HasCandidate` direct fallback)
- Gold/payout (reads `_playerManager.CurrentGold` every OnGUI frame)
- Action feedback (via `PlayerActionFeedbackEvent`, 3-second display)
- Economy transaction (via `EconomyTransactionCompletedEvent`)
- Inventory (iterates `_inventoryManager.Items` every OnGUI frame)
- Hunger/stamina (via `HungerChangedEvent` + `StaminaChangedEvent`)
- Equipment/hotbar state
- World state (day, scene name)

No Canvas, UI Toolkit, or new Unity UI components needed.

---

## Interaction prompt strategy

**USE_EXISTING_INTERACTION_PROMPT_SOURCE**

`InteractionSystem.cs` publishes `InteractionPromptChangedEvent` when the interactable candidate changes.
`DebugHud.cs` subscribes to this event AND has a direct fallback via `_interactionSystem.HasCandidate`.
Coverage: full.

---

## Gold/payout strategy

**USE_REAL_GOLD_SOURCE**

`DebugHud.DrawPlayerState()` reads `_playerManager.CurrentGold` directly in every `OnGUI()` call (IMGUI polls each frame). Gold update is immediate after `PlayerManager.AddGold()`.
Coverage: full.

---

## Feedback message strategy

**WIRE_SELLPOINT_FEEDBACK_EVENTS**

`SellPoint.cs` previously only called `Debug.Log` after a sale — no event was published.
Adding `PlayerActionFeedbackEvent` and `EconomyTransactionCompletedEvent` publishing to SellPoint.Interact() makes the sale visible in the DebugHud feedback area and economy area.
Coverage: full after this change.

---

## Hotbar strategy

**USE_EXISTING_DEBUGHUD_HOTBAR_DISPLAY**

`DebugHud.DrawEquipmentState()` reads `_saveManager.HotbarState.SelectedItemId` and `SelectedSlotIndex` and displays them in the info panel.
No new hotbar view needed.
Coverage: partial (shows selected slot only, not all 6 slots visually).
Debt: full 6-slot visual hotbar deferred to WAVE_INTEGRATION_10/11.

---

## Active skill slots strategy

**ACTIVE_SKILL_RUNTIME_DEFERRED_TO_WAVE_INTEGRATION_10_11**

`SkillActiveSlotsViewModel.cs` exists but is not connected to a runtime view in DebugHud.
The DebugHud does not show active skill slots.
This is acceptable per spec: 4 empty placeholders are acceptable for WAVE_INTEGRATION_08.
Documenting as debt — not blocking.

---

## Player condition strategy

**USE_REAL_PLAYER_CONDITION_SOURCE**

`DebugHud` subscribes to `HungerChangedEvent` and `StaminaChangedEvent` (via inner components) and reads directly via bootstrap fallback. Coverage: full.

---

## Design/Direction Compliance Matrix

| Direction source | Found? | Rule/constraint extracted | Impact on this spec | Applied? | Evidence |
|---|---:|---|---|---:|---|
| docs/GDD_v2.6.md | NO | Reference not found | N/A | N/A | File absent |
| docs/FASE7_SPEC_MVP_FARM_v2.2.md | NO | Reference not found | N/A | N/A | File absent |
| docs/project/CURRENT_STATE.md | YES | WAVE_INTEGRATION_07 BUILD_VALIDATED; DebugHud present | Confirms baseline; no conflict | YES | CURRENT_STATE.md L86-131 |
| WAVE_04 HUD spec report | NO | Reference not found | N/A | N/A | File absent |
| WAVE_11 closeout report | YES | HUDGameplayViewModel exists; WAVE_11 specs BUILD_VALIDATED | Confirms ViewModels exist; adapter strategy correct | YES | WAVE_11_CLOSEOUT_REPORT.md |
| WAVE_INTEGRATION_07 report | YES | SellPoint wired, economy loop complete | Confirms dependency met | YES | WAVE_INTEGRATION_07_SHIPPING_ECONOMY_REPORT.md |

Result: NO_APPLICABLE_HUD_DIRECTION_FOUND (no GDD/FASE docs present). Using existing code architecture as direction source. No design conflict detected.

---

## Temporary/debt policy

- TODO_INTEGRATION_NOT_FINAL required: NO (DebugHud is functional HUD; feedback events are real wiring)
- HUD fields deferred: Active skill slots (visual); full 6-slot hotbar visual
- Economy payout debt visible: NO — gold shows immediately via IMGUI polling
- Active skill runtime deferred: YES — `ACTIVE_SKILL_RUNTIME_DEFERRED_TO_WAVE_INTEGRATION_10_11`

---

## Scene modification policy

No scene changes needed from code. Human must run CreateMvpFarmScene generator to pick up the new SellPoint (already done in WAVE_INTEGRATION_07). DebugHud is already in the scene. No additional scene modification required for WAVE_INTEGRATION_08.

---

## Human Unity actions required

1. Run `CindarsHope > Advanced > Legacy > Scenes > Create MVP FarmScene` (if not done since WAVE_INTEGRATION_07)
2. Enter Play Mode
3. Execute WAVE_INTEGRATION_08 Human Play Mode checklist
