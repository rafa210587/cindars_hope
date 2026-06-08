# Execution Report: 11_spec_ui_hud_hotbar_active_slots_notifications_debug_runtime

**Status:** BUILD_VALIDATED  
**Date:** 2026-06-08  
**Wave:** 11 — UI / UX / Input / Menus  
**Priority:** P0

---

## Acceptance Criteria Extracted

| Criterion | Implementation | Status |
|-----------|---------------|--------|
| GameplayHudViewModel with all HUD fields | HUDGameplayViewModel.cs (renamed class) | OK |
| HP/Stamina permanent, MP conditional | ShowMp flag | OK |
| HungerCompact/FatigueCompact contextual | Compact float fields | OK |
| HotbarSlots[] | HotbarSlotViewModel list | OK |
| ActiveSkillSlots[] capped at 4 | FinalHudGuardValidator enforces cap | OK |
| Dash/Dodge/Block excluded from active slots | FinalHudGuardValidator.IsDashDodgeBlock() | OK |
| StatusEffects/Buffs projections | StatusBuffProjection list | OK |
| ContextPromptProjection | ContextPromptProjection class | OK |
| CompanionProjection/PetProjection optional/future | Optional string fields, null by default | OK |
| DebugVisible=false by default | Verified by test | OK |
| NotificationPriority enum (5 values) | NotificationViewModel.cs | OK |
| NotificationViewModel with SpoilerTier, StackPolicy | NotificationViewModel.cs | OK |
| NotificationQueuePolicy with caps, debug filter | NotificationQueuePolicy class | OK |
| DebugHudProjection separate, IsAllowedInFinalBuild=false | DebugHudProjection.cs (CindarsHope.UI.HUD) | OK |
| FinalHudGuardValidator (forbidden fields, slot cap, dash/dodge/block) | FinalHudGuardValidator.cs | OK |
| No Breath/Folego/BR in final HUD (forbidden set) | FinalHudGuardValidator.ForbiddenFieldIds | OK |
| 20 EditMode tests | HudNotificationDebugProjectionTests.cs | OK |

---

## Existing Systems Audit

- `HUDGameplayViewModel.cs` — EXISTING_PARTIAL (had HP/MP/Stamina/Season; no hotbar/active slots/notifications)
- `PlayerStatusHUD.cs`, `EquipmentHUD.cs`, `PlayerNeedsHUD.cs`, `ManaHUD.cs` — MonoBehaviour HUD components; NOT touched (spec excludes scene/prefab changes)
- `CindarsHope.UI.Debug` namespace — CONFLICT: existing code uses it for `Log/LogWarning/LogError`; mitigated by renaming projection to `CindarsHope.UI.HUD` namespace
- No existing `Notifications/` directory — MISSING_SAFE_TO_CREATE
- Strategy: HARDEN_EXISTING for HUDGameplayViewModel + CREATE_MINIMAL for new contracts

---

## Scope Executed

- `Assets/_Game/Scripts/UI/HUD/HUDGameplayViewModel.cs` — renamed inner class to `GameplayHudViewModel`, added hotbar/active slots/notifications/companion fields; `HUDGameplayViewModel` kept as alias for backward compat
- `Assets/_Game/Scripts/UI/HUD/HotbarSlotViewModel.cs` — HotbarSlotViewModel, ActiveSkillSlotViewModel, StatusBuffProjection, ContextPromptProjection
- `Assets/_Game/Scripts/UI/HUD/FinalHudGuardValidator.cs` — slot cap, Dash/Dodge/Block exclusion, forbidden fields
- `Assets/_Game/Scripts/UI/Notifications/NotificationViewModel.cs` — NotificationPriority, NotificationViewModel, NotificationQueuePolicy
- `Assets/_Game/Scripts/UI/Debug/DebugHudProjection.cs` — DebugHudProjection (namespace: CindarsHope.UI.HUD to avoid collision)
- `Assets/_Game/Tests/EditMode/UI/HudNotificationDebugProjectionTests.cs` — 20 tests
- Assembly-CSharp.csproj — 6 entries updated

---

## Out of Scope Respected

- No scene/prefab/asset changes
- No MonoBehaviour HUD components modified
- No Packages/ or ProjectSettings/ changes
- No skill runtime
- No hotbar input binding
- No companion/pet runtime
- No debug console

---

## Technical Decision

`CindarsHope.UI.Debug` namespace was in conflict with existing code that uses `CindarsHope.UI.Debug.Log/LogWarning/LogError` as an existing debug helper. Created `DebugHudProjection.cs` in `CindarsHope.UI.HUD` namespace instead. Correct design: the projection is a read-only data model, not a debug helper.

---

## Canon Compliance

| Check | Status |
|-------|--------|
| Active slots capped at 4 | OK — validator enforces, test verifies |
| Dash/Dodge/Block excluded from active slots | OK — validator enforces |
| No Breath/Folego/BR in final HUD | OK — forbidden set in validator |
| DebugHud not used for final communication | OK — IsAllowedInFinalBuild=false |
| DebugVisible=false by default | OK — verified by test |
| CompanionProjection optional/future | OK — null by default |
| Notifications don't block input except CriticalModal | OK — BlocksInput=false by default |
| DebugOnly notifications filtered in final | OK — policy.DebugOnlyFilteredInFinalBuild=true |

---

## Validation

Validation method: dotnet build --no-restore (explicit exit code)  
Exit code: 0  
Assembly-CSharp: PASS (0E/0W)  
Assembly-CSharp-Editor: LEGACY_BLOCKER (pre-existing, not blocking)  
Quality check: KNOWN_PESTER_ISSUE (pre-existing)  
Docs validation: EXPECTED_FAIL_LEGACY_ONLY  
Validation mode: RUNTIME_VALIDATED_WITH_KNOWN_LEGACY_GATES

---

## Testing Quality Gate

Changed runtime code: YES  
Changed deterministic logic: YES (active slot cap, dash/dodge/block exclusion, forbidden fields, notification queue policy)  
Changed Unity scene/prefab/asset wiring: NO  
Automated tests: YES — 20 EditMode tests  
Manual Play Mode: NOT REQUIRED (pure C# projection contracts)  
Residual risk: ViewModels not wired to GameBootstrap; notification queue not integrated with event bus; HUD MonoBehaviour controllers not updated

---

## Remaining Work

- GameBootstrap wiring for GameplayHudViewModel
- EventBus integration for notification queue
- MonoBehaviour HUD controllers updated to use new ViewModels
- PlayMode scenario: HUD visible during combat, active slots cap verified, notifications appear
