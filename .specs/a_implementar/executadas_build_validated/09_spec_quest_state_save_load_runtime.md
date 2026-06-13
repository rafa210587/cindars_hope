# SPEC — Quest State Save Load Runtime

> **Spec ID:** `09_spec_quest_state_save_load_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P0  
> **Type:** Runtime / Save Load / Quest State  
> **Domain:** Quest / SaveLoad / QuestStateSection / QuestFlags / Normalization  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestDefinition contract, MainProgressionSection, FonteAnyaSection, save migration, reward idempotency, quest log UI, farm orders save, festival expiry ou final choice state.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_state_save_load_runtime_execution_report.md`  
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
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> **Blocks:**  
  - quest reward idempotency;
  - quest log projection;
  - farm order/festival/cave contract states;
  - main progression hooks;
  - anti-softlock normalization.
> **Scope:** definir/endurecer QuestStateSection save/load, QuestState persistence, QuestFlags, ChoiceHistory, GrantedRewardIds/GrantedFlagIds e load normalization sem misturar MainProgression/FonteAnya.  
> **Out of scope:** save migration real, UI, quest content, reward application engine, main progression/Fonte state implementation.

---

# /speckit.specify

## 1. Contexto

O direction separa explicitamente `QuestStateSection`, `MainProgressionSection` e `FonteAnyaSection`. QuestState persiste quests genéricas, side, orders, festival, tutorial e hidden; MainProgression persiste atos/fragmentos/Cindar/Arco/nivel 100/101/finais; FonteAnya persiste funções da Fonte, Água Viva, respec, purificação, decisão final e corrupção/estado.

Esta spec cobre apenas `QuestStateSection` e quest flags genéricas.

---

## 2. Problema

Sem save/load próprio:

```text
quest concluída pode voltar ativa;
reward pode ser aplicado duas vezes;
objective progress pode ser perdido;
quest flag pode sumir ou duplicar;
choice history pode não persistir;
HiddenCompleted pode aparecer no log por erro;
main progression pode ficar escondida em QuestState;
FonteAnya pode virar flag solta;
load pode não normalizar estado inconsistente.
```

---

## 3. Objetivo

Criar/endurecer:

```text
QuestStateSection;
QuestStateRecord;
QuestObjectiveStateRecord;
QuestFlags save;
KnownObjectiveIds;
KnownHints;
ChoiceHistory;
GrantedRewardIds;
GrantedFlagIds;
Tracked/Discovered;
load normalization;
no MainProgression/FonteAnya merge.
```

---

## 4. Regras de design

```text
QuestState é persistido.
QuestDefinition é referenciada por QuestId.
QuestState, MainProgression e FonteAnya são seções separadas.
QuestFlag não substitui QuestState.
Reward precisa registrar GrantedRewardIds quando necessário.
Flags concedidas registram GrantedFlagIds quando necessário.
Load deve normalizar inconsistências recuperáveis sem esconder erro.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero que quest ativa, objetivo, pista, escolha e recompensa sobrevivam a save/load.
Como runtime, quero recuperar estado por QuestId.
Como reward engine, quero saber se reward já foi aplicado.
Como anti-softlock, quero normalizar estados impossíveis no load.
Como main progression, quero ficar separado do QuestState genérico.
```

---

## 6. Escopo

Inclui:

```text
QuestStateSection data contract;
QuestStateRecord;
ObjectiveStateRecord;
QuestFlag persisted set;
ChoiceHistory;
GrantedRewardIds/GrantedFlagIds;
load normalization rules;
save/load validators;
tests.
```

Não inclui:

```text
save schema migration;
QuestDefinition data assets;
reward engine;
condition/trigger runtime;
Quest Log UI;
MainProgressionSection implementation;
FonteAnyaSection implementation.
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

- QuestState save deve persistir QuestId, State, CurrentStepId, CompletedStepIds, FailedStepIds, ObjectiveStates, KnownObjectiveIds, KnownHints, StartedAtDay, StartedAtTime, CompletedAtDay, ExpiresAtDay, Tracked, Discovered, FailureReason, ChoiceHistory, GrantedRewardIds e GrantedFlagIds.
- QuestFlags são persistentes e consultáveis por outros sistemas.
- QuestStateSection persiste quests genéricas; MainProgressionSection persiste atos, fragmentos, Cindar, Arco, nível 100/101 e finais; FonteAnyaSection persiste funções da Fonte, Água Viva, respec, purificação, decisão final e corrupção/estado.
- QuestState, MainProgression e FonteAnya são seções separadas.
- Reward não deve aplicar duas vezes após save/load.
- Main quest deve ser recuperável sem intervenção externa.

### Deferred / future from directions

- Save migration real.
- MainProgression concrete state.
- FonteAnya concrete state.
- Reward engine.
- Quest Log UI.
- Quest content data.

### Explicitly not redefined here

- QuestDefinition contract.
- Save infrastructure.
- Reward application engine.
- Condition/trigger engine.
- Main quest progression systems.
- Fonte systems.

## 7. Modelo de domínio

### 7.1 QuestStateSection

```text
Version
QuestStates[]
QuestFlags[]
GlobalKnownHints[]
LastQuestStateNormalizationVersion
DebugLastValidatedAt optional
```

### 7.2 QuestStateRecord

```text
QuestId
State
CurrentStepId
CompletedStepIds[]
FailedStepIds[]
ObjectiveStates[]
KnownObjectiveIds[]
KnownHints[]
StartedAtDay
StartedAtTime
CompletedAtDay optional
ExpiresAtDay optional
Tracked
Discovered
FailureReason optional
ChoiceHistory[]
GrantedRewardIds[]
GrantedFlagIds[]
RepeatInstanceId optional
```

### 7.3 QuestObjectiveStateRecord

```text
ObjectiveId
State: Unknown | Known | Active | Completed | Failed | OptionalSkipped
CurrentAmount
RequiredAmountSnapshot
LastProgressDay optional
LastTriggerId optional
FailureReason optional
```

### 7.4 QuestFlagSaveRecord

```text
FlagId
Value: true/false/string/int optional
GrantedByQuestId optional
GrantedByRewardId optional
GrantedAtDay optional
IsHidden
DebugTags[]
```

### 7.5 ChoiceHistoryRecord

```text
ChoiceId
QuestId
StepId
ChoiceType
SelectedOptionId
SelectedAtDay
IsIrreversible
ConfirmationShown
FutureConsequenceTags[]
```

---

## 8. Load normalization

On load, resolver must check:

```text
QuestState record references valid QuestDefinition.
CurrentStepId exists in QuestDefinition unless quest terminal.
CompletedStepIds and FailedStepIds exist.
ObjectiveStates reference valid ObjectiveIds.
KnownObjectiveIds do not reveal hidden future objectives.
GrantedRewardIds/GrantedFlagIds are unique.
Tracked cannot be true for Unknown/Hidden undiscovered quest.
Completed quest cannot be Active unless recovery path explicitly.
Expired FarmOrder/Festival has visible expiry state.
Main/Fonte data not inside QuestStateSection.
```

Allowed normalization examples:

```text
duplicate GrantedRewardId -> dedupe and report warning.
Tracked hidden unknown quest -> set Tracked=false and report.
Objective completed before quest started -> preserve and allow retroactive trigger if policy allows.
Missing definition -> mark Blocked and report, not delete.
```

---

## 9. Anti-softlock load rules

```text
If critical quest item was sold/discarded:
  do not silently fail; mark recovery needed or restore/protect via policy if system exists.

If NPC unavailable:
  quest state can remain Waiting with known availability hint.

If weather/lunar condition was missed:
  preserve Waiting and next known opportunity if discovered.

If player died/corpse pending:
  quest should not fail main progression unless explicit non-main contract.

If reward applied but state not completed:
  use GrantedRewardIds to avoid duplicate and mark repair needed.
```

---

## 10. Criteria

```text
QuestStateSection exists or is hardened.
QuestState persists all required fields.
QuestFlags are separate from QuestState.
MainProgression/FonteAnya not stored in QuestStateSection.
Load normalization detects invalid references and duplicates.
GrantedRewardIds/GrantedFlagIds dedupe reward/flag replay.
Tests cover save/load roundtrip, duplicate reward ids, hidden tracked normalization, invalid references and section separation.
```

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Save/QuestStateSection.cs
Assets/_Game/Scripts/Quests/Save/QuestStateRecord.cs
Assets/_Game/Scripts/Quests/Save/QuestObjectiveStateRecord.cs
Assets/_Game/Scripts/Quests/Save/QuestFlagSaveRecord.cs
Assets/_Game/Scripts/Quests/Save/ChoiceHistoryRecord.cs
Assets/_Game/Scripts/Quests/Save/QuestStateSaveLoadAdapter.cs
Assets/_Game/Scripts/Quests/Save/QuestStateLoadNormalizer.cs
Assets/_Game/Tests/EditMode/Quests/QuestStateSaveLoadTests.cs
```

Consolidar existentes se houver.

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_state_save_load_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/MainProgression/**
Assets/_Game/Scripts/Fonte/**
```

A leitura é permitida apenas para validar separação; não alterar sem spec própria.

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar save/load quest state existente.
2. Consolidar QuestStateSection/records.
3. Implementar/harden load normalization.
4. Garantir GrantedRewardIds/GrantedFlagIds dedupe.
5. Validar separação MainProgression/FonteAnya.
6. Criar tests.
7. Criar report.
```

---

## 15. Paralelização

- Parallelizable: NO
- Must not run with:
  - save/load global schema;
  - quest reward idempotency;
  - main progression/Fonte save specs;
  - quest log UI;
  - condition/trigger engine.
- Reason: quest save/load is core state.

---

## 16. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if QuestStateSection exists; otherwise STOP.
Does this add save section? NO unless dedicated save migration is approved.
Does this require migration? NO unless adding QuestStateSection; then STOP.
Does this persist Unity references? NO.
```

---

## 17. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO.
```

---

## 18. Impacto UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO by default; save/load tests are enough unless UI integration is touched.
```

---

## 19. Riscos

```text
Risco: save schema absent.
Mitigação: STOP; do not create migration silently.

Risco: over-normalization hides bug.
Mitigação: report warnings/errors.

Risco: QuestState swallows Fonte/Main progression.
Mitigação: section separation validator.
```

---

# /speckit.tasks

## 20. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar quest save/load.
- [ ] T003 — Consolidar QuestStateSection records.
- [ ] T004 — Implementar load normalizer.
- [ ] T005 — Implementar section separation checks.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest state save/load foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Quest core | A spec preserva Definition -> State -> Steps -> Objectives -> Conditions -> Triggers -> Rewards -> Flags? | Contrato/referências no report. | PARTIAL |
| Separação de estado | QuestState, QuestFlag, MainProgression e FonteAnya permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Anti-softlock | Quest crítica não fica impossível por item perdido, NPC, tempo, lua, morte ou save/load? | Plano/validator. | PARTIAL |
| Anti-spoiler | Quest log/visibility/rewards ocultos não revelam cedo Arquivista, final, 101, boss ou segredo? | Checklist/visibility. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_state_save_load_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "QuestStateSection|QuestStateRecord|ObjectiveStates|KnownObjectiveIds|ChoiceHistory|GrantedRewardIds|GrantedFlagIds|QuestFlags|MainProgressionSection|FonteAnyaSection" Assets/_Game/Scripts docs/design .specs
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a quest state save/load existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, quest state, quest flags, main progression, FonteAnya e UI projection permanecem consistentes
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

### Scenario 3 — Critical quest anti-softlock

```text
Given uma quest crítica depende de item, NPC, clima, lua, caverna, morte, reward ou save/load
When o jogador perde item, recarrega, morre, muda horário ou completa step antes da quest iniciar
Then existe fallback, trigger retroativo, item protegido, NPC alternativo, normalização no load ou bloqueio explícito
And a main quest não fica impossível.
```

### Scenario 4 — Idempotency and replay safety

```text
Given objective, trigger, reward, flag ou branch já foi aplicado
When o mesmo evento chega de novo após reload, scene reload, event bus duplicate ou reentrada de diálogo
Then estado/recompensa/flag não é aplicado duas vezes
And ChoiceHistory/GrantedRewardIds/GrantedFlagIds preservam a decisão.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Quest Log, notificação, diálogo, entrega, mapa, farm order ou evento de main quest
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- QuestStateSection absent requiring migration.
- MainProgression stored inside QuestStateSection.
- FonteAnya state stored as QuestFlag only.
- Reward applied twice due to missing GrantedRewardIds.
- ChoiceHistory lost on reload.
- Hidden quest tracked after load.
- Invalid QuestId deleted silently.
- Unity references persisted.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest State Save Load Runtime

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
- Definition/State/Step/Objective/Condition/Trigger/Reward/Flag separation:
- QuestState vs QuestFlag separation:
- QuestState vs MainProgression vs FonteAnya separation:
- Anti-softlock:
- Anti-spoiler:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-softlock:
- Idempotency/replay safety:
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
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Save Section Separation Matrix

| Section | Owns |
|---|---|
| QuestStateSection | generic quests, side, orders, festival, tutorial, hidden |
| MainProgressionSection | acts, fragments, Cindar, Memory Arc, level 100/101, endings |
| FonteAnyaSection | Fonte functions, LivingWater, respec, purification, final decision, corruption/state |
| InventorySection | items, quest items, protected items |
| WorldStateSection | day/time/weather/lunar/festival |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestDefinition|QuestState|QuestStep|Objective|Condition|Trigger|QuestEvent|Reward|QuestFlag|MainProgression|FonteAnya|ChoiceHistory|GrantedReward|Spoiler|Softlock|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de Quest Log, diálogo, entrega, reward, objective progress ou evento de main quest, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, save/load normalization and section validation logic is deterministic.
- Requires EditMode tests: YES for roundtrip/normalization/dedupe/section-separation tests.
- Requires PlayMode automated or final human scenario: NO by default; save/load tests only.
- Requires regression test: YES if fixing existing quest save/load bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no migration without approval; section separation preserved.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_state_save_load_runtime_execution_report.md.
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
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
