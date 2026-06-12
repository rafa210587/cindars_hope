# WAVE_INTEGRATION_26 — Reward Idempotency Matrix

Date: 2026-06-11

---

## Idempotency Mechanism

Rewards are tracked via `QuestStateRecord.GrantedRewardIds` (list of strings).
`QuestRewardApplicator.Apply()` checks `ctx.AlreadyGrantedRewardIds` before applying any reward.
If reward ID already in set: `SkippedAlreadyGranted = true`, reward NOT applied again.
`GrantedRewardIds` is persisted in save data and restored after load.

---

## Idempotency Scenarios

| Scenario | Expected Result | Mechanism | Status |
|----------|----------------|-----------|--------|
| First TurnIn — Q1 | Gold 50 given once; flag granted once | GrantedRewardIds empty → apply | PASS |
| Second TurnIn — Q1 (already completed) | `QuestTurnInResult.AlreadyCompleted()` returned; no reward | State == Completed → early exit | PASS |
| First TurnIn — Q2 | Gold 30 given once; flag granted once | GrantedRewardIds empty → apply | PASS |
| Second TurnIn — Q2 (already completed) | No reward; AlreadyCompleted | State == Completed → early exit | PASS |
| First TurnIn — Q3 | Gold 60 given once; flag granted once | GrantedRewardIds empty → apply | PASS |
| Second TurnIn — Q3 (already completed) | No reward; AlreadyCompleted | State == Completed → early exit | PASS |
| Save during active quest, Load, TurnIn | Reward given once | GrantedRewardIds persisted in save | PASS |
| Load after completion, attempt TurnIn again | No reward; AlreadyCompleted | State == Completed persists | PASS |

---

## Reward ID Registry

| RewardId | QuestId | RewardType | Amount | IdempotencyPolicy |
|----------|---------|------------|--------|-------------------|
| reward_supply_quest_gold | quest_first_supplies_for_cindar | Gold | 50 | TrackByRewardId |
| reward_supply_quest_flag | quest_first_supplies_for_cindar | QuestFlagGrant | — | TrackByFlagId |
| reward_tools_quest_gold | quest_tools_for_the_town | Gold | 30 | TrackByRewardId |
| reward_tools_quest_flag | quest_tools_for_the_town | QuestFlagGrant | — | TrackByFlagId |
| reward_cave_quest_gold | quest_echo_from_the_cave | Gold | 60 | TrackByRewardId |
| reward_cave_quest_flag | quest_echo_from_the_cave | QuestFlagGrant | — | TrackByFlagId |

---

## Code Path

```csharp
// QuestService.TurnIn():
var alreadyGranted = new HashSet<string>(record.GrantedRewardIds);
foreach (var reward in rewards)
{
    var result = _rewardApplicator.Apply(reward, ctx);
    if (result.Success && !result.SkippedAlreadyGranted)
    {
        // Apply gold/items
        if (!record.GrantedRewardIds.Contains(result.GrantedRewardId))
            record.GrantedRewardIds.Add(result.GrantedRewardId);
    }
}
record.State = (int)QuestStateStatus.Completed; // prevents future TurnIn
```

Dual protection:
1. `GrantedRewardIds` prevents double-apply of same reward in same session or after load
2. `State == Completed` prevents TurnIn from executing at all for already-completed quests
