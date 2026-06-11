# WAVE_INTEGRATION_23 — HUD Data Binding Matrix

**Date:** 2026-06-10

---

## ViewModel → View Binding

| ViewModel Field | Source Event | View Component | Headless? | Human Wiring Needed |
|---|---|---|---|---|
| `Hp` / `MaxHp` | `HPChangedEvent` | `StatusBarsHudView` | YES | HP bar Image/Slider fill |
| `Stamina` / `MaxStamina` | `StaminaChangedEvent` | `StatusBarsHudView` | YES | Stamina bar fill |
| `Mp` / `MaxMp` + `ShowMp` | `ManaChangedEvent` | `StatusBarsHudView` | YES | Mana bar fill (conditional) |
| `HungerCompact` | `HungerChangedEvent` | `StatusBarsHudView` | YES | Hunger compact indicator |
| `QuestPrompt` | Quest events | `QuestTrackerHudView` | YES | Text component |
| `ContextPrompt` | `InteractionPromptChangedEvent` | `InteractionPromptHudView` | YES | Prompt Text + visibility |
| `ActiveSkillSlots` (4) | Update poll via SkillTreeManager | `ActiveSkillSlotsHudView` | YES | Slot icons (R/T/Y/G) |
| (feedback) | `GameplayFeedbackService` | `FeedbackToastHudView` | YES | Toast Text + animation |
| (visibility) | `HudVisibilityController` | All views | YES | CanvasGroup alpha |

---

## Event → ViewModel Mapping (GameplayHudRuntimeBinder)

| Event | ViewModel Fields Updated |
|---|---|
| `HPChangedEvent(delta, currentHP, maxHP)` | `Hp = currentHP`, `MaxHp = maxHP` |
| `StaminaChangedEvent(currentStamina, maxStamina)` | `Stamina`, `MaxStamina` |
| `ManaChangedEvent(currentMana, maxMana)` | `Mp`, `MaxMp`, `ShowMp = maxMana > 0` |
| `HungerChangedEvent(delta, currentValue, maxValue)` | `HungerCompact = currentValue/maxValue` |
| `InteractionPromptChangedEvent(hasCandidate, prompt)` | `ContextPrompt.IsVisible`, `ContextPrompt.DescriptionKey`, `ContextPrompt.ActionKey = "E"` |
| `QuestAcceptedEvent(questId)` | `QuestPrompt = "Quest ativa: {questId}"` |
| `QuestObjectiveProgressedEvent(questId, objId, curr, req)` | `QuestPrompt = "{questId}: {curr}/{req}"` |
| `QuestReadyToCompleteEvent(questId)` | `QuestPrompt = "Entregar: {questId}"` |
| `QuestCompletedEvent(questId)` | `QuestPrompt = ""` |
| Update() polling | `ActiveSkillSlots` refreshed from SkillTreeManager.State |

---

## Event → Feedback Queue (GameplayFeedbackService)

| Event | Toast Text | Duration | Priority |
|---|---|---|---|
| `PlayerActionFeedbackEvent` | evt.Message | evt.DurationSeconds | Normal |
| `GameSavedEvent(success)` | "Jogo salvo." / "Falha: {msg}" | 3s | Normal |
| `GameLoadedEvent(success)` | "Jogo carregado." / "Falha: {msg}" | 3s | Normal |
| `QuestAcceptedEvent` | "Quest aceita: {questId}" | 4s | Important |
| `QuestCompletedEvent` | "Quest concluída: {questId}" | 4s | Important |
| `QuestRewardClaimedEvent` | "Recompensa: {gold}g" / "Recompensa recebida." | 3s | Normal |
| `EnemyKilledEvent` | "Loot: {item} x{amt}" or "Inimigo derrotado." | 2s | Low |
| `CaveLevelEnteredEvent` | "Caverna — nível {n}" | 3s | Normal |
| `CaveExitedEvent` | "Retornando à superfície..." | 3s | Normal |
| `NotificationToastRequestedEvent` | evt.Message | 3s | Normal |

---

## FinalHudGuardValidator Rules Applied

| Rule | Enforcement |
|---|---|
| Active slots ≤ 4 | Checked in `ActiveSkillSlotsHudView.Refresh()` |
| No debug visible in final HUD | `GameplayHudViewModel.DebugVisible = false` (default) |
| No forbidden field IDs (Breath, debug_coord, etc.) | Checked against hotbar and skill slot IDs |
| Dash/Dodge/Block not in active slots | `IsDashDodgeBlock()` check |

---

*Created: 2026-06-10 (WAVE23)*
