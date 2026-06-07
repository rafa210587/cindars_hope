# SPEC — Cave Treasure Mining Loot Snapshot Runtime

> **Spec ID:** `06_spec_cave_treasure_mining_loot_snapshot_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 06 — Economy / Loot / Rewards Foundation  
> **Priority:** P0  
> **Type:** Runtime / Cave / Treasure / Mining / Snapshot Loot  
> **Domain:** Cave / Mining Nodes / Treasure Rooms / Snapshot / Depleted State  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_06_LOOT_REWARDS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere CaveProceduralGenerator, CaveRuntimeMaterializer, CaveRunSeed, VisitedLevelSnapshots, DepletedNodeIds, ResourceNodeDataSO, treasure room layout ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Loot/**`, `Assets/_Game/Scripts/Resources/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Economy/**`, `docs/validation/06_spec_cave_treasure_mining_loot_snapshot_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
> **Blocks:**  
  - cave procedural generation hardening;
  - resource node snapshot persistence;
  - treasure room rewards;
  - economy balance by cave depth;
  - crafting material supply.
> **Scope:** definir/endurecer loot de mineração/tesouros da caverna preso a snapshot/run, depletion state e gating por bioma/andar/progresso.  
> **Out of scope:** geração procedural completa, layout de salas, enemy spawns, boss fights, final level 101 content, exact loot table content.

---

# /speckit.specify

## 1. Contexto

Cave Direction exige preservar CaveRunManager, CaveProceduralGenerator, CaveRuntimeMaterializer, snapshots, boss gates, checkpoints, DepletedNodeIds e VisitedLevelSnapshots. Recursos e fishing spots entram no snapshot. Mining nodes, treasures e special rooms são parte da fantasia e economia da caverna.

Esta spec formaliza loot de mineração e tesouro ligado a snapshot, sem refazer a geração procedural.

---

## 2. Problema

Sem contrato de cave loot snapshot:

```text
mining node pode rerollar loot ao revisitar;
treasure chest pode abrir duas vezes após reload;
resource node pode renovar dentro da mesma run sem regra;
rare vein pode aparecer fora da faixa de bioma/andar;
nível 101 pode gerar mineração comum indevidamente;
boss chest pode duplicar first-time reward;
CaveRunSeed pode mudar ao abrir tesouro.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CaveLootSourceDefinition;
CaveMiningNodeLootProfile;
CaveTreasureProfile;
CaveLootSnapshotEntry;
Depleted/Opened state;
Snapshot-seeded rolls;
Biome/floor gates;
Level 100/101 restrictions;
No reroll on revisit;
Tests/validators.
```

---

## 4. Regras de design

```text
A caverna é a economia de risco.
Mineração principal vem da caverna.
Fazenda final pode ter pedreira, mas não substitui toda mineração.
Dentro da mesma CaveRunSeed, revisitar nível usa snapshot, não reroll.
Resources e fishing spots entram no snapshot.
Boss first-time reward é separado de repeat reward.
Nível 101 não é farming loop e não gera mineração comum.
```

---

## 5. User stories / engineering stories

```text
Como cave snapshot, quero preservar nodes, treasures e depletion.
Como jogador, quero que tesouro aberto continue aberto.
Como economy, quero minério raro por profundidade/risco.
Como crafting, quero supply de materiais coerente por bioma.
Como save/load, quero impedir reroll/reloot por reload.
```

---

## 6. Escopo

Inclui:

```text
mining node loot profile;
treasure/chest loot profile;
snapshot entry;
depleted/opened state;
floor/biome/progress gates;
seeded roll;
level 100/101 restrictions;
tests/validators.
```

Não inclui:

```text
procedural layout generation;
scene/prefab placement;
enemy spawn generation;
full concrete loot content;
boss fight rewards beyond restrictions;
UI loot popup.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- CaveRunSeed revisits use snapshot, not reroll.
- Resources and fishing spots enter the snapshot.
- DepletedNodeIds and VisitedLevelSnapshots already exist or are partial and must be preserved.
- Cave elements include mining clusters, rare veins, treasure nodes/chests, lore points, special rooms and fishing spots.
- Mining resources vary by floor range; 101 has no common mining and only lore/endgame rewards.
- Resource nodes do not renew within same run in MVP unless future explicit policy.

### Deferred / future from directions

- Full procedural generation rewrite.
- Final treasure table content.
- Level 101 final reward implementation.
- Secret room mechanics.
- Puzzle rewards.
- Treasure UI/popup.

### Explicitly not redefined here

- CaveRunManager.
- CaveProceduralGenerator.
- CaveRuntimeMaterializer.
- VisitedLevelSnapshots.
- DepletedNodeIds.
- ItemDefinition/Inventory backend.

## 7. Modelo de domínio

### 7.1 CaveLootSourceType

```text
MiningNode
RareVein
TreasureChest
LockedChest
BossGateChest
SecretRoomReward
SpecialRoomReward
FishingSpot
LorePoint
Level100GateReward
Level101LoreReward
```

### 7.2 CaveMiningNodeLootProfile

```text
ProfileId
ResourceNodeId
AllowedFloorRange
AllowedBiomes[]
RequiredCaveProgress[]
BaseLootTableId
RareLootTableId optional
QualityRollProfile
QuantityRollProfile
DepletedStatePolicy
SnapshotSeeded
DebugTags[]
```

### 7.3 CaveTreasureProfile

```text
TreasureProfileId
TreasureType
AllowedFloorRange
AllowedBiomes[]
RequiredKeyOrFlag optional
GuardedByPack optional
LootTableId
FirstTimeOnly
Repeatable
SnapshotSeeded
OpenedStatePolicy
DebugTags[]
```

### 7.4 CaveLootSnapshotEntry

```text
SnapshotId
CaveRunSeed
CaveLevel
SourceInstanceId
SourceType
GridPosition
LootRollSeed
RolledLootPreview optional
IsOpened
IsDepleted
OpenedDay optional
DepletedDay optional
RewardConsumedFlags[]
```

---

## 8. Snapshot rules

```text
Loot roll seed belongs to snapshot/source instance.
Revisiting same level with same CaveRunSeed must not reroll loot.
Opening chest sets IsOpened.
Mining node depletion sets IsDepleted.
Reload after open/depleted must preserve state.
Debug regeneration may reroll only by explicit debug command.
ForwardExit/BackExit do not change CaveRunSeed.
```

---

## 9. Floor/biome gates

```text
1-10: stone, copper, coal, clay, simple gem.
11-25: copper high, early iron, fungal minerals, petrified wood.
26-40: iron, silver, ice crystal, deep salt.
41-55: high iron, sulfur, ember mineral, obsidian.
56-70: high silver, early gold, old parts, broken runes.
71-85: gold, dark crystal, drow cloth, lunar echo.
86-99: mithril/rare metal, unstable blackstone, corrupted essence, stabilized blackstone rare.
100: gate fragments, boss reward, level 101 key.
101: no common mining; Anya/lore fragments only.
```

---

## 10. Treasure rules

```text
Treasure can come from common chests, locked chests, boss gate chests, unique rewards, rare veins, secret rooms, puzzles future and elite drops.
Treasure categories include gold, ore, gems, reagents, rare seeds, stabilized blackstone fragments, construct parts, elemental essences, gear, blueprints, lore items and keys/progression fragments.
Treasure should reward risk and preserve active budget/traps.
```

---

## 11. Level 100/101 restrictions

```text
Level 100:
  - controlled/semi-special;
  - minimal/symbolic mining;
  - few treasures before boss;
  - boss arena and sealed portal to 101;
  - no repeat unique gate reward.

Level 101:
  - not procedural common;
  - no common mining;
  - no common packs;
  - no farming loop;
  - lore/endgame rewards only.
```

---

## 12. Criteria

```text
Mining and treasure loot tied to snapshot.
Revisit/reload does not reroll/open again.
Depleted/opened state persists or STOP if save missing.
Floor/biome gates enforced.
Level 101 common mining blocked.
Boss gate treasure respects first-time/repeat separation.
Tests cover snapshot, depletion, open once, reload, gates and level restrictions.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Cave/Loot/CaveLootSourceType.cs
Assets/_Game/Scripts/Cave/Loot/CaveMiningNodeLootProfile.cs
Assets/_Game/Scripts/Cave/Loot/CaveTreasureProfile.cs
Assets/_Game/Scripts/Cave/Loot/CaveLootSnapshotEntry.cs
Assets/_Game/Scripts/Cave/Loot/CaveSnapshotLootResolver.cs
Assets/_Game/Scripts/Cave/Loot/CaveLootSnapshotValidator.cs
Assets/_Game/Tests/EditMode/Economy/CaveTreasureMiningLootSnapshotTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Resources/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/06_spec_cave_treasure_mining_loot_snapshot_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar cave snapshot/resource/treasure systems.
2. Consolidar cave loot profiles and snapshot entries.
3. Integrar DepletedNodeIds/VisitedLevelSnapshots if existing.
4. Implementar/harden no-reroll and opened/depleted checks.
5. Implementar validators de floor/biome/level 101.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - cave procedural generation;
  - loot table contract;
  - enemy/boss drops;
  - save schema;
  - cave runtime generation/checkpoints.
- Reason: cave loot consumes snapshot and resource contracts.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if DepletedNodeIds/VisitedLevelSnapshots exist; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding CaveLootSnapshotEntry persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CaveTreasureOpenedEvent, CaveNodeDepletedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for chest/mine/revisit/reload flow.
```

---

## 21. Riscos

```text
Risco: reroll on revisit.
Mitigação: snapshot seed tests.

Risco: loot duplication after reload.
Mitigação: opened/depleted tests.

Risco: level 101 becomes farm loop.
Mitigação: validator.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar cave snapshot/resource/treasure systems.
- [ ] T003 — Consolidar cave loot contracts.
- [ ] T004 — Integrar opened/depleted state.
- [ ] T005 — Implementar no-reroll validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a cave treasure/mining loot snapshot foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de cave treasure/mining loot snapshot? | Arquivos alterados e justificativa. | PARTIAL |
| Snapshot/idempotência | Loot/reward não duplica após reload, revisit, boss repeat ou coleta repetida? | Testes ou validators. | PARTIAL |
| Economia segura | A spec impede gold/hour absurdo, rare loot cedo demais e reward farm infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/06_spec_cave_treasure_mining_loot_snapshot_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CaveLoot|MiningNode|TreasureChest|CaveSnapshot|VisitedLevelSnapshot|DepletedNodeIds|ResourceNode|LootRollSeed|CaveRunSeed|Level101" Assets/_Game/Scripts docs/design docs/specs
rg -n "LootTable|RewardTable|DropTable|EnemyDrop|BossReward|Treasure|MiningNode|CaveSnapshot|FirstTimeReward|RepeatReward|BaseValue|AntiArbitrage|GoldHour|Save" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

A execução deve classificar cada achado como:

```text
EXISTING_CANONICAL
  Sistema já existe e deve ser reaproveitado/endurecido.

EXISTING_PARTIAL
  Sistema existe, mas precisa hardening/delta.

MISSING_SAFE_TO_CREATE
  Sistema não existe e criação é pequena, isolada e dentro do escopo.

MISSING_BUT_DEFER
  Sistema não existe, mas criação exigiria outro domínio/spec.

CONFLICT
  Há dois caminhos possíveis ou contrato divergente. Parar e reportar.
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base relacionado a cave treasure/mining loot snapshot existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, snapshot, economia e inventário permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Duplicate prevention

```text
Given loot/reward já foi coletado, boss já foi derrotado, treasure já foi aberto ou node já foi minerado
When o jogador recarrega, revisita ou tenta repetir a ação
Then o sistema não concede a recompensa única novamente
And reward repetível usa tabela própria, menor e controlada quando existir.
```

### Scenario 4 — Protected/lore/endgame resource

```text
Given Fruto Mana, Água Viva, Pedra Negra estabilizada, Unique, KeyItem, QuestItem ou lore reward
When loot/reward/shop/crafting tenta tratar isso como item comum repetível
Then a ação é bloqueada, marcada future ou exigida por gate explícito
And o report registra proteção.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de caverna, loot popup, chest, mining node, boss reward, loja ou relatório
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Mining node rerolla ao revisitar.
- Chest abre duas vezes após reload.
- Resource renova dentro da mesma run sem policy.
- Rare vein aparece fora de faixa/bioma.
- Level 101 gera mineração comum.
- Boss gate chest duplica reward único.
- Debug regeneration afeta save normal.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Cave Treasure Mining Loot Snapshot Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Duplicate prevention:
- Protected/lore/endgame resource:
- Save/load safety:
- Visual/final scenario:

## Validation
- Docs validation:
- C# build:
- Unity compile:
- EditMode tests:
- PlayMode automated:
- Final human scenario:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next specs impacted
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever Inventory/Economy/Cave/Enemy system canônico existente.
5. A implementação quebrar CaveRunSeed, snapshots, BossDefeatStates, DepletedNodeIds ou VisitedLevelSnapshots.
6. A implementação permitir first-time boss reward mais de uma vez.
7. A implementação tratar Fruto Mana, Água Viva, Pedra Negra estabilizada ou lore/key rewards como commodity comum.
8. A implementação tornar progressão principal dependente exclusivamente de drop raro aleatório.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Cave Loot Snapshot Matrix

| Source | Snapshot state | Repeat behavior |
|---|---|---|
| MiningNode | IsDepleted | no renew in same run MVP |
| RareVein | IsDepleted + floor gate | no reroll |
| TreasureChest | IsOpened | no reopen |
| LockedChest | IsOpened + key/flag | no reopen |
| BossGateChest | reward consumed | first-time/repeat split |
| SecretRoomReward | discovered/opened | no duplicate |
| FishingSpot | snapshot entry | policy-specific |
| Level101LoreReward | story state | no farm loop |

## 23H. Level Restriction Validator

```text
If CaveLevel == 101:
  no common MiningNode;
  no common TreasureChest;
  no common EnemyDrop farming;
  only authored lore/endgame rewards.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "LootTable|RewardTable|DropTable|EnemyDrop|BossReward|Treasure|MiningNode|CaveSnapshot|FirstTimeReward|RepeatReward|BaseValue|AntiArbitrage|GoldHour|Save" Assets/_Game/Scripts docs/design docs/specs
```

C# runtime/editor quando houver alteração C#:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário visual/gameplay de loot, treasure, mining, boss reward, enemy drop ou balance report, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, snapshot/open/depleted/no-reroll logic is deterministic.
- Requires EditMode tests: YES for no-reroll/open-once/depleted/reload/floor-gate/level101 tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for mining/chest/revisit visual flow.
- Requires regression test: YES if fixing existing cave loot duplication/reroll bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no reroll on revisit; no level 101 farm loop.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/06_spec_cave_treasure_mining_loot_snapshot_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não persistir Unity references.
Não usar UI como fonte de verdade.
Não gerar reward único duas vezes.
Não criar gold/hour absurdo.
Não transformar loot raro/lore/endgame em commodity comum.
Não tornar progressão principal dependente só de drop raro.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
