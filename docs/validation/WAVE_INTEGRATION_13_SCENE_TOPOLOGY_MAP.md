# WAVE_INTEGRATION_13 — Scene Topology Map

## Scene Inventory (Game Scenes Only)

| SceneId Constant | Scene Name | Path | Type | Placeholder? | MVP? |
|-----------------|-----------|------|------|-------------|------|
| SceneId.Farm | FarmScene | Assets/_Game/Scenes/FarmScene.unity | ACTIVE_ENTRYPOINT | NO | YES |
| SceneId.Town | TownScene | Assets/_Game/Scenes/TownScene.unity | ACTIVE_TARGET | TOWNSCENE_PLACEHOLDER_MINIMAL | YES |
| SceneId.Cave | CaveScene | Assets/_Game/Scenes/CaveScene.unity | ACTIVE_TARGET | NO (has procedural cave runtime) | YES |

Note: BootScene, PersistentManagers, CaveEntranceScene, HomeInteriorScene are MISSING.
Do NOT create them via YAML. Defer to human Unity Editor action.

## Entry/Exit Points Per Scene

### FarmScene

| Point Type | ID | Direction | Target |
|-----------|-----|-----------|--------|
| Gate | gate_farm_town_exit | EXIT | scene_town |
| Gate | gate_farm_cave_entrance | EXIT | scene_cave |
| Spawn Anchor | spawn_farm_from_town | ENTRY | (arriving from Town) |
| Spawn Anchor | spawn_farm_from_cave | ENTRY | (arriving from Cave) |
| Spawn Anchor | spawn_farm_default | ENTRY | (default / new game) |

### TownScene

| Point Type | ID | Direction | Target |
|-----------|-----|-----------|--------|
| Gate | gate_town_farm_exit | EXIT | scene_farm |
| Spawn Anchor | spawn_town_from_farm | ENTRY | (arriving from Farm) |

### CaveScene

| Point Type | ID | Direction | Target |
|-----------|-----|-----------|--------|
| Gate | gate_cave_farm_exit | EXIT | scene_farm |
| Spawn Anchor | spawn_cave_from_farm | ENTRY | (arriving from Farm) |

Note: CaveScene already has CaveExitPortal which handles the Cave→Farm transition
via SceneTransitionState. SceneTransitionGate is an additional/replacement component
for the same function with stable ID contracts.

## Managers Per Scene

### FarmScene (current wiring)

| Manager | Status |
|---------|--------|
| GameBootstrap | WIRED (scene root) |
| PlayerManager | WIRED |
| InventoryManager | WIRED |
| TimeManager | WIRED |
| EconomyManager | WIRED |
| ShopManager | WIRED |
| EquipmentManager | WIRED |
| SaveManager | WIRED |
| ModalManager | WIRED |
| HungerManager | WIRED |
| StaminaManager | WIRED |
| ManaManager | WIRED |
| SkillTreeManager | WIRED |

### TownScene (current wiring)

| Manager | Status |
|---------|--------|
| GameBootstrap | UNKNOWN — TownScene wiring not audited in detail; human must verify |
| NpcController | WIRED (7+ NPCs from WAVE_INTEGRATION_12) |
| NpcShopController | WIRED |
| PlayerManager | UNKNOWN |

Debt: TownScene may not have all managers wired for full gameplay after Farm→Town transition.

### CaveScene (current wiring)

| Manager | Status |
|---------|--------|
| CaveLevelRuntimeController | WIRED |
| CaveRunManager | WIRED |
| CaveRuntimeMaterializer | WIRED |
| CaveExitPortal | WIRED |
| CaveEnemySpawner | WIRED |

## Build Settings Status

EMPTY — no scenes in Build Settings per EditorBuildSettings.asset audit (m_Scenes: []).

Human must add FarmScene, TownScene, CaveScene to Build Settings via:
File > Build Settings > Add Open Scenes

Without Build Settings entries, SceneManager.LoadScene will fail in standalone builds.
In Editor Play Mode, SceneTransitionRouter falls back to EditorSceneManager.LoadSceneInPlayMode
using the scene file path, so transitions work in-editor without Build Settings.
