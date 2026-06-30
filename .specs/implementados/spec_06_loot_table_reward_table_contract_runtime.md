# SPEC — Loot Table Reward Table Contract Runtime

> **Spec ID:** `06_spec_loot_table_reward_table_contract_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 06 — Economy / Loot / Rewards Foundation  
> **Priority:** P0  
> **Type:** Runtime / Data Contract / Loot Tables / Reward Tables  
> **Domain:** Economy / LootTable / RewardTable / QualityRoll / RarityRoll / Pity  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_06_LOOT_REWARDS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere enemy drop tables, cave treasure/mining tables, boss rewards, quest/order rewards, inventory schema, item definitions ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Loot/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Economy/**`, `docs/validation/06_spec_loot_table_reward_table_contract_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
> **Blocks:**  
  - enemy/common/elite/boss drop tables;
  - cave treasure/mining loot;
  - quest/order/festival reward tables;
  - economy balance validation;
  - inventory item grant services.
> **Scope:** definir/endurecer contrato comum de LootTableSO/RewardTableSO, roll profiles, guarantees, rarity/quality, unique/first-time/repeat policies e safety validators.  
> **Out of scope:** conteúdo final de drops, boss rewards concretos, cave generation changes, enemy stats, UI loot popup, inventory schema migration.

---

# /speckit.specify

## 1. Contexto

Loot/crafting/economia define tipos de loot table: `LootTableSO`, `EnemyDropTableSO`, `BossDropTableSO`, `TreasureTableSO`, `MiningNodeTableSO`, `ForageTableSO`, `FishingTableSO`, `ShopInventoryTableSO`, `OrderRewardTableSO`, `QuestRewardTableSO` e `FestivalRewardTableSO`.

Esta spec cria o contrato comum. Ela não cria o conteúdo final de cada tabela.

---

## 2. Problema

Sem contrato comum:

```text
cada drop table pode representar rarity/quality de forma diferente;
reward único pode ser entregue duas vezes;
drop raro pode bloquear progressão sem pity/alternativa;
gold range pode quebrar economia;
Unique/Lore/KeyItem pode aparecer como drop repetível;
quality roll pode não ser seeded;
table pode ignorar SourceType, bioma, andar, progress flag e repeat rules.
```

---

## 3. Objetivo

Criar/endurecer:

```text
LootTableDefinition;
RewardTableDefinition;
LootSourceType;
LootEntry;
GuaranteedDrops;
WeightedDrops;
RareDrops;
UniqueDrops;
GoldRange;
QualityRollProfile;
RarityRollProfile;
QuantityRollProfile;
FirstTimeBonus;
RepeatFarmRules;
PityRules;
validators.
```

---

## 4. Regras de design

```text
Progressão principal não deve depender exclusivamente de drop raro aleatório.
Se item raro for necessário, deve haver pity, receita alternativa, compra tardia, quest ou boss guaranteed drop.
Unique normalmente é first-time, boss, quest ou lore.
Boss drop deve ser direcional e memorável.
Loot raro acelera progressão ou abre build; não deve bloquear caminho principal se chance baixa.
```

---

## 5. User stories / engineering stories

```text
Como cave/mining/enemy reward, quero usar o mesmo contrato de loot.
Como economy, quero validar gold/rarity/quality sem conhecer cada sistema.
Como save/load, quero impedir reward único duplicado.
Como designer, quero declarar guaranteed, weighted, rare e unique drops com gates.
Como executor, quero validators antes de conteúdo final.
```

---

## 6. Escopo

Inclui:

```text
loot/reward table contracts;
entry types;
roll profiles;
first-time/repeat policies;
pity policy;
source/gate fields;
protected item validation;
tests/validators.
```

Não inclui:

```text
tabelas finais por inimigo;
tabelas finais por bioma;
boss reward content final;
quest/order reward runtime;
UI loot popup;
save schema migration.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
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

- Loot table types include enemy, boss, treasure, mining, forage, fishing, shop, order, quest and festival reward tables.
- LootTableSO minimum fields include SourceType, biome/floor/progress gates, guaranteed/weighted/rare/unique drops, gold range, quality/rarity/quantity profiles, FirstTimeBonus, RepeatFarmRules and optional PityRules.
- Progression must not depend exclusively on rare random drop.
- Bosses should drop first-time reward, unique/rare component, blueprint/recipe when appropriate and lore resource if applicable.
- Boss must not become infinite gold farm.
- Unique should have protection against accidental loss.

### Deferred / future from directions

- Enemy family concrete tables.
- Boss concrete reward tables.
- Treasure room contents.
- Quest/order reward runtime.
- Festival rewards.
- Loot popup UI.
- Balance final.

### Explicitly not redefined here

- ItemDefinition contract.
- Inventory grant service.
- Cave snapshot runtime.
- Enemy roster/stats.
- Boss gate runtime.
- Quest state.

## 7. Modelo de domínio

### 7.1 LootSourceType

```text
FarmCrops
FarmAnimals
Fishing
Foraging
Trees
Rocks
FarmQuarryLate
CaveMining
CaveEnemies
CaveElites
CaveBosses
CaveTreasure
SpecialRooms
TrapsTreasureTrap
CityShops
NpcOrders
NpcGifts
Festivals
QuestRewards
LoreEvents
FonteDeAnya
BromecianRuins
```

### 7.2 LootTableDefinition

```text
LootTableId
SourceType
AllowedBiomes[]
AllowedFloorRanges[]
RequiredProgressFlags[]
RequiredReputationFlags[]
RequiredStoryFlags[]
GuaranteedDrops[]
WeightedDrops[]
RareDrops[]
UniqueDrops[]
GoldRange
QualityRollProfile
RarityRollProfile
QuantityRollProfile
FirstTimeBonus
RepeatFarmRules
PityRules optional
DebugTags[]
```

### 7.3 LootEntry

```text
EntryId
ItemId
QuantityMin
QuantityMax
Weight
DropChance
QualityPolicy
RarityPolicy
RequiredFlag optional
ForbiddenFlag optional
FirstTimeOnly
Repeatable
IsUniqueReward
IsLoreReward
IsProgressionCritical
```

### 7.4 RewardGrantResult

```text
Success
FailureReason
GrantedItems[]
GrantedGold
GrantedFlags[]
GrantedRecipes[]
GrantedReputation optional
FirstTimeRewardConsumed
RepeatRewardUsed
DebugRolls[]
```

---

## 8. Roll rules

```text
GuaranteedDrops always grant if table is eligible.
WeightedDrops use weights and can be seeded.
RareDrops must have chance/cap and should not block main path without alternative.
UniqueDrops are first-time or explicitly authored.
GoldRange must respect source risk and depth/tier.
QualityRollProfile must be deterministic/seeded when tied to snapshot.
RarityRollProfile must not be confused with quality.
```

---

## 9. Pity and progression rules

```text
If drop is progression-critical and chance-based:
  - add pity;
  - or make first-time boss guaranteed;
  - or add recipe alternative;
  - or add late shop/quest fallback.

No main quest blocker may be pure low chance random.
```

---

## 10. Protected item rules

```text
QuestItem/KeyItem: normally guaranteed/story, not weighted repeat loot.
Unique: first-time or protected reward.
Fruto Mana: not common table loot.
Água Viva: Fonte/lore, not common loot.
Pedra Negra estabilizada: deep gated/endgame, never common early loot.
Blackstone corrupted shard: dangerous/lore gated, not generic sellable material by default.
```

---

## 11. Criteria

```text
Loot table contract exists or is hardened.
Reward grant result is explicit.
Unique/first-time/repeat policies exist.
Pity/alternative is required for progression-critical random drops.
Protected items cannot appear as common repeat loot.
Quality and rarity roll profiles are distinct.
Tests cover guaranteed, weighted, rare, unique, first-time, repeat and protected entries.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Loot/LootSourceType.cs
Assets/_Game/Scripts/Loot/LootTableDefinition.cs
Assets/_Game/Scripts/Loot/LootEntry.cs
Assets/_Game/Scripts/Loot/RewardTableDefinition.cs
Assets/_Game/Scripts/Loot/RewardGrantResult.cs
Assets/_Game/Scripts/Loot/LootTableResolver.cs
Assets/_Game/Scripts/Loot/LootTableValidator.cs
Assets/_Game/Tests/EditMode/Economy/LootTableContractTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/06_spec_loot_table_reward_table_contract_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar loot/reward/drop table systems.
2. Consolidar LootTable/RewardTable contracts.
3. Implementar validators de source/gates/protection.
4. Implementar resolver mínimo sem conteúdo final.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - enemy drop tables;
  - cave treasure/mining loot;
  - boss rewards;
  - quest/order rewards;
  - item definition schema.
- Reason: this is the shared base contract.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless reward grant state is added; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. LootGrantedEvent if event contract allows.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED only if loot popup is wired.
```

---

## 20. Riscos

```text
Risco: reward unique duplicado.
Mitigação: first-time/repeat policy.

Risco: progressão bloqueada por drop raro.
Mitigação: pity/alternative validator.

Risco: protected resource becomes common loot.
Mitigação: protected item validator.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar loot/reward systems.
- [ ] T003 — Consolidar contracts.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar resolver mínimo se seguro.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a loot table/reward table contract foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de loot table/reward table contract? | Arquivos alterados e justificativa. | PARTIAL |
| Snapshot/idempotência | Loot/reward não duplica após reload, revisit, boss repeat ou coleta repetida? | Testes ou validators. | PARTIAL |
| Economia segura | A spec impede gold/hour absurdo, rare loot cedo demais e reward farm infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/06_spec_loot_table_reward_table_contract_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "LootTable|RewardTable|LootEntry|GuaranteedDrops|WeightedDrops|RareDrops|UniqueDrops|FirstTimeBonus|RepeatFarmRules|PityRules" Assets/_Game/Scripts docs/design .specs
rg -n "LootTable|RewardTable|DropTable|EnemyDrop|BossReward|Treasure|MiningNode|CaveSnapshot|FirstTimeReward|RepeatReward|BaseValue|AntiArbitrage|GoldHour|Save" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given o sistema base relacionado a loot table/reward table contract existe ou foi criado de forma mínima
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

- Unique drop em WeightedDrops repetível.
- ProgressionCritical drop sem pity/alternativa.
- QualityRoll e RarityRoll confundidos.
- GoldRange sem source/tier.
- Protected item em common repeat loot.
- FirstTimeBonus concedido duas vezes.
- Loot table depende de prefab/Unity reference.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Loot Table Reward Table Contract Runtime

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


## 23G. Loot Entry Safety Matrix

| Entry type | Repeatable? | Use case |
|---|---:|---|
| GuaranteedDrop | conditional | progression/basic reward |
| WeightedDrop | yes | common variety |
| RareDrop | yes, capped | optional acceleration/build |
| UniqueDrop | no by default | boss/quest/lore |
| FirstTimeBonus | no | first completion |
| RepeatReward | yes, smaller | controlled farming |
| PityReward | until granted | anti-bad-luck |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "LootTable|RewardTable|DropTable|EnemyDrop|BossReward|Treasure|MiningNode|CaveSnapshot|FirstTimeReward|RepeatReward|BaseValue|AntiArbitrage|GoldHour|Save" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, loot/reward selection and validation logic is deterministic.
- Requires EditMode tests: YES for guaranteed/weighted/rare/unique/pity/protected item tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if loot popup/inventory grant flow is wired.
- Requires regression test: YES if fixing existing loot duplication/protection bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no unique reward duplication; no protected commodity leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/06_spec_loot_table_reward_table_contract_runtime_execution_report.md.
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
