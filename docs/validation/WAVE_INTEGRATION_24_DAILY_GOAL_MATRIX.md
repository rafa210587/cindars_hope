# WAVE_INTEGRATION_24 — Daily Goal Matrix

**Date:** 2026-06-11

## Goals Implemented

| Goal ID | Display Name | Required Progress | Trigger Event | Completion |
|---------|-------------|------------------|---------------|-----------|
| daily_goal_first_harvest | Primeira colheita do dia | 1 | CropHarvestedEvent | DailyGoalCompletedEvent published |
| daily_goal_sell_first_crop | Vender primeiro item colhido | 1 | EconomyTransactionCompletedEvent (WasSuccessful=true, GoldDelta > 0) | DailyGoalCompletedEvent published |

## Goal Lifecycle

| Phase | Behavior |
|-------|----------|
| Day start | Goals reset via DayStartedEvent.OnDayStarted → ResetDailyGoals() |
| Progress | AddProgress(goalId, 1) via relevant event |
| Completion | Completed=true, DailyGoalCompletedEvent published |
| Idempotency | After Completed=true, AddProgress is a no-op |
| After load | RestoreFromSaveData restores exact progress/completed/day |

## Events Produced

| Event | When |
|-------|------|
| DailyGoalProgressedEvent(goalId, current, required) | Each time progress advances (current < required) |
| DailyGoalCompletedEvent(goalId) | When current >= required |

## Feedback Chain

```
CropHarvestedEvent
  → FarmDailyGoalService.OnCropHarvested → AddProgress("daily_goal_first_harvest")
  → DailyGoalProgressedEvent or DailyGoalCompletedEvent
  → FarmLoopFeedbackBridge → PlayerActionFeedbackEvent("Meta diária concluída!")
  → GameplayFeedbackService → HudFeedbackUpdatedEvent → FeedbackToastHudView
```

## Save/Load Goal State

| Case | Expected | Actual |
|------|----------|--------|
| CaptureSaveData | All goal states serialized | FarmDailyGoalService.CaptureSaveData() |
| RestoreFromSaveData | Progress/completed/claimed restored | RestoreFromSaveData() — idempotent |
| New goal (not in save) | Default state (0 progress) | InitializeGoalStates covers all definitions |
| Unknown goalId in save | Skipped (not in _states) | _states.TryGetValue guard |
| Completed goal after load | Stays completed, AddProgress no-op | Completed=true guard |

## Known Debt

| Debt | Priority | Notes |
|------|----------|-------|
| SAVE_LOAD_DAILY_GOAL_DEBT | P2 | CLOSED (verified fable_65 Fase 0). SaveManager.cs JÁ chama CaptureFarmDailyGoalsSaveData() (linha ~200/571) e RestoreFromSaveData() (linha ~1151) — DTO `GameSaveData.DailyGoals` round-trip com Claimed. Fechado por F13 (save debt closure). |
| DAILY_GOAL_REWARD_DEFERRED | P3 | CLOSED (fable_65). Meta concluída paga ouro+XP idempotente (Claimed) via FarmDailyGoalRewardTable + FarmDailyGoalService.TryClaimReward + GameBootstrapDailyGoalRewardSink (PlayerManager.AddGold / PlayerProgressionManager.AddXp). |
| DAILY_GOAL_HUD_DISPLAY_DEFERRED | P3 | CLOSED (fable_65). DailyGoalsHudView + DailyGoalsHudProjection (read-only, padrão WI-23) registrado no GameplayHudCanvasController; respeita HudVisibilityController. Labels visuais Canvas ligados por humano (headless, igual às outras views WI-23). |
| DAILY_GOAL_EDITMODE_TESTS | P2 | CLOSED (fable_65). FarmDailyGoalServiceTests.cs (18 testes EditMode): idempotência, integridade catálogo/tabela, reset diário, n/m, round-trip de save, projeção. |

## fable_65 — Catálogo Expandido (4-6 metas/dia)

| Goal ID | Display Name | Required | Trigger Event (REAL) | Reward (ouro / XP) |
|---------|-------------|----------|----------------------|--------------------|
| daily_goal_first_harvest | Primeira colheita do dia | 1 | CropHarvestedEvent | 15 / 10 |
| daily_goal_sell_first_crop | Vender primeiro item colhido | 1 | EconomyTransactionCompletedEvent | 20 / 10 |
| daily_goal_harvest_three | Colher 3 cultivos | 3 | CropHarvestedEvent | 20 / 12 |
| daily_goal_plant_three | Plantar 3 sementes | 3 | SeedPlantedEvent | 10 / 8 |
| daily_goal_gather_resource | Coletar 1 recurso da fazenda | 1 | TreeChoppedEvent | 15 / 8 |
| daily_goal_talk_to_npc | Falar com 1 morador | 1 | NpcInteractionStartedEvent | 10 / 5 |

**Total diário máximo:** 90 ouro + 53 XP (TUNING HUMANO PENDENTE — valores v1 conservadores; dentro de farm_rules "50-100 ouro/dia"). Pagamento só com `!Claimed` → anti-exploit.

**Metas candidatas FORA da v1 (evento ainda inexistente):** regar N plots — `CropWateredEvent` NÃO existe como tipo runtime (só referência em validator); fica como meta futura.

**Toast:** o toast de conclusão deixou de ser emitido por `FarmLoopFeedbackBridge` (genérico "Meta diária concluída!") e passou a ser o toast rico de recompensa emitido por `FarmDailyGoalService` ("Meta concluída: +X ouro, +Y XP") — evita duplicação no mesmo evento.
