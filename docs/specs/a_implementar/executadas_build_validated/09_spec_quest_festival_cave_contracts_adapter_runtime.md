# SPEC — Quest Festival Cave Contracts Adapter Runtime

> **Spec ID:** `09_spec_quest_festival_cave_contracts_adapter_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P1  
> **Type:** Runtime / Quest Adapter / Festival / Cave Contracts  
> **Domain:** Quest / FestivalQuest / CaveContract / Expiry / Risk / Reward  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_ADAPTERS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere festival calendar runtime, cave run state, enemy spawn, loot/reward tables, city contract services, quest save schema, UI board/festival screens ou main progression/Fonte hooks.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Cave/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_festival_cave_contracts_adapter_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> **Blocks:**  
  - festival quest UI;
  - festival stalls/minigames;
  - cave contract board;
  - roads guild services;
  - cave reward balance;
  - main quest cave gates.
> **Scope:** definir/endurecer adapters de Festival quests e CaveContracts para o sistema genérico de quests, com expiry, deadlines, cave run constraints, reward idempotency e anti-softlock.  
> **Out of scope:** festival minigames/stalls final, enemy spawn implementation, cave procedural changes, full contract catalogs, UI final, main quest boss gates.

---

# /speckit.specify

## 1. Contexto

Festival quests podem expirar quando festival acaba e devem informar data/horário quando conhecidas. CaveContracts usam objetivos como caçar inimigo, coletar recurso, atingir profundidade, derrotar elite, descobrir fraqueza e mapear área; podem ter risco maior, expirar por calendário se contrato e não conflitar com state da cave run.

Esta spec cria adapters de Festival e CaveContract sem implementar conteúdo final.

---

## 2. Problema

Sem adapters:

```text
festival quest pode continuar ativa após evento acabar;
festival pode bloquear rotina agrícola essencial sem aviso;
cave contract pode depender de spawn raro sem fallback;
cave contract pode conflitar com CaveRunSeed/snapshot;
contrato pode pagar várias vezes;
elite/boss/rare resource pode virar farm de reward;
data/horário de festival pode não aparecer quando conhecido;
contrato pode ser aceito sem acesso real à cave depth.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FestivalQuestAdapter;
FestivalQuestDefinition;
FestivalExpiryPolicy;
CaveContractDefinition;
CaveContractObjectiveAdapter;
CaveContractRiskPolicy;
CaveRunState compatibility;
ContractRewardPolicy;
Expiry/idempotency;
validators/tests.
```

---

## 4. Regras de design

```text
Festival quest pode expirar no fim do festival.
Festival quest deve informar data/horário quando descoberta.
Festival não deve impedir rotina agrícola essencial sem aviso.
CaveContract pode ter risco maior e prazo.
CaveContract não deve conflitar com cave run state.
Defeat objective usa EnemyId/FamilyId/BossId estável.
Quest não deve depender de spawn raro sem fallback.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero saber prazo de festival/contrato quando conhecido.
Como festival, quero expirar quests corretamente ao final.
Como guilda/contrato, quero objetivos de caverna usando ids estáveis.
Como cave, quero contracts compatíveis com run seed/snapshot/depth.
Como economy, quero reward de risco sem farm infinito.
```

---

## 6. Escopo

Inclui:

```text
festival quest adapter;
festival expiry policy;
cave contract definition;
cave contract objective mappings;
enemy family/depth/resource/weakness objectives;
cave run compatibility policy;
reward/expiry idempotency;
tests/validators.
```

Não inclui:

```text
festival minigames;
festival stalls;
full event calendar implementation;
enemy spawn changes;
cave procedural changes;
final contract catalog;
UI board/festival screen final.
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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

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

- Festival quests podem expirar quando o festival acaba e devem informar data/horário quando conhecidas.
- Festival não deve impedir rotina agrícola essencial sem aviso.
- CaveContract é usado para caçar inimigo, coletar recurso, atingir profundidade, derrotar elite, descobrir fraqueza e mapear área.
- CaveContract pode ter risco maior, pode expirar por calendário se for contrato e não deve conflitar com state da cave run.
- Cave/combat objectives incluem ReachCaveDepth, DefeatEnemy, DefeatEnemyFamily, DefeatBoss, CollectCaveResource, DiscoverWeakness, CompleteCaveRun, RecoverCorpse e InteractWithCaveObject.
- Defeat objective deve usar EnemyId/FamilyId/BossId estável; quest não deve depender de spawn raro sem fallback.

### Deferred / future from directions

- Festival minigames/stalls.
- Full festival calendar implementation.
- Final cave contract catalog.
- Enemy spawn tables.
- Cave procedural changes.
- UI board/festival screens.
- Main quest cave gates.

### Explicitly not redefined here

- Quest core contracts.
- Reward application.
- World calendar/festival runtime.
- CaveRunManager/snapshot.
- Enemy roster/family ids.
- Loot/reward tables.

## 7. Modelo de domínio

### 7.1 FestivalQuestDefinition

```text
FestivalQuestId
QuestId
FestivalId
KnownStartDay
KnownStartTime
KnownEndDay
KnownEndTime
ObjectiveIds[]
RewardTableId
ExpiryPolicy
VisibilityPolicyId
CanBeCompletedAfterFestival
AffectsEssentialFarmRoutine
WarningRequired
DebugTags[]
```

### 7.2 FestivalExpiryPolicy

```text
ExpireAtFestivalEnd
ExpireAtDayEnd
ExpireAtSpecificTime
RemainAvailableForTurnIn
NeverExpireStoryOnly
```

### 7.3 CaveContractDefinition

```text
CaveContractId
QuestId
ProviderServiceId optional
ContractType
RequiredCaveAccessDepth
AllowedDepthRange
AllowedBiomeIds[]
RequiredEnemyId optional
RequiredEnemyFamilyId optional
RequiredBossId optional
RequiredResourceId optional
RequiredKnowledgeId optional
RequiredQuantity
DeadlinePolicy
RiskTier
RewardTableId
RepeatPolicy
FallbackPolicyId
DebugTags[]
```

### 7.4 CaveContractType

```text
DefeatEnemy
DefeatEnemyFamily
DefeatElite
DefeatBoss
CollectCaveResource
ReachCaveDepth
CompleteCaveRun
DiscoverWeakness
MapArea
RecoverCorpse
InteractWithCaveObject
```

### 7.5 ContractCompletionResult

```text
Success
FailureReason
ProgressApplied
RewardApplied
Expired
RepeatInstanceId optional
CaveRunSeed optional
SnapshotReference optional
DebugNotes[]
```

---

## 8. Festival rules

```text
Festival quest can expire at festival end.
Known festival date/time appears in Quest Log.
Hidden festival condition stays hidden until discovered.
Festival quest must not block core farm routine without warning.
Festival quest reward applies once.
Festival quest can remain available for turn-in only if policy says so.
```

---

## 9. Cave contract rules

```text
Contract must validate cave access.
Depth contract uses stable depth reached state.
Enemy contract uses EnemyId/FamilyId/BossId stable id.
Elite contract cannot depend on rare spawn without fallback or guaranteed contract spawn policy.
Resource contract must respect cave snapshot/depleted nodes.
Weakness contract uses Bestiary/Knowledge, not direct hidden data.
CompleteCaveRun contract must respect run result.
RecoverCorpse contract must not punish main quest.
```

---

## 10. Cave run compatibility

```text
Contract must not mutate CaveRunSeed.
Contract must not reroll cave snapshot.
Contract progress can read cave events.
Contract should handle player death/recovery.
Contract progress should survive leaving/reentering if QuestState supports it.
Contract cannot require level 101 common farming.
```

---

## 11. Reward and economy guardrails

```text
Riskier cave contracts can pay better.
Repeat contracts must be bounded.
Boss/elite unique reward cannot be repeated through contract.
Contract reward uses QuestRewardApplicator.
Contract cannot become guaranteed infinite gold/hour exploit.
```

---

## 12. Criteria

```text
Festival and cave contract adapters exist or are hardened.
Festival expiry and known time projection exist.
Cave contracts map to standard objective types/triggers.
Cave contract uses stable IDs.
Contracts avoid rare spawn softlock.
Reward/expiry idempotency exists.
Tests cover festival expiry, remain-available turn-in, cave depth, enemy family, resource, weakness, repeat and reward duplication.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Festivals/FestivalQuestDefinition.cs
Assets/_Game/Scripts/Quests/Festivals/FestivalQuestAdapter.cs
Assets/_Game/Scripts/Quests/Festivals/FestivalExpiryPolicy.cs
Assets/_Game/Scripts/Quests/CaveContracts/CaveContractDefinition.cs
Assets/_Game/Scripts/Quests/CaveContracts/CaveContractType.cs
Assets/_Game/Scripts/Quests/CaveContracts/CaveContractObjectiveAdapter.cs
Assets/_Game/Scripts/Quests/CaveContracts/CaveContractValidator.cs
Assets/_Game/Tests/EditMode/Quests/FestivalCaveContractAdapterTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/Enemies/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_festival_cave_contracts_adapter_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar festival/cave contract systems.
2. Consolidar festival quest adapter/expiry.
3. Consolidar cave contract definition/objective adapter.
4. Integrar reward idempotency.
5. Implementar validators de cave run/rare spawn/repeat reward.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - Quest core contracts;
  - world calendar/festival runtime;
  - cave run/snapshot runtime;
  - enemy spawn/loot tables;
  - reward engine.
- Reason: adapters cross quest, world, cave and economy state.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if QuestState handles expiry/repeat ids; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO unless adapter-specific state is missing; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds event subscribers: CONDITIONAL if adapters subscribe to quest/event router.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes Quest Log/board/festival projection data.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for festival/contract board flow.
```

---

## 21. Riscos

```text
Risco: festival quest expires incorrectly.
Mitigação: expiry tests.

Risco: cave contract depends on rare spawn.
Mitigação: fallback/guaranteed policy.

Risco: contract conflicts with CaveRunSeed.
Mitigação: cave run compatibility tests.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar festival/cave contract systems.
- [ ] T003 — Consolidar festival adapter/expiry.
- [ ] T004 — Consolidar cave contract adapter.
- [ ] T005 — Integrar reward/expiry/repeat validation.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest festival and cave contract adapters foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Quest core | A spec usa os contratos de QuestDefinition, QuestState, Condition, Trigger, Reward e QuestFlag existentes? | Referência explícita no report. | PARTIAL |
| Separação de estado | QuestState, QuestFlag, MainProgression e FonteAnya permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Anti-softlock | Quest crítica, order ou contrato não cria bloqueio irreversível sem fallback? | Plano/validator. | PARTIAL |
| Anti-spoiler | UI/log/flags/adapters não revelam Arquivista, 101, finais ou Anya cedo? | Checklist/visibility. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Economia | FarmOrder/Contract/Festival não gera reward infinito nem buy/sell exploit? | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_festival_cave_contracts_adapter_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "FestivalQuest|FestivalExpiry|CaveContract|DefeatEnemyFamily|ReachCaveDepth|DiscoverWeakness|ContractReward|RepeatPolicy|CaveRunSeed" Assets/_Game/Scripts docs/design docs/specs
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a quest festival and cave contract adapters existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And QuestState, QuestFlag, rewards, UI projection e adapters permanecem consistentes
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

### Scenario 3 — Anti-spoiler / visibility

```text
Given quest, flag, order, festival, cave contract ou adapter contém informação oculta
When a UI/log/projection/validator consulta o estado
Then só conhecimento autorizado aparece
And passos futuros, rewards secretos, boss oculto, nível 101, finais e segredos de Anya permanecem ocultos até descoberta.
```

### Scenario 4 — Idempotency and expiry

```text
Given reward, flag, order, festival ou contract já foi aplicado/completado/expirado
When reload, day transition, event repeat ou reentrada ocorre
Then reward/flag não duplica
And expired/completed state não retorna ativo sem repeat policy explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Quest Log, board, festival, cave contract, notification ou delivery UI
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Festival quest persists active after festival ends incorrectly.
- Known festival time not shown.
- Cave contract requires rare spawn without fallback.
- Contract mutates CaveRunSeed.
- Resource contract rerolls cave snapshot.
- Repeat contract grants unique/boss reward.
- Contract accepted without access to required depth.
- Reward duplicates after reload.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Festival Cave Contracts Adapter Runtime

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

## Canon / quest compliance
- Quest core contracts reused:
- QuestState vs QuestFlag separation:
- MainProgression vs FonteAnya separation:
- Anti-softlock:
- Anti-spoiler:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:
- No reward/economy exploit:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler/visibility:
- Idempotency/expiry:
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
4. A implementação exigir reescrever Quest/Save/EventBus/Fonte/MainProgression canônico existente.
5. A implementação esconder MainProgression ou FonteAnya dentro de QuestState genérico.
6. A implementação usar QuestFlag como substituto para estado complexo de quest.
7. A implementação permitir reward duplicado após reload/evento repetido.
8. A implementação revelar cedo Arquivista do Silêncio, final choices, nível 101, boss oculto ou segredo de Anya.
9. A implementação tornar main quest expirável por tempo/calendário.
10. A implementação criar PetFuture, SocialFuture, romance/casamento profundo ou companion full runtime fora do escopo.
11. A implementação gerar reward infinito por FarmOrder/Festival/CaveContract.
12. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Adapter Objective Matrix

| Adapter | Objective | Trigger |
|---|---|---|
| Festival attend | AttendFestival | OnFestivalStarted/Ended |
| Festival activity | WinFestivalActivityFuture | future only |
| Cave depth | ReachCaveDepth | OnCaveDepthReached |
| Enemy family | DefeatEnemyFamily | OnEnemyFamilyDefeated |
| Boss | DefeatBoss | boss event, stable BossId |
| Resource | CollectItem/CollectCaveResource | OnItemCollected |
| Weakness | DiscoverWeakness | OnWeaknessDiscovered |
| Complete run | CompleteCaveRun | OnCaveRunCompleted |
| Corpse | RecoverCorpse | OnCorpseRecovered |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de Quest Log, board, Festival, FarmOrder, CaveContract ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, festival expiry/cave contract/progress/repeat logic is deterministic.
- Requires EditMode tests: YES for festival expiry/time/depth/enemy/resource/weakness/repeat/reward tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for festival/contract board visual flow.
- Requires regression test: YES if fixing existing festival/contract adapter bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no cave run conflict; no repeat reward exploit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_festival_cave_contracts_adapter_runtime_execution_report.md.
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
Não misturar QuestState com MainProgression/FonteAnya.
Não usar QuestFlag como substituto de QuestState.
Não aplicar reward/flag duas vezes.
Não revelar spoiler oculto cedo.
Não fazer main quest expirar por tempo.
Não implementar PetFuture/SocialFuture/romance profundo.
Não criar exploit de reward por order/festival/contract.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
