# WAVE_INTEGRATION_15 — Reward Idempotency Matrix

Date: 2026-06-10

---

## Reward Types × Idempotency

| Reward Type | Idempotency Rule | Applied? | Evidence |
|-------------|-----------------|---------|----------|
| Gold | TrackByRewardId — GrantedRewardIds guard | YES | QuestService.TurnIn checks AlreadyGrantedRewardIds via QuestRewardApplicator |
| Item | TrackByRewardId — GrantedRewardIds guard | YES | Same guard; item add called only if not already granted |
| QuestFlagGrant | TrackByFlagId — GrantedFlagIds guard | YES | QuestRewardApplicator.ApplyFlagGrant checks AlreadyGrantedFlagIds |
| Reputation | NOT IMPLEMENTED | N/A | Future reward type; deferred |

---

## Transaction Failure Cases

| Scenario | Behavior |
|----------|----------|
| Gold reward succeeds, item reward fails (inventory full) | Gold applied and recorded; item NOT recorded; partial reward state |
| Double turn-in attempt (state=Completed) | QuestService.TurnIn returns AlreadyCompleted(); no reward re-applied |
| Reward already granted (GrantedRewardIds contains rewardId) | QuestRewardApplicator returns AlreadyGranted; reward skipped silently |
| QuestService null at turn-in | QuestOfferPanelController logs warning; no crash |

---

## RewardClaimed Flag Lifecycle

| Stage | State | Record |
|-------|-------|--------|
| Quest accepted | GrantedRewardIds = [] | QuestStateRecord created |
| First turn-in | Rewards applied; GrantedRewardIds += ["reward_supply_quest_gold", "reward_supply_quest_flag"] | State → Completed |
| Second turn-in | QuestService.TurnIn returns AlreadyCompleted() immediately | No rewards re-applied |
| Save/load | QUEST_SAVE_LOAD_IN_MEMORY — state not persisted across sessions | DEBT: SaveManager integration pending |

---

## Save/Load Status

| Item | Status |
|------|--------|
| QuestStateSection in-memory | YES — per session |
| QuestStateRecord format | WAVE09 DTO — no Unity refs (compliant with save-dto-simple-types-only rule) |
| SaveManager integration | NOT IMPLEMENTED — QUEST_SAVE_LOAD_IN_MEMORY debt |
| Save/load test | NOT RUN — no SaveManager integration |
