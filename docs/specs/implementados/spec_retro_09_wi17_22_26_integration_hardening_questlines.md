# Retro-Spec 09 — WI-17..22 + WI-26: Hardening de Integração, Aceitação do Slice e Questlines

> **Spec ID:** `spec_retro_09_wi17_22_26_integration_hardening_questlines`
> **Status:** RETRO_DOCUMENTED (código implementado em 2026-06; spec escrita a posteriori para reconstrutibilidade)
> **Tipo:** Retro-spec (consolidada — 7 waves)
> **Domínio:** Integração (combate/loot, save/load, HUD/UX, aceitação, bugfix, débitos, quests)
> **Código que documenta:**
> - `Assets/_Game/Scripts/Editor/Validation/ValidateWave17CaveCombatLootLoop.cs` … `ValidateWave22DebtBacklog.cs`, `ValidateWave26QuestlineObjectiveVariety.cs` (validadores por wave)
> - `Assets/_Game/Scripts/Quests/Runtime/QuestProgressEventBridge.cs` (WAVE15 + WAVE26)
> - `Assets/_Game/Scripts/Quests/Runtime/QuestRegistry.cs` + `QuestRuntimeIds` (catálogo W15/W26)
> - `Assets/_Game/Scripts/Quests/QuestDefinition.cs` (modelo)
> - `Assets/_Game/Scripts/Editor/Thalindra/CreateThalindraQuestDialogueTree.cs`
> - `Assets/_Game/Scripts/Cave/Runtime/CaveSmokeTestSpawnerBridge.cs` (WI-17)
> - `Assets/_Game/Scripts/Save/SaveData.cs` / `SaveManager.cs` / `Quests/Runtime/QuestRuntimeBootstrap.cs` (delta WI-18)
> **Evidência de execução:** `docs/validation/WAVE_INTEGRATION_17_CAVE_COMBAT_LOOT_REPORT.md`, `..._18_SAVE_LOAD_GAP_REPORT.md`, `..._19_HUD_UX_ACCEPTANCE_REPORT.md`, `..._20_PLAYABLE_SLICE_CLOSEOUT_REPORT.md`, `..._21_POST_ACCEPTANCE_BUGFIX_REPORT.md`, `..._22_DEBT_BACKLOG_REPORT.md`, `..._26_QUESTLINE_OBJECTIVE_VARIETY_REPORT.md` (+ matrizes por wave)
> **Supersedida/complementada por:** `fable_13` (fecha DEBT-SAVE-* remanescentes), `fable_14` (HUD visual), `fable_34`/`fable_35`/`fable_36` (infraestrutura de quests por dados + cadeias + atos principais — substituirão o catálogo hard-coded), `fable_10` (Act 1 jogável). WI-23/24/25 têm retro-specs próprias (06/07/08).

---

# /speckit.specify

## Contexto

As waves de integração 17–22 transformaram sistemas isolados num slice jogável auditável: loop de combate+loot na caverna (17), fechamento do gap de save de quests (18), guard de modais + cobertura de feedback (19), gate de aceitação do slice (20), triagem pós-aceitação (21) e consolidação canônica de débitos + roadmap seguinte (22). A WAVE26 expandiu o sistema de quests com uma cadeia de 3 quests e 7 tipos de objetivo ligados a eventos reais. O padrão comum: auditar o existente, implementar apenas o delta, criar um validador editor por wave e nunca editar YAML de cena (débitos de wiring ficam com o humano).

## Comportamento implementado (por wave — regras que valem hoje)

### WAVE17 — Cave First Combat + Loot Extraction Loop

- **Decisão**: `USE_EXISTING` para todo o stack de combate; único código novo runtime foi `CaveSmokeTestSpawnerBridge` (spawna 1 inimigo smoke `enemy_slime_basic`, HP 10, com EnemyHealth/EnemyChaseController/EnemyContactDamage/Knockback/HitFlash existentes).
- Propostas de sistemas paralelos REJEITADAS (DamagePayload, EnemyDefinition, novo EnemyHealth) — `DamageRequest`/`EnemyDataSO`/`Combat.EnemyHealth` já existiam.
- Loot: `EnemyKilledEvent` → `EnemyDropSpawner` → `InventoryManager.AddItem` direto (sem pickup físico); drop idempotente (`Die()` único + SetActive(false)). Item smoke: `item_material_stone`.
- Extração preserva inventário/ouro/quest state entre cenas (managers DontDestroyOnLoad — matriz EXTRACTION_PRESERVATION).
- Regras vigentes: ataque player Q/E com stamina (`PlayerAttackController`, dano desarmado 3); combate final/AI final NÃO foram tocados.

### WAVE18 — Save/Load Gap Closure (quests)

- DTOs novos em `SaveData.cs`: `QuestObjectiveStateSaveData`, `QuestStateSaveData`, `QuestStateSectionSaveData`; campo `GameSaveData.Quests`. Motivo: `QuestStateSection`/`Record` usam properties — JsonUtility não serializa; DTOs com fields públicos são obrigatórios.
- `SaveManager`: `CaptureQuestSaveData`/`RestoreQuestSaveData` + normalização `saveData.Quests ??= new(...)` (save legado sem seção carrega limpo).
- `QuestRuntimeBootstrap`: APIs estáticas `CaptureSaveData()`/`RestoreFromSaveData()`/`SetPendingSaveData()` com pending pre-inicialização (restore antes do QuestService existir é aplicado no `Initialize()`).
- `SchemaVersion = 5` mantido (campo opcional, backward compatible; nenhuma migração nova).
- Idempotência de recompensa pós-load: `GrantedRewardIds` persistidos + guard de TurnIn (já existiam; wave validou e persistiu).
- Débito declarado: HP de inimigo da caverna não salvo (`CAVE_ENEMY_HP_SAVE_DEBT`, estratégia DROP_DIRECT_TO_INVENTORY) → consolidado na W22, fechado por fable_13/44.

### WAVE19 — HUD/UX Polish + Acceptance Gate

- Delta mínimo (quase tudo `ALREADY_IMPLEMENTED`): criado `GameLoadedEvent`; `ModalType.QuestLog` adicionado ao enum com `PushModal`/`TryPopModal` no `QuestLogPanelController`; `DebugHud` ganhou 9 subscriptions de feedback (save/load, 5 de quest, cave enter/exit, enemy killed).
- Regras vigentes: TODA UI modal passa pelo stack do `ModalManager`; dash/dodge/block e double-tap são bloqueados por `HasActiveModal`; todo evento de quest/save/cave/loot tem feedback visível.

### WAVE20 — Playable Slice Acceptance Closeout

- Wave documental: consolidou os status de WAVE13–19, classificou P0=0, P1 código=0, P1 wiring=5 (B001–B005), P2=8, P3=3; produziu checklist humano final de 56 passos, matriz go/no-go, registro de bugs/débitos e template de evidência de aceitação.
- Regra vigente: o slice MVP só vira `ACCEPTED` com evidência humana de Play Mode preenchida no template — nenhuma claim automática.

### WAVE21 — Post-Acceptance Bugfix

- Resultado: `NO_OP_NO_P0_P1_FOUND` — todos os P1 eram HUMAN_WIRING_REQUIRED (política unity-yaml proíbe o agente de editar cenas); B005 reclassificado P2. Nenhum código alterado.
- Regra vigente: bugs de wiring de cena NUNCA são "consertados" via YAML pelo agente; viram instruções de wiring + checklist de reteste.

### WAVE22 — Debt Backlog Canônico + Next Roadmap

- Consolidou débitos de WAVE13–21 em IDs canônicos `DEBT-{ÁREA}-{NNN}` (SCENE/SAVE/UX/CAVE/NPC/FARM/QUEST/TEST/LOOT/COMBAT/AUDIO): 4 P1-wiring, 13 P2, 4 P3; 11 grupos deduplicated; 6 débitos obsoletos removidos; 1 aceito (DEBT-CAVE-002); 1 FIX_NOW_TRIVIAL (DEBT-UX-004, equipment modal guard — corrigido depois).
- Produziu `NEXT_ROADMAP_PROPOSAL` (MVP_PLUS_00..10) e `HANDOFF_FOR_NEXT_EXECUTION` — origem das waves 23–26.
- Regra vigente: `WAVE_INTEGRATION_22_CANONICAL_DEBT_REGISTER.md` é o registro canônico de débitos do slice; débitos novos referenciam esses IDs.

### WAVE26 — Questline Expansion + Objective Variety

- **Cadeia de 3 quests** (catálogo hard-coded em `QuestRegistry.RegisterSmokeTestQuests`, `TEMPORARY_QUEST_SMOKE_TEST` — pipeline por dados é fable_34):

| # | QuestId | Giver | Objetivo | Recompensas | Pré-req |
|---|---|---|---|---|---|
| 1 (W15) | `quest_first_supplies_for_cindar` (Tutorial) | `npc_thalindra` | CollectItem `item_material_wood` ×2 + `item_material_stone` ×2 | Gold 50 + flag `flag_first_town_supplies_delivered` | — |
| 2 (W26) | `quest_tools_for_the_town` (Side) | `npc_pip` | SellItem target `any` ×1 (qualquer venda com GoldDelta>0) | Gold 30 + flag `flag_town_tools_funded` | Q1 |
| 3 (W26) | `quest_echo_from_the_cave` (Side) | `npc_maelor` | ReachCaveDepth `cave_level_1` ×1 (CaveLevel == 1) | Gold 60 + flag `flag_cave_first_explored` | Q2 |

- **Idempotência dupla** em toda recompensa: estado `Completed` bloqueia TurnIn repetido + `GrantedRewardIds` com `IdempotencyPolicy` (`TrackByRewardId` para gold, `TrackByFlagId` para flags), persistidos pela WI-18.
- **7 tipos de objetivo com handler**: CollectItem (`InventoryChangedEvent`, W15), CraftItem (`ItemCraftedEvent`, W15), SellItem (`EconomyTransactionCompletedEvent`), ReachCaveDepth (`CaveLevelEnteredEvent`), HarvestCrop (`CropHarvestedEvent`), TalkToNpc (`NpcInteractionStartedEvent`), DefeatEnemy (`EnemyKilledEvent`) — os 4 últimos com bridge pronto, ainda sem quest na cadeia.
- `QuestProgressEventBridge` (classe pura, sem refs de cena, vivo via `QuestRuntimeBootstrap`): subscribe/unsubscribe simétrico dos 7 eventos; venda só progride se `WasSuccessful`.
- `QuestRuntimeIds`: constantes estáveis de quests/NPCs/targets (sentinela `AnyCropItemTarget = "any"`).
- `QuestDefinition` (modelo): campos SpecKit-ready (Category, SpoilerTier, StepIds, RewardIds, RepeatPolicy, prerequisites/blockers) + campos legacy compat (DisplayName, Objectives, Conditions com `ConditionType` ItemCount/DaysPassed/SeasonReached/LocationVisited/NpcMet/EventTriggered); `CanExpire()` = não-Main com ExpiryRuleIds.
- `CreateThalindraQuestDialogueTree` (menu `CindarsHope/Setup/Create Thalindra Quest DialogueTree`): gera `DialogueTree_Thalindra_QuestOffer.asset` com nó único oferecendo a Q1 via `DialogueActionType.OfferQuest` payload `quest_first_supplies_for_cindar`.
- Débito declarado: Q2/Q3 exigem `QuestGiverInteractable` em npc_pip/npc_maelor na TownScene (SCENE_WIRING_DEBT humano).

### Padrão transversal (todas as waves)

- 1 validador editor por wave (`ValidateWave17...` a `ValidateWave26...`, menus `CindarsHope/Validate ...`) checando tipos, assets, docs e gates da wave anterior — gates encadeados (W26 verificou W20–W25 SATISFIED).
- Status honestos por wave (`CODE_READY_HUMAN_UNITY_ACTION_REQUIRED`, `BUILD_VALIDATED_*_PENDING_HUMAN_PLAYMODE`, `NO_OP_*`) — nunca ACCEPTED sem evidência humana.
- Builds exigidos antes/depois: Assembly-CSharp 0E e Assembly-CSharp-Editor 0E (warnings pré-existentes tolerados e listados).

## Critérios de aceite (verificáveis no código/docs atuais)

1. Os 9 validadores `ValidateWave17..22, 26` (+23/24/25) existem em `Editor/Validation/` e compilam.
2. `GameSaveData.Quests` existe com DTOs field-based; save legado sem a seção normaliza para vazio; `GrantedRewardIds` sobrevivem ao load.
3. A cadeia Q1→Q2→Q3 está registrada no `QuestRegistry` com os givers, objetivos, recompensas e pré-requisitos da tabela.
4. `QuestProgressEventBridge.Subscribe()` registra exatamente os 7 eventos (2 W15 + 5 W26) e ignora vendas com `WasSuccessful == false`.
5. `WAVE_INTEGRATION_22_CANONICAL_DEBT_REGISTER.md` lista os débitos canônicos com IDs `DEBT-*`; nenhum débito novo do slice usa outro formato.
6. Nenhuma das waves editou `.unity/.prefab/.asset` manualmente (scene changes NONE nos reports; única exceção: assets criados por menus AssetDatabase).

---

# /speckit.plan

## Arquitetura real

| Camada | Arquivos | Papel |
|---|---|---|
| Smoke combat (W17) | `Cave/Runtime/CaveSmokeTestSpawnerBridge.cs` | Spawner fino sobre componentes existentes |
| Save de quests (W18) | `Save/SaveData.cs`, `Save/SaveManager.cs`, `Quests/Runtime/QuestRuntimeBootstrap.cs`, `QuestService.cs` | DTOs + capture/restore + pending restore |
| UX guard (W19) | `UI/Modal/ModalManager` (+ ModalType.QuestLog), `QuestLogPanelController`, `DebugHud`, `Core/Events/GameLoadedEvent.cs` | Stack de modal completo + feedback total |
| Aceitação/débitos (W20–22) | docs/validation (matrizes, registers, roadmap) + validadores | Governança do slice |
| Quests (W15/W26) | `Quests/Runtime/{QuestRegistry, QuestProgressEventBridge, QuestRuntimeBootstrap}`, `Quests/QuestDefinition.cs`, `Editor/Thalindra/CreateThalindraQuestDialogueTree.cs` | Catálogo, progressão por eventos, oferta via diálogo |
| Validação | `Editor/Validation/ValidateWave1*–2*.cs` | 1 validador por wave, gates encadeados |

## Contratos

- `QuestRegistry.Register(definition, objectives, rewards, giverId)`; `TryGetQuest/GetAllQuests/GetObjectives/GetRewards/GetGiverId`.
- `QuestRuntimeIds.*` — IDs estáveis (quests, NPCs givers, targets `any`/`cave_level_1`, board `board_first_quest_01`).
- `QuestProgressEventBridge.Subscribe()/Unsubscribe()` — única ponte evento→QuestService; QuestService expõe `OnInventoryChanged/OnItemCrafted/OnCropHarvested/OnItemSold/OnNpcTalkedTo/OnCaveLevelEntered/OnEnemyKilled`.
- `QuestRuntimeBootstrap.CaptureSaveData()/RestoreFromSaveData()/SetPendingSaveData()` (estáticos) ↔ `SaveManager`.
- `RewardIdempotencyPolicy.TrackByRewardId | TrackByFlagId` em toda `QuestRewardDefinition`.
- Gates de wave: report da wave anterior com status aceito é pré-condição verificada pelo validador da seguinte.

## Decisões e invariantes

- **Audit-first / no parallel systems**: toda wave começa com matriz "existia vs criar"; propostas que duplicariam sistemas são rejeitadas e registradas.
- **Save DTOs field-based, simple types only** (JsonUtility) — properties nunca; normalização defensiva para saves legados; schema só sobe com migração.
- **Idempotência de recompensa é dupla e persistida** (estado + GrantedRewardIds).
- **Modal stack universal**: nenhuma UI fora do `ModalManager`; ações de movimento respeitam `HasActiveModal`.
- **Débitos canônicos com ID** (`DEBT-ÁREA-NNN`) e disposições explícitas (NEEDS_HUMAN_DECISION / NEXT_SPEC_REQUIRED / NEXT_ROADMAP_ITEM / ACCEPTED_DEBT / FIX_NOW_TRIVIAL).
- **Wiring de cena é sempre humano** (política unity-yaml) — agente entrega código + instruções + checklist.
- **Quests hard-coded são explicitamente temporárias** (`TEMPORARY_QUEST_SMOKE_TEST`) até fable_34 trazer pipeline por dados.

---

# /speckit.tasks

## Reconstrução (passos para refazer do zero, na ordem das waves)

1. (W17) Criar `CaveSmokeTestSpawnerBridge` reusando EnemyHealth/Chase/ContactDamage/DropSpawner; confirmar loop matar→loot→inventário→extração com managers DontDestroyOnLoad.
2. (W18) Criar os 3 DTOs de quest em `SaveData.cs` + campo `Quests`; implementar capture/restore no `SaveManager` e as APIs estáticas com pending no `QuestRuntimeBootstrap`; manter schema e adicionar normalização.
3. (W19) Garantir `ModalType` para toda UI (incluindo QuestLog) com Push/TryPop; criar `GameLoadedEvent`; subscrever feedbacks de save/load/quest/cave/loot no HUD de debug.
4. (W20–22) Produzir os documentos de aceitação (checklist 56 passos, go/no-go, bug register), triagem pós-aceitação e o registro canônico de débitos + roadmap MVP_PLUS — com validadores editor correspondentes.
5. (W26) Registrar Q2/Q3 no `QuestRegistry` (valores da tabela), adicionar os 5 novos handlers ao `QuestService` e as 5 subscriptions ao `QuestProgressEventBridge`; manter `QuestRuntimeIds` como fonte de IDs; criar o menu da árvore de oferta da Thalindra.
6. Criar 1 validador editor por wave com verificação do gate anterior.

## Débitos conhecidos

- Catálogo de quests hard-coded (fable_34/35/36 substituem por dados + cadeias + atos).
- Q2/Q3 sem `QuestGiverInteractable` em cena (SCENE_WIRING_DEBT humano — npc_pip/npc_maelor).
- HP de inimigo da caverna não persiste (DEBT consolidado na W22; política de save da caverna em fable_13/44).
- Checklists humanos de Play Mode das waves 17–22/26 pendentes de execução consolidada (batch em `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`).
- Sem testes EditMode para QuestRegistry/bridge nessas waves (Testing Quality Gate posterior exige — cobrir condição/trigger/idempotência ao reconstruir).
- 4 tipos de objetivo (HarvestCrop/TalkToNpc/DefeatEnemy/CraftItem) com bridge pronto mas sem quest exercitando-os.
