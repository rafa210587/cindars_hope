# SPEC — Quest Flags Registry Runtime

> **Spec ID:** `09_spec_quest_flags_registry_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P0  
> **Type:** Runtime / Data Contract / Quest Flags / Registry  
> **Domain:** Quest / QuestFlag / Registry / Fact State / Cross-system Query  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_ADAPTERS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState save/load, MainProgressionSection, FonteAnyaSection, reward application, quest content, dialogue/shop/city unlocks ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_flags_registry_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
> **Blocks:**  
  - quest conditions;
  - dialogue/service/shop unlocks;
  - quest rewards;
  - farm/city/cave adapters;
  - anti-softlock validators.
> **Scope:** definir/endurecer QuestFlag registry, tipos, ownership, visibility/spoiler, persistence policy e query API sem usar flags como substituto de QuestState/MainProgression/FonteAnya.  
> **Out of scope:** conteúdo final de flags, save migration, quest content, MainProgression/Fonte concrete state.

---

# /speckit.specify

## 1. Contexto

QuestFlag é fato persistente consultável por sistemas. O direction alerta que QuestFlag não substitui QuestState: não esconder progressão complexa em flags soltas quando QuestState deveria existir. QuestFlag também não deve engolir MainProgression ou FonteAnya.

Esta spec cria o registry e validação de flags.

---

## 2. Problema

Sem registry:

```text
flags podem ser string solta;
typo em flag pode quebrar unlock;
flag oculta pode aparecer na UI;
QuestFlag pode virar estado complexo de quest;
MainProgression pode ser reduzida a flags soltas;
FonteAnya pode virar FonteLivingWaterUnlocked sem seção própria;
flag pode ser setada sem owner/reward/evento;
flag pode não persistir ou persistir demais.
```

---

## 3. Objetivo

Criar/endurecer:

```text
QuestFlagDefinition;
QuestFlagRegistry;
QuestFlagType;
QuestFlagScope;
QuestFlagVisibility;
QuestFlagOwnership;
Query API;
Set/Clear validation;
Spoiler policy;
Save/load bridge.
```

---

## 4. Regras de design

```text
QuestFlag registra fato persistente consultável.
QuestFlag não substitui QuestState.
QuestFlag não substitui MainProgressionSection.
QuestFlag não substitui FonteAnyaSection.
Flags têm owner/scope/visibility/spoiler.
Flags ocultas não aparecem cru no Quest Log.
Set/Clear deve ser idempotente.
```

---

## 5. User stories / engineering stories

```text
Como quest condition, quero consultar flag sem string solta.
Como service/shop/dialogue, quero unlock por flag validada.
Como reward engine, quero set/clear idempotente.
Como UI, quero saber se flag é secreta.
Como validator, quero bloquear flags que deveriam ser QuestState/Main/Fonte.
```

---

## 6. Escopo

Inclui:

```text
flag definition contract;
registry;
flag types/scopes/visibility;
query/set/clear API contract;
ownership rules;
spoiler policy;
validator/tests.
```

Não inclui:

```text
final list of all flags;
save migration;
main progression state;
FonteAnya state;
quest content.
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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md

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

- QuestFlag é fato persistente consultável por sistemas.
- QuestFlag registra fatos que outras quests/sistemas podem consultar.
- Não usar QuestFlag como substituto para todos os estados de quest.
- Não esconder progressão complexa em flags soltas quando QuestState deveria existir.
- Exemplos de flags: FonteRespawnUnlocked, FragmentWaterProtected, CindarDiaryRead, VaelrionIntroduced, NyxCultRumorKnown, BlackStoneObserved, TownKnowsAnyaRumor, ShopNightUnlocked, CaveGateMemoryOpened, ArchivistNameKnown.
- QuestState, MainProgression e FonteAnya são seções separadas.

### Deferred / future from directions

- Final registry complete.
- Save migration.
- MainProgression concrete implementation.
- FonteAnya concrete implementation.
- Quest content data.
- UI display of flags.

### Explicitly not redefined here

- QuestState save/load.
- Reward application.
- Condition resolver.
- Quest Log projection.
- Service/dialogue/shop adapters.

## 7. Modelo de domínio

### 7.1 QuestFlagType

```text
Boolean
Integer
String
Enum
Counter
Timestamp
```

### 7.2 QuestFlagScope

```text
QuestLocal
GlobalStory
City
Farm
Cave
FonteReferenceOnly
MainProgressionReferenceOnly
Shop
Dialogue
Festival
Debug
```

### 7.3 QuestFlagVisibility

```text
PublicKnown
PlayerKnownAfterDiscovery
HiddenInternal
DebugOnly
SpoilerLocked
```

### 7.4 QuestFlagDefinition

```text
FlagId
DisplayNameKey optional
DescriptionKey optional
FlagType
Scope
Visibility
SpoilerTier
OwnerSystem
AllowedSetters[]
AllowedClearers[]
Persists
DefaultValue
CanAppearInQuestLog
CanBeUsedByConditions
CanBeGrantedByReward
IsDeprecated
ReplacementFlagId optional
DebugTags[]
```

### 7.5 QuestFlagRegistryQuery

```text
FlagId
ExpectedType optional
RequiredVisibility optional
RequestingSystem
AllowHidden
```

---

## 8. Ownership rules

```text
Quest system owns general quest flags.
City systems may query city-scoped flags.
Shop systems may query shop unlock flags.
Dialogue systems may query dialogue/lore flags.
Farm/cave systems may query flags but must not hide own state as flags.
Fonte-related flags are reference/trigger flags only; actual Fonte state lives in FonteAnyaSection.
Main progression flags are reference/trigger flags only; actual main progression lives in MainProgressionSection.
```

---

## 9. Examples and classification

```text
FonteRespawnUnlocked:
  scope FonteReferenceOnly, not source of truth for Fonte state.

FonteLivingWaterUnlocked:
  scope FonteReferenceOnly, derived/reference; source is FonteAnyaSection.

FragmentWaterProtected:
  scope MainProgressionReferenceOnly; source is MainProgressionSection.

CindarDiaryRead:
  scope GlobalStory or QuestLocal.

VaelrionIntroduced:
  scope GlobalStory.

NyxCultRumorKnown:
  scope Dialogue/GlobalStory.

BlackStoneObserved:
  scope GlobalStory/Cave.

TownKnowsAnyaRumor:
  scope City/Dialogue.

ShopNightUnlocked:
  scope Shop/City.

CaveGateMemoryOpened:
  scope Cave/MainProgressionReferenceOnly.

ArchivistNameKnown:
  scope GlobalStory, high SpoilerTier.
```

---

## 10. Set/clear rules

```text
Set is idempotent.
Clear is idempotent.
Invalid flag id fails loudly in dev/test.
Type mismatch fails.
Hidden flag cannot be shown in UI.
Deprecated flag logs warning and may map to replacement.
Setter must be authorized by AllowedSetters.
QuestFlagGrant rewards record GrantedFlagIds.
```

---

## 11. Criteria

```text
QuestFlagDefinition/Registry exists or is hardened.
Flag query/set/clear API is type-safe or validated.
Flags have scope, owner and visibility.
Fonte/Main flags are reference-only, not source of truth.
QuestFlag not used as QuestState replacement.
Tests cover valid flag, invalid id, type mismatch, hidden visibility, idempotent set/clear and Fonte/Main guardrails.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Flags/QuestFlagType.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagScope.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagVisibility.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagDefinition.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagRegistry.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagService.cs
Assets/_Game/Scripts/Quests/Flags/QuestFlagValidator.cs
Assets/_Game/Tests/EditMode/Quests/QuestFlagRegistryTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_flags_registry_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar flag/string unlock systems.
2. Consolidar QuestFlagDefinition/Registry.
3. Implementar/harden QuestFlagService.
4. Implementar validators for scope/owner/visibility.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - quest state save/load;
  - reward application;
  - main progression/Fonte save;
  - dialogue/service unlock specs.
- Reason: registry is shared state vocabulary.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if QuestFlags already persist; otherwise STOP.
Does this add save section? NO unless dedicated save migration is approved.
Does this require migration? NO unless current flags are unstructured and must be migrated; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. QuestFlagChangedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO; provides visibility info to UI.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO by default.
```

---

## 20. Riscos

```text
Risco: existing string flags not migrated.
Mitigação: STOP or compatibility layer only.

Risco: flag becomes state dumping ground.
Mitigação: scope/owner validator.

Risco: hidden flag shown.
Mitigação: visibility tests.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar quest flag/unlock systems.
- [ ] T003 — Consolidar flag registry.
- [ ] T004 — Implementar service/query/set/clear.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest flags registry foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
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
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_flags_registry_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "QuestFlag|FlagRegistry|FlagId|QuestFlagGrant|QuestFlagClear|FonteRespawnUnlocked|ShopNightUnlocked|ArchivistNameKnown|MainProgression|FonteAnya" Assets/_Game/Scripts docs/design .specs
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a quest flags registry existe ou foi criado de forma mínima
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

- Flag typo creates silent unlock failure.
- Flag hidden shown in UI.
- Flag used as QuestState replacement.
- Main progression stored as many flags.
- Fonte state stored only as flag.
- Flag type mismatch ignored.
- Flag grant repeats without GrantedFlagIds.
- Deprecated flag keeps being used.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Flags Registry Runtime

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


## 23G. QuestFlag Ownership Matrix

| Scope | Owns source state? | Notes |
|---|---:|---|
| QuestLocal | yes | quest-specific facts |
| GlobalStory | yes | broad discovered facts |
| City/Farm/Cave | mixed | only facts/unlocks, not full subsystem state |
| FonteReferenceOnly | no | source is FonteAnyaSection |
| MainProgressionReferenceOnly | no | source is MainProgressionSection |
| Shop/Dialogue/Festival | yes for unlock facts | not full service state |
| Debug | no production | validation/dev only |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, flag registry/query/set/clear/validation logic is deterministic.
- Requires EditMode tests: YES for valid/invalid/type/visibility/idempotent/Fonte-Main guardrail tests.
- Requires PlayMode automated or final human scenario: NO by default; data/service only.
- Requires regression test: YES if fixing existing flag registry bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; flags typed/validated; no Fonte/Main state swallowed.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_flags_registry_runtime_execution_report.md.
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
