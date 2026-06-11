# WAVE_INTEGRATION_23 — Existing UI Audit

**Date:** 2026-06-10
**Auditor:** WAVE23 agent
**Purpose:** Map all existing UI before creating anything new

---

## Existing HUD Components (Reuse — Do NOT Recreate)

| Component | File | Status | Notes |
|---|---|---|---|
| `GameplayHudViewModel` | `UI/HUD/HUDGameplayViewModel.cs` | ✓ EXISTS | Full ViewModel with HP/Stamina/MP/Hunger/QuestPrompt/ContextPrompt/ActiveSkills |
| `HUDGameplayViewModel` | same file | ✓ EXISTS | Legacy alias → GameplayHudViewModel |
| `FinalHudGuardValidator` | `UI/HUD/FinalHudGuardValidator.cs` | ✓ EXISTS | Active slot cap, forbidden field IDs, Dash/Dodge/Block slot guard |
| `HotbarSlotViewModel` | `UI/HUD/HotbarSlotViewModel.cs` | ✓ EXISTS | Hotbar slot data |
| `ActiveSkillSlotViewModel` | same file | ✓ EXISTS | Active skill slot (R/T/Y/G keys) |
| `StatusBuffProjection` | same file | ✓ EXISTS | Status effects/buffs |
| `ContextPromptProjection` | same file | ✓ EXISTS | Interaction prompt data |
| `DebugHudProjection` | `UI/Debug/DebugHudProjection.cs` | ✓ EXISTS | Dev-only debug data |

## Existing Dev HUD (Keep — Complement, Not Replace)

| Component | File | Status | Notes |
|---|---|---|---|
| `DebugHud` | `UI/DebugHud.cs` | ✓ EXISTS | IMGUI dev overlay — 750 lines |
| Event subscriptions | same | ✓ WIRED | 15 events: HP, Stamina, Quest, Cave, Interaction, Economy, Progression, etc. |
| `RebindRuntimeReferences` | same | ✓ EXISTS | Called by GameBootstrap after scene load |

**Decision:** DebugHud remains unchanged. GameplayHudRuntimeBinder subscribes to the same events independently — no coupling between the two HUDs.

## Existing Modal System (Integrate — Do NOT Replace)

| Component | Status | Notes |
|---|---|---|
| `ModalManager` | ✓ EXISTS | Stack-based modal system with `HasActiveModal`, `PushModal`, `TryPopModal` |
| `ModalType` enum | ✓ EXISTS | 15 types including QuestLog (added WAVE19) |
| `ModalBase` | ✓ EXISTS | Base class for modal UI |

**Key fact:** `ModalManager` has NO open/close events. HUD must poll `HasActiveModal` in Update.

## Existing Input Router (Integrate — Do NOT Replace)

| Component | Status | Notes |
|---|---|---|
| `GameplayInputRouter` | ✓ EXISTS | Routes I/K/U/Esc keys via GameEventBus |
| Modal guard | ✓ EXISTS | `if (hasModal) return;` blocks all shortcuts when modal open |

**Key fact:** `GameplayInputRouter` already handles input blocking during modals. `HudVisibilityController` only needs to sync visual state, not input.

## Existing Events Available for HUD Binding

| Event | Source | Used For |
|---|---|---|
| `HPChangedEvent` | PlayerManager | Status bars HP |
| `StaminaChangedEvent` | StaminaManager | Status bars Stamina |
| `ManaChangedEvent` | ManaManager | Status bars Mana |
| `HungerChangedEvent` | HungerManager | Secondary needs compact |
| `InteractionPromptChangedEvent` | InteractionSystem | Interaction prompt |
| `QuestAcceptedEvent` | QuestService | Quest tracker |
| `QuestObjectiveProgressedEvent` | QuestService | Quest tracker progress |
| `QuestReadyToCompleteEvent` | QuestService | Quest tracker ready |
| `QuestCompletedEvent` | QuestService | Quest tracker cleared |
| `PlayerActionFeedbackEvent` | various | Feedback toast |
| `GameSavedEvent` | SaveManager | Feedback toast |
| `GameLoadedEvent` | SaveManager | Feedback toast |
| `EnemyKilledEvent` | combat | Feedback toast |
| `CaveLevelEnteredEvent` | CaveRunManager | Feedback toast |
| `CaveExitedEvent` | CaveRunManager | Feedback toast |
| `NotificationToastRequestedEvent` | various | Feedback toast |
| `QuestRewardClaimedEvent` | QuestService | Feedback toast |

## Existing Bootstrap Pattern (Reference)

`PlayerMovementActionRuntimeBootstrap` uses:
```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
private static void EnsureInstance()
{
    if (_instance != null) return;
    var go = new GameObject("...");
    DontDestroyOnLoad(go);
    _instance = go.AddComponent<...>();
}
```

**WAVE23 GameplayHudBootstrap uses the same pattern.**

## Existing Notification System (Related — Not Replaced)

| Component | Status | Notes |
|---|---|---|
| `NotificationViewModel` | ✓ EXISTS | Priority, StackPolicy, SpoilerTier |
| `NotificationQueuePolicy` | ✓ EXISTS | MaxVisible, Duration, DebugOnly filter |
| `NotificationToastController` | ✓ EXISTS | Scene-based toast controller |

**Decision:** `GameplayFeedbackService` is simpler than `NotificationViewModel` (only text+duration+priority). It uses `NotificationToastRequestedEvent` as one of its input sources but does NOT replace the notification system.

---

## What WAVE23 Creates (vs What Exists)

| What | Status Before WAVE23 |
|---|---|
| `HudEvents.cs` (HudFeedbackUpdatedEvent, HudVisibilityChangedEvent) | NEW |
| `GameplayFeedbackMessage.cs` | NEW |
| `GameplayFeedbackService.cs` | NEW |
| `HudVisibilityController.cs` | NEW |
| `GameplayHudRuntimeBinder.cs` | NEW |
| `GameplayHudCanvasController.cs` | NEW |
| `GameplayHudBootstrap.cs` | NEW |
| `Views/StatusBarsHudView.cs` | NEW (headless) |
| `Views/QuestTrackerHudView.cs` | NEW (headless) |
| `Views/ActiveSkillSlotsHudView.cs` | NEW (headless) |
| `Views/InteractionPromptHudView.cs` | NEW (headless) |
| `Views/FeedbackToastHudView.cs` | NEW (headless) |
| `Views/ModalBlockerHudView.cs` | NEW (headless) |

---

*Created: 2026-06-10 (WAVE23 audit)*
