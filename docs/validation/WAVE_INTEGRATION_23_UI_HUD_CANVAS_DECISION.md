# WAVE_INTEGRATION_23 — UI/HUD Canvas Finalization: Decision Report

**Date:** 2026-06-10
**Status:** `IN_EXECUTION`
**Branch:** dev

---

## Preflight

```
Branch:              dev
Last commit:         37dfea0
Working tree:        M TownScene.unity (pre-existing, non-blocking)
WAVE20 gate:         SATISFIED — WAVE_INTEGRATION_20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md exists
WAVE21 gate:         SATISFIED — WAVE_INTEGRATION_21_POST_ACCEPTANCE_BUGFIX_REPORT.md exists (NO_OP)
WAVE22 gate:         SATISFIED — WAVE_INTEGRATION_22_DEBT_BACKLOG_REPORT.md exists
Open P0:             0
Open P1 (código):    0
Can proceed:         YES
```

---

## Scope Decision

### What This Spec Does

WAVE23 creates a **runtime Canvas HUD system** that:
1. Bootstraps via `RuntimeInitializeOnLoadMethod` — no scene editing required
2. Creates a `GameplayHudCanvas` GameObject programmatically at runtime
3. Maintains a `GameplayHudViewModel` (already existed since WAVE11) updated via GameEventBus
4. Has headless "view" components (MonoBehaviours) that contain logic contracts
5. Hides HUD elements when modals are active via `HudVisibilityController`
6. Provides player-facing feedback via `GameplayFeedbackService`

### What This Spec Does NOT Do

- Does NOT recreate DebugHud (IMGUI dev tool — kept as-is)
- Does NOT recreate Inventory/QuestLog/SkillTree/Shop UI (already exist)
- Does NOT modify ModalManager, GameBootstrap, or any existing system
- Does NOT edit .unity/.prefab/.asset YAML files
- Does NOT create Canvas prefabs (deferred to human Unity Editor work)
- Does NOT add visual UI elements (Image, Text components) — headless contracts only

---

## Architecture Decision

### Canvas Bootstrap Strategy

**Decision:** `RuntimeInitializeOnLoadMethod(AfterSceneLoad)` pattern (same as `PlayerMovementActionRuntimeBootstrap`).

**Why:** Avoids scene editing entirely. The HUD is a DontDestroyOnLoad singleton created programmatically. Same proven pattern as WAVE12 movement controllers.

### View Architecture

**Decision:** Headless MonoBehaviour views with `Initialize(GameplayHudViewModel)` contract.

**Why:** The actual Canvas visual elements (Text, Image, Slider) require human Unity Editor wiring. The views hold the data binding logic but have empty visual refresh stubs. This means:
- Code compiles ✓
- Logic is implemented and testable ✓
- Visual wiring is a human task (Unity Editor) ✓
- No hardcoded coordinates or sprite references ✓

### HUD Visibility Gate

**Decision:** Poll `ModalManager.HasActiveModal` in Update via `HudVisibilityController`, publish `HudVisibilityChangedEvent` on change.

**Why:** ModalManager has no open/close events. Polling is simple and efficient (one bool check per frame). Event published only on transition (not every frame).

### Feedback Service

**Decision:** Separate `GameplayFeedbackService` MonoBehaviour with message queue and priority.

**Why:** Toast feedback needs de-duplication, priority ordering, and duration management. Keeping it separate from the binder keeps responsibilities clear.

---

## Files Authorized (WAVE23 Scope)

### New Runtime Code

| File | Purpose |
|---|---|
| `Assets/_Game/Scripts/Core/Events/HudEvents.cs` | HudFeedbackUpdatedEvent, HudVisibilityChangedEvent |
| `Assets/_Game/Scripts/UI/HUD/GameplayFeedbackMessage.cs` | Data class for feedback messages |
| `Assets/_Game/Scripts/UI/HUD/GameplayFeedbackService.cs` | Event-driven feedback queue |
| `Assets/_Game/Scripts/UI/HUD/HudVisibilityController.cs` | Polls ModalManager, publishes visibility events |
| `Assets/_Game/Scripts/UI/HUD/GameplayHudRuntimeBinder.cs` | Binds game events → GameplayHudViewModel |
| `Assets/_Game/Scripts/UI/HUD/GameplayHudCanvasController.cs` | Canvas lifecycle and view coordination |
| `Assets/_Game/Scripts/UI/HUD/GameplayHudBootstrap.cs` | RuntimeInitializeOnLoadMethod entry |
| `Assets/_Game/Scripts/UI/HUD/Views/StatusBarsHudView.cs` | HP/Stamina/Mana headless view |
| `Assets/_Game/Scripts/UI/HUD/Views/QuestTrackerHudView.cs` | Quest tracker headless view |
| `Assets/_Game/Scripts/UI/HUD/Views/ActiveSkillSlotsHudView.cs` | Active skill slots headless view |
| `Assets/_Game/Scripts/UI/HUD/Views/InteractionPromptHudView.cs` | Interaction prompt headless view |
| `Assets/_Game/Scripts/UI/HUD/Views/FeedbackToastHudView.cs` | Feedback toast headless view |
| `Assets/_Game/Scripts/UI/HUD/Views/ModalBlockerHudView.cs` | HUD visibility gate |

### New Editor Code

| File | Purpose |
|---|---|
| `Assets/_Game/Scripts/Editor/Validation/ValidateWave23UiHudCanvasFinalization.cs` | 27-check validator |

### Existing Code (NOT Modified)

| File | Status |
|---|---|
| `Assets/_Game/Scripts/UI/DebugHud.cs` | KEPT as-is |
| `Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs` | KEPT as-is (legacy alias) |
| `Assets/_Game/Scripts/UI/HUD/GameplayHudViewModel.cs` | KEPT as-is (already implemented) |
| `Assets/_Game/Scripts/UI/HUD/FinalHudGuardValidator.cs` | KEPT as-is |
| `Assets/_Game/Scripts/UI/HUD/HotbarSlotViewModel.cs` | KEPT as-is |
| `Assets/_Game/Scripts/UI/Modal/ModalManager.cs` | NOT MODIFIED |
| `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` | NOT MODIFIED |

---

## Prohibited Files (NOT Touched)

```
Packages/
ProjectSettings/
Assets/_Game/Scenes/*.unity       — no scene editing
Assets/_Game/Prefabs/**           — no prefab creation
Assets/_Game/Data/**              — no asset modification
```

---

## Human Wiring Remaining (Post-WAVE23)

```
1. In Unity Editor: add StatusBarsHudView canvas elements (HP bar, Stamina bar, Mana bar)
2. In Unity Editor: add QuestTrackerHudView Text elements
3. In Unity Editor: add InteractionPromptHudView Text/Image elements
4. In Unity Editor: add FeedbackToastHudView Text element with animation
5. In Unity Editor: add ActiveSkillSlotsHudView slot icons
6. In Unity Editor: wire CanvasGroup to ModalBlockerHudView for alpha control
7. Verify GameplayHudCanvas GameObject appears in DontDestroyOnLoad during Play Mode
```

---

*Created: 2026-06-10 (WAVE23 — MVP+ 01)*
