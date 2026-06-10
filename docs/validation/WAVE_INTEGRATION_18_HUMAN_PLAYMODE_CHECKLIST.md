# WAVE_INTEGRATION_18 — Human Play Mode Checklist

**Date:** 2026-06-10
**Status:** PENDING_HUMAN_EXECUTION

---

## Pre-Conditions

1. Unity Editor open, project compiled with no errors
2. dev branch pulled (`git pull origin dev`)
3. `slot_1.json` cleared or moved to start fresh (optional)
4. FarmScene or TownScene accessible

---

## Checklist

| Step | Action | Expected Result | Pass/Fail | Notes |
|------|--------|----------------|-----------|-------|
| 1 | Pull latest dev | Working tree clean | | |
| 2 | Open FarmScene in Unity Editor | No red errors in Console | | |
| 3 | Press Play | Player spawns, no red errors | | |
| 4 | Pick up item or check inventory | Inventory shows some items | | |
| 5 | Press save key (or call SaveManager.SaveGame via debug) | Log: "Game saved to slot_1.json" | | |
| 6 | Reload scene (stop Play, press Play again, call LoadGame) | Player/inventory/gold restored | | |
| 7 | Navigate to Town (if WAVE16 wired) | TownScene loads, NPCs visible | | |
| 8 | Interact with Thalindra NPC | Shop opens or dialogue appears | | |
| 9 | Save in TownScene | Log: "Game saved to slot_1.json" | | |
| 10 | Stop Play, Press Play, Load | NPC position and HasMet state restored | | |
| 11 | Verify save from Town preserved Farm data | Farm data not erased in save file | | |
| 12 | Accept quest from NPC (if wired) | Quest active in log | | |
| 13 | Save with quest active | Save completes | | |
| 14 | Stop Play, Press Play, Load | Quest still active (not reset to fresh) | | |
| 15 | Complete quest objectives | Quest shows ReadyToComplete | | |
| 16 | Save/load ReadyToComplete | Quest still shows ReadyToComplete after load | | |
| 17 | Turn in quest | Reward applied (gold/item), quest Completed | | |
| 18 | Save after completed quest | Quests.QuestStates saved with State=Completed | | |
| 19 | Stop Play, Press Play, Load | Quest is Completed (not Active again) | | |
| 20 | Attempt turn-in again on completed quest | No duplicate reward | | |
| 21 | Verify GrantedRewardIds in save file | GrantedRewardIds array present in slot_1.json > Quests > QuestStates | | |
| 22 | Enter cave (if WAVE16 wired) | CaveScene loads | | |
| 23 | Defeat enemy (if WAVE17 wired) | Enemy deactivates, loot added to inventory | | |
| 24 | Save in cave | Save completes, Farm/NPC data not erased | | |
| 25 | Load in cave | Inventory still has loot (enemy may revive — debt) | | |
| 26 | Exit cave back to FarmScene | Inventory preserved, quest state preserved | | |
| 27 | Copy slot_1.json, corrupt it, rename to slot_1.json | Load attempt fails gracefully, no crash | | |
| 28 | Restore good slot_1.json from backup | Good save loads successfully | | |

---

## Expected slot_1.json Structure After Quest Save

```json
{
  "SchemaVersion": 5,
  "Player": {...},
  "Inventory": {...},
  "Quests": {
    "Version": 1,
    "QuestStates": [
      {
        "QuestId": "quest_...",
        "State": 3,
        "Tracked": true,
        "Discovered": true,
        "GrantedRewardIds": ["reward_gold_100"],
        "GrantedFlagIds": [],
        "ObjectiveStates": [...]
      }
    ],
    "GlobalKnownHints": []
  },
  "Npcs": {...},
  "Cave": {...}
}
```

---

## Blocking Issues

| Issue | Severity | Resolution |
|-------|----------|------------|
| Quest not in save after load | BLOCKING | Verify QuestRuntimeBootstrap initializes before Save call |
| Quest resets to Active after load | BLOCKING | Verify QuestService.RestoreFromSaveData called in ApplySaveData |
| Reward duplicates after load | BLOCKING | Verify GrantedRewardIds captured and restored |
| slot_1.json has no "Quests" key | BLOCKING | Verify GameSaveData.Quests field present and captured |
| Load crashes on null Quests | BLOCKING | Verify ValidateAndNormalizeSave normalizes Quests |
| NPC position not restored | WARNING | Verify NpcManager wired in SaveManager |
| Cave run state lost | WARNING | Verify CaveRunManager wired in SaveManager |

---

## Status on Completion

Update:

```text
docs/validation/WAVE_INTEGRATION_18_SAVE_LOAD_GAP_REPORT.md
docs/project/CURRENT_STATE.md
```

Set status to:
- `BUILD_VALIDATED_SAVE_LOAD_GAP_CLOSED_PENDING_HUMAN_PLAYMODE` → if builds pass, all code wired, Play Mode not yet run
- `ACCEPTED` → if all 28 steps pass with no blocking issues
- `BLOCKED_BY_SCENE_WIRING` → if WAVE16 scene wiring prevents cave test
