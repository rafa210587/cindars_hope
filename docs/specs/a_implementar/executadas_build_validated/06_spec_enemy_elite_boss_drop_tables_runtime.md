# SPEC — Enemy Elite Boss Drop Tables Runtime

> **Spec ID:** `06_spec_enemy_elite_boss_drop_tables_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 06 — Economy / Loot / Rewards Foundation  
> **Priority:** P0  
> **Type:** Runtime / Economy / Cave / Enemy Drops / Elite Drops / Boss Rewards  
> **Domain:** Cave / Enemy Drops / Elite Drops / Boss First-Time and Repeat Rewards  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_06_LOOT_REWARDS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere enemy stats/actions/spawn resolver, boss gate runtime, cave snapshot, item definitions, loot table contract, economy pricing, XP balance ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Enemies/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Loot/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Economy/**`, `docs/validation/06_spec_enemy_elite_boss_drop_tables_runtime_execution_report.md`  
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
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
> **Blocks:**  
  - enemy defeat reward pipeline;
  - boss gate rewards;
  - cave crafting materials;
  - bestiary/knowledge rewards;
  - economy balance validation.
> **Scope:** definir/endurecer drops de inimigos comuns, elites e bosses, separando first-time reward, repeat reward, XP e proteções de progressão.  
> **Out of scope:** enemy combat AI/stats, spawn packs, concrete full drop database, boss fight design, XP formula final, UI loot popup.

---

# /speckit.specify

## 1. Contexto

Loot Direction define que inimigos comuns devem dropar materiais de craft mais do que gear pronto; elites têm componente temático garantido/quase garantido; bosses têm first-time reward relevante, componente único/raro, blueprint/recipe e possível lore resource.

Cave Direction preserva BossDefeatStates e define que boss derrotado não deve repetir recompensa única.

---

## 2. Problema

Sem contrato de enemy/boss drops:

```text
boss pode dropar recompensa única várias vezes;
inimigo comum pode virar fonte principal de gear raro;
elite pode não parecer recompensa de risco;
drop raro pode bloquear progressão sem alternativa;
XP/gold/material podem ser inconsistentes por profundidade;
Pedra Negra estabilizada pode aparecer cedo;
EnemyDataSO pode misturar loot com comportamento de forma rígida.
```

---

## 3. Objetivo

Criar/endurecer:

```text
EnemyDropProfile;
EnemyFamilyDropRules;
EliteDropProfile;
BossRewardProfile;
FirstTimeBossReward;
RepeatBossReward;
XPRewardProfile hooks;
DefeatRewardContext;
BossDefeatState integration;
protected/gated drops.
```

---

## 4. Regras de design

```text
Inimigo comum alimenta crafting/economia, não gear raro pronto.
Elite deve parecer recompensa de risco.
Boss drop deve ser direcional, memorável e conectado a lore/progressão.
Boss não deve virar farm infinito de ouro.
Boss first-time reward deve ser separado de repeat reward.
Drop raro não deve bloquear progressão principal sem alternativa.
```

---

## 5. User stories / engineering stories

```text
Como enemy defeat pipeline, quero resolver drops por enemy/family/depth/variant.
Como boss gate, quero conceder first-time reward uma vez e repeat reward controlado depois.
Como crafting, quero receber componentes temáticos por família.
Como economy, quero impedir boss/gold farm infinito.
Como save/load, quero preservar BossDefeatStates e reward consumed.
```

---

## 6. Escopo

Inclui:

```text
enemy drop profile contracts;
elite drop profile;
boss reward profile;
first-time vs repeat reward;
BossDefeatState/reward consumed integration;
depth/biome/family gates;
protected drop validators;
tests.
```

Não inclui:

```text
enemy stats/actions;
spawn resolver;
full concrete roster drops;
boss fights;
XP formula final;
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
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
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

- Inimigos comuns devem dropar pequena chance de material comum, chance moderada de componente temático, baixa chance de componente raro e ouro baixo/moderado conforme família.
- Elites devem dropar componente temático garantido/quase garantido, maior chance de componente raro, ouro melhor e chance de blueprint/gear especial.
- Bosses devem dropar first-time reward relevante, componente único/raro, atalho de craft, blueprint/recipe e possível lore resource.
- Boss não deve virar farm infinito de ouro.
- Boss derrotado não deve repetir recompensa única; repeat reward precisa tabela própria, menor e controlada.
- Cave systems preserve BossDefeatStates and snapshots.

### Deferred / future from directions

- Full concrete enemy family drop table.
- Exact XP/gold tuning.
- Boss fight mechanics.
- Enemy stat/action changes.
- Bestiary UI rewards.
- Level 101 final rewards.

### Explicitly not redefined here

- LootTable contract.
- EnemyDataSO stats/actions.
- Cave snapshot implementation.
- Inventory grant service.
- Recipe/crafting database.
- Boss gate runtime.

## 7. Modelo de domínio

### 7.1 EnemyDropProfile

```text
EnemyDropProfileId
EnemyId optional
EnemyFamilyId
NativeFloorRange
BiomeIds[]
VariantTags[]
CommonMaterialTableId
ThematicComponentTableId
RareComponentTableId
GoldRangeProfileId
XPRewardProfileId optional
NoNormalLoot
DebugTags[]
```

### 7.2 EliteDropProfile

```text
EliteDropProfileId
BaseEnemyFamilyId
GuaranteedOrNearGuaranteedComponent
RareDropTableId
BlueprintChanceProfile
SpecialGearChanceProfile optional
GoldBonusMultiplier
XPBonusMultiplier optional
```

### 7.3 BossRewardProfile

```text
BossRewardProfileId
BossId
BossGateLevel
FirstTimeRewardTableId
RepeatRewardTableId
RequiredStoryFlags[]
GrantedStoryFlags[]
GrantedCheckpoint optional
UnlocksLevel101Access optional
LoreRewardIds[]
BossRewardConsumedFlag
DebugTags[]
```

### 7.4 DefeatRewardContext

```text
EnemyId
EnemyFamilyId
VariantTags[]
CaveLevel
BiomeId
IsElite
IsBoss
BossId optional
RunSeed
SnapshotId
PlayerProgressFlags[]
BossDefeatState
```

---

## 8. Common enemy drop rules

```text
Common enemies:
  - low/moderate gold;
  - thematic crafting material;
  - rare component chance low;
  - gear-ready drop rare/controlled;
  - no unique progression key unless authored and guaranteed elsewhere.
```

---

## 9. Elite drop rules

```text
Elite:
  - guaranteed or near-guaranteed thematic component;
  - better rare component chance;
  - possible blueprint/recipe chance;
  - possible special gear low chance;
  - should not become trivial repeat farm.
```

---

## 10. Boss reward rules

```text
First-time reward:
  - relevant and memorable;
  - may include unique component, blueprint, lore item, checkpoint/story unlock;
  - consumed once and persisted.

Repeat reward:
  - separate table;
  - smaller and controlled;
  - no unique progression item;
  - no infinite gold exploit.
```

---

## 11. Pedra Negra and endgame gating

```text
Pedra Negra instável/corrompida:
  - high-risk, lore/corruption gated.
Pedra Negra estabilizada:
  - deep/endgame or special recipe/story.
Nível 101:
  - no common packs/mining.
  - rewards are lore/endgame, not repeat farm.
```

---

## 12. Criteria

```text
Enemy/elite/boss drop profile contracts exist.
Boss first-time and repeat rewards are separate.
Boss reward consumed state is checked.
Common enemy does not become rare gear source.
Elite has risk-reward identity.
Protected/endgame drops gated.
Tests cover common, elite, boss first-time, boss repeat, reload and protected item.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Loot/Enemies/EnemyDropProfile.cs
Assets/_Game/Scripts/Loot/Enemies/EliteDropProfile.cs
Assets/_Game/Scripts/Loot/Enemies/BossRewardProfile.cs
Assets/_Game/Scripts/Loot/Enemies/DefeatRewardContext.cs
Assets/_Game/Scripts/Loot/Enemies/EnemyDefeatRewardResolver.cs
Assets/_Game/Scripts/Loot/Enemies/BossRewardStateValidator.cs
Assets/_Game/Tests/EditMode/Economy/EnemyBossDropRewardTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Loot/**
Assets/_Game/Scripts/Enemies/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Economy/**
docs/validation/06_spec_enemy_elite_boss_drop_tables_runtime_execution_report.md
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
1. Auditar enemy defeat/drop/boss reward systems.
2. Consolidar enemy/elite/boss drop profiles.
3. Integrar BossDefeatStates/reward consumed.
4. Implementar/harden reward resolver.
5. Implementar validators de protected/endgame drops.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - loot table contract;
  - cave snapshot loot;
  - enemy AI/stats;
  - boss gates;
  - inventory grant service.
- Reason: enemy/boss rewards depend on loot table and cave state.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if BossDefeatStates/reward flags exist; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding reward consumed flags; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. EnemyRewardResolvedEvent, BossFirstTimeRewardGrantedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for enemy/boss reward flow.
```

---

## 21. Riscos

```text
Risco: boss reward duplication.
Mitigação: BossDefeatState/reward consumed tests.

Risco: common enemy farms rare gear.
Mitigação: profile validators.

Risco: progression-critical random drop.
Mitigação: guaranteed/pity/alternative rule.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar enemy/boss reward systems.
- [ ] T003 — Consolidar drop/reward profiles.
- [ ] T004 — Integrar BossDefeatStates.
- [ ] T005 — Implementar reward resolver/validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a enemy/elite/boss drops foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de enemy/elite/boss drops? | Arquivos alterados e justificativa. | PARTIAL |
| Snapshot/idempotência | Loot/reward não duplica após reload, revisit, boss repeat ou coleta repetida? | Testes ou validators. | PARTIAL |
| Economia segura | A spec impede gold/hour absurdo, rare loot cedo demais e reward farm infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/06_spec_enemy_elite_boss_drop_tables_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "EnemyDrop|EnemyDropProfile|EliteDrop|BossReward|BossDefeatState|FirstTimeReward|RepeatReward|XPReward|NoNormalLoot" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a enemy/elite/boss drops existe ou foi criado de forma mínima
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

- Boss first-time reward concedido duas vezes.
- RepeatReward contém unique/progression key.
- Common enemy dropa gear raro pronto como fonte principal.
- Elite sem reward temático.
- Pedra Negra estabilizada aparece cedo.
- Progression-critical item é rare random sem alternativa.
- Enemy loot altera EnemyDataSO combat stats indevidamente.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Enemy Elite Boss Drop Tables Runtime

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


## 23G. Enemy Reward Matrix

| Source | Reward type | Guardrail |
|---|---|---|
| Common enemy | thematic/common materials | no rare gear source |
| Variant enemy | variant-themed material | gated by variant |
| Elite | guaranteed/near guaranteed component | risk reward |
| Boss first-time | unique/lore/blueprint/unlock | once only |
| Boss repeat | smaller controlled table | no unique/progression item |
| Level 101 boss | endgame/lore | no farm loop |

## 23H. Boss Reward Idempotency

```text
If BossDefeatState says first-time consumed:
  use repeat table only.
If repeat table missing:
  grant no repeat reward or safe minimal reward.
Never grant first-time unique twice.
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

- Changed deterministic logic: YES, enemy/boss reward selection and idempotency logic is deterministic.
- Requires EditMode tests: YES for common/elite/boss-first/boss-repeat/reload/protected drop tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for enemy/boss reward visual flow.
- Requires regression test: YES if fixing existing boss reward/drop duplication bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no first-time duplication; no rare/endgame leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/06_spec_enemy_elite_boss_drop_tables_runtime_execution_report.md.
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
