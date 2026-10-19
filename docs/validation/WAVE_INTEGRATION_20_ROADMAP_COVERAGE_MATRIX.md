# WAVE_INTEGRATION_20 — Roadmap Coverage Matrix

**Date:** 2026-06-10
**Status:** `BUILD_VALIDATED_ACCEPTANCE_CHECKLIST_READY_PENDING_HUMAN_PLAYMODE`

---

## Cobertura dos 15 Steps do Roadmap

| Roadmap Step | Sistema | Evidência Existente | Status Code | Blocking? | Condição |
|---|---|---|---|---|---|
| 1. Entrar no jogo | boot/scene/FarmScene | FarmScene como cena inicial; GameBootstrap DontDestroyOnLoad; WAVE_INTEGRATION_04/05 | CODE_READY | NÃO | Requer FarmScene configurada em Build Settings |
| 2. Andar na farm | player/movement | PlayerController + WASD/Arrows; WAVE_INTEGRATION_03 | BUILD_VALIDATED | NÃO | Funciona se player spawn está wired |
| 3. Plantar/regar/colher | farm crops | FarmPlot.cs + CropGrowthProcessor + FarmWateringService + HarvestCommand; WAVE05 (~123 EditMode tests) | BUILD_VALIDATED | NÃO | Requer FarmPlot GameObjects wired no FarmScene |
| 4. Coletar recurso | resource interactables | FarmResourceInteractable + TreeResource/RockResource/ForageResource em CreateMvpFarmScene; WAVE_INTEGRATION_06 | CODE_READY | NÃO | Requer scene creator executado ou wiring manual |
| 5. Vender item | shipping/economy | SellPoint wired em Zone_ShippingSellpoint (3.5, 7.5); EconomyTransactionCompletedEvent; WAVE_INTEGRATION_07 | BUILD_VALIDATED | NÃO | Requer SellPoint no FarmScene |
| 6. Abrir inventory | inventory UI | InventoryPanelController (I key, ModalType.Inventory, RuntimeInitializeOnLoad); WAVE_INTEGRATION_09 | BUILD_VALIDATED | NÃO | RuntimeInitializeOnLoad = sem wiring manual |
| 7. Comprar/vender no NPC | shop NPC | NpcShopController + BuyPanel + SellPanel + ShopManager; Thalindra wired via auto-wire; WAVE_INTEGRATION_12C + FIX-001B | BUILD_VALIDATED | SIM (Farm→Town) | Requer SceneTransitionGate wired (WAVE13) para chegar na Town |
| 8. Pegar quest | quest giver | QuestRuntimeBootstrap (RuntimeInitializeOnLoad); QuestGiverInteractable auto-wire; WAVE_INTEGRATION_15 + FIX-001 | BUILD_VALIDATED | SIM (Farm→Town) | Requer Farm→Town transition (WAVE13 wiring) |
| 9. Completar quest | quest objectives/reward | QuestService.TurnIn + QuestRewardApplicator + GrantedRewardIds (idempotente); WAVE_INTEGRATION_15+18 | BUILD_VALIDATED | SIM (Farm→Town) | Depende de item delivery + Farm→Town |
| 10. Comprar/equipar skill | skill tree | SkillTreeGameplayPanelController (U key, 69 nodes, RuntimeInitializeOnLoad); WAVE_INTEGRATION_10 | BUILD_VALIDATED | NÃO | RuntimeInitializeOnLoad = sem wiring manual |
| 11. Usar skill | skill effects | Dash=Space, Dodge=DoubleTap, Block=Shift; PlayerMovementActionRuntimeBootstrap; WAVE_INTEGRATION_11B | BUILD_VALIDATED | NÃO | Runtime auto-attach; sem wiring manual |
| 12. Entrar na cave | cave entrance | CaveEntranceInteractable + CaveRuntimeBridge; WAVE_INTEGRATION_16 | CODE_READY_HUMAN_WIRING | SIM | Requer CaveEntranceInteractable wired em FarmScene + Farm→Cave transition (WAVE13) |
| 13. Derrotar inimigo/coletar loot | cave combat/loot | EnemyHealth + EnemyChaseController + EnemyDropSpawner (item_material_stone); WAVE_INTEGRATION_17 | CODE_READY_HUMAN_WIRING | SIM | Requer CaveSmokeTestSpawnerBridge + EnemyDropSpawner wired em CaveScene |
| 14. Salvar/carregar | save/load | SaveManager + F5/F9; QuestStateSectionSaveData; WAVE_INTEGRATION_18+19 | BUILD_VALIDATED | NÃO | Funciona sem wiring manual |
| 15. Confirmar estado persistido | state validation | QuestService.RestoreFromSaveData + GrantedRewardIds; WAVE_INTEGRATION_18 | BUILD_VALIDATED | NÃO | Verificável após Steps 13-14 |

---

## Sumário por Categoria

| Categoria | Steps | Código | Wiring Human | Status |
|---|---|---|---|---|
| Sem wiring manual | 2,6,10,11,14,15 | BUILD_VALIDATED | NÃO | READY_FOR_PLAYMODE |
| Wiring cena existente já feito | 5 (SellPoint) | BUILD_VALIDATED | Já documentado em WAVE07 | READY_FOR_PLAYMODE |
| Wiring scene creator (um comando) | 1,3,4 | CODE_READY | CreateMvpFarmScene ou manual | GATED_ON_SCENE_CREATOR |
| Requer Farm→Town transition | 7,8,9 | BUILD_VALIDATED | WAVE13 gates + anchors | GATED_ON_WAVE13_WIRING |
| Requer Farm→Cave transition | 12,13 | CODE_READY | WAVE13 + WAVE16 + WAVE17 wiring | GATED_ON_WAVE13+16+17_WIRING |

---

## Sub-Slice Sem Wiring Manual (testável imediatamente)

Steps **2, 6, 10, 11, 14, 15** são testáveis sem nenhuma ação humana no Unity Editor:

```
Andar na farm
Abrir inventory (I)
Skill tree (U)
Usar skill (Dash/Dodge/Block)
Salvar (F5) / Carregar (F9)
Confirmar estado persistido
```

Este sub-slice é suficiente para validar o core loop de player/skills/save.

---

## Bloqueadores Técnicos por Wiring

| Bloqueador | Steps Afetados | Resolução | WAVE |
|---|---|---|---|
| FarmScene Build Settings | 1 | Human: Add FarmScene to Build Settings | WAVE13 |
| FarmScene gates/anchors | 3,4,5 | Human: run CreateMvpFarmScene OR wire manually | WAVE05/07 |
| Farm→Town SceneTransitionGate | 7,8,9 | Human: place gate + anchor in FarmScene + TownScene | WAVE13 |
| CaveEntranceInteractable | 12 | Human: place CaveEntranceInteractable in FarmScene | WAVE16 |
| CaveSmokeTestSpawnerBridge | 13 | Human: place bridge + EnemyDropSpawner in CaveScene | WAVE17 |
