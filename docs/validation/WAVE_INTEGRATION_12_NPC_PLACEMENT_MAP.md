# WAVE INTEGRATION 12 - NPC Placement Map

Status: TOWNSCENE_MVP_NPCS_PLACED_STATIC_VALIDATED_PENDING_HUMAN_PLAYMODE

Target scene: `Assets/_Game/Scenes/TownScene.unity`

| NpcId | Scene | Position | Facing | Reachable? | Interactable? | Dialogue? | Shop? | Movement? | Notes |
|---|---|---|---|---:|---:|---:|---:|---:|---|
| npc_pip_miudinho | TownScene | (-5, 1) | Down | 1 | 1 | 1 | 0 | 0 | Existing tutorial NPC, now has placement marker |
| npc_sylveth | TownScene | (4.5, 2) | Down | 1 | 1 | 1 | 1 | 0 | Seed/farm merchant using `shop_seeds_tools` |
| npc_brumdar | TownScene | (-4.75, 2.2) | Down | 1 | 1 | 1 | 1 | 0 | Blacksmith using `shop_blacksmith` |
| npc_renko | TownScene | (0, 3.25) | Down | 1 | 1 | 1 | 1 | 0 | General merchant using `shop_general_store` |
| npc_thalindra | TownScene | (-6.2, -0.5) | Down | 1 | 1 | 1 | 0 | 0 | Library/quest hook NPC |
| npc_zrix | TownScene | (5.75, -1.75) | Down | 1 | 1 | 1 | 1 | 0 | Cave rumor/supplies NPC using `shop_cave_supplies` |
| npc_nimble | TownScene | (2.75, -3.2) | Down | 1 | 1 | 1 | 0 | 0 | Crafting/workshop NPC |
| npc_vaalara_wanderer_01 | TownScene | (0, -1.5) | Dynamic | 1 | 1 | 1 | 0 | 1 | Existing temporary extra retained; not counted in MVP completeness |

Static evidence:
- Each MVP object exists by name in `TownScene.unity`.
- Each MVP object has one `NpcScenePlacementMarker`.
- Each MVP marker records `NpcId`, `TownScene`, stable placement id and movement profile.

Unity validation:
Unity validation: NOT RUN
Reason: Unity batchmode aborted because another Unity instance has this project open.
Command attempted: `Unity.exe -batchmode -quit -projectPath ... -executeMethod CindarsHope.Editor.SceneCreation.CreateMvpTownScene.CreateScene`
Residual risk: Unity Play Mode interaction and visual reachability are not validated locally.
