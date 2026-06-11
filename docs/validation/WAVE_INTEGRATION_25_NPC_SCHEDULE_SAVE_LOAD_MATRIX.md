# WAVE INTEGRATION 25 - NPC Schedule and Save/Load Matrix

Status: SAVE_LOAD_IMPLEMENTED_SCHEDULE_TIME_BLOCK_DEBT

## Save/Load per NPC

All NPCs use the existing NpcManagerSaveData/NpcSaveData system from SaveManager.
Fields persisted: NpcId, SceneId, Vector2 Position, bool HasMet.

| NpcId | PositionSaved | HasMetSaved | ScheduleIdSaved | ScheduleStateSaved | RestoreResult |
|---|---|---|---|---|---|
| npc_pip | YES | YES | NO (transient) | NO (transient) | Position+HasMet restored |
| npc_sylveth | YES | YES | NO | NO | Position+HasMet restored |
| npc_brumdar | YES | YES | NO | NO | Position+HasMet restored |
| npc_renko | YES | YES | NO | NO | Position+HasMet restored |
| npc_thalindra | YES | YES | NO | NO | Position+HasMet restored |
| npc_zrix | YES | YES | NO | NO | Position+HasMet restored |
| npc_nimble | YES | YES | NO | NO | Position+HasMet restored |
| npc_corvus | YES | YES | NO | NO | Position+HasMet restored |
| npc_mara | YES | YES | NO | NO | Position+HasMet restored |
| npc_gurd | YES | YES | NO | NO | Position+HasMet restored |
| npc_hund | YES | YES | NO | NO | Position+HasMet restored |
| npc_ozzra | YES | YES | NO | NO | Position+HasMet restored |
| npc_gruta | YES | YES | NO | NO | Position+HasMet restored |
| npc_yael | YES | YES | NO | NO | Position+HasMet restored |
| npc_dagna | YES | YES | NO | NO | Position+HasMet restored |
| npc_alaric | YES | YES | NO | NO | Position+HasMet restored |
| npc_mirela | YES | YES | NO | NO | Position+HasMet restored |
| npc_eiran | YES | YES | NO | NO | Position+HasMet restored |
| npc_liora | YES | YES | NO | NO | Position+HasMet restored |
| npc_orlan | YES | YES | NO | NO | Position+HasMet restored |
| npc_savra | YES | YES | NO | NO | Position+HasMet restored |
| npc_tovin | YES | YES | NO | NO | Position+HasMet restored |
| npc_maelor | YES | YES | NO | NO | Position+HasMet restored |

## Schedule System

| NpcId | DefaultScheduleId | TimeBlock | HomeAnchor | CanInteract |
|---|---|---|---|---|
| npc_pip | schedule_pip_default | Default (day boundary only) | DefaultPosition | YES |
| npc_sylveth | schedule_sylveth_default | Default | DefaultPosition | YES |
| npc_brumdar | schedule_brumdar_default | Default | DefaultPosition | YES |
| npc_renko | schedule_renko_default | Default | DefaultPosition | YES |
| npc_thalindra | schedule_thalindra_default | Default | DefaultPosition | YES |
| npc_zrix | schedule_zrix_default | Default | DefaultPosition | YES |
| npc_nimble | schedule_nimble_default | Default | DefaultPosition | YES |
| (others) | schedule_*_default | Default | DefaultPosition | YES |

## TIME_BLOCK_DEBT

GameTimeManager does not publish TimeBlockChangedEvent.
NpcScheduleService resolves positions only on DayStartedEvent (once per day).
Intra-day period transitions (Morning/Midday/Evening/Night) are TIME_BLOCK_DEBT.
Schedule state (currentAnchorId/currentScheduleBlock) is transient — not saved/loaded.
On save/load: NPCs restore to their saved position (not schedule-resolved position).
After next DayStartedEvent: schedule service will set them to their DefaultPosition home anchor.

## Implementation

NpcScheduleService (WAVE25 new code):
- Subscribe DayStartedEvent in OnEnable/OnDisable
- On event: resolve DefaultPosition from NpcDataSO for each NpcController/NpcShopController
- Set transform.position = defaultPosition if NPC not currently interacting
- Maintains NpcScheduleRuntimeState (transient in-memory)

NpcManager.CaptureSaveData: captures current transform.position (post-schedule teleport)
NpcManager.RestoreFromSaveData: restores to saved position (pre-schedule state)
