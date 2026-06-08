# SPEC — Quest Anti-Softlock Validation Future

> **Spec ID:** `03_spec_quest_anti_softlock_validation_future`  
> **Status:** A implementar  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P1 / Safety  
> **Type:** Runtime Validation / Quest / Anti-softlock  
> **Domain:** Quests / Main Quest Safety / Recovery / Validation  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere main progression, QuestState save/load, rewards, inventory key-item protection, NPC schedules, cave/death/corpse recovery ou calendar/lunar conditions.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/03_spec_quest_anti_softlock_validation_future_execution_report.md`  
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
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/specs/a_implementar/03_spec_quest_objective_event_contract_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_state_save_load_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_reward_application_idempotency_runtime.md`
> **Blocks:**  
  - main quest content/runtime;
  - inventory key item protection;
  - NPC schedule integration;
  - calendar/weather/lunar quest conditions;
  - cave/death/corpse quest objectives.
> **Scope:** definir e implementar validação anti-softlock para quests críticas, especialmente main quest, sem implementar todo o conteúdo final.  
> **Out of scope:** conteúdo final da main quest, repair/admin tool completo, quest item respawn final, NPC schedule final, cave boss gates finais.

---

# /speckit.specify

## 1. Contexto

O direction de Quest exige que toda quest crítica tenha plano anti-softlock. A main quest não pode ficar impossível por item vendido, NPC fora de schedule, clima/lua perdido, morte do jogador, inventário cheio, caverna resetada ou save/load no meio do evento.

Esta spec transforma essa regra em contrato validável.

---

## 2. Problema

Sem anti-softlock formal:

```text
main quest pode depender de item vendido;
quest pode exigir NPC em horário impossível;
evento lunar raro pode ser requisito opaco;
reward pode já ter sido aplicado;
step pode completar antes da quest iniciar;
save/load pode perder estado intermediário;
cave reset/death/corpse pode bloquear progressão.
```

---

## 3. Objetivo

Criar validação anti-softlock para quests críticas:

```text
critical quests declaram SoftlockPolicy;
main quest não expira por calendário;
critical objectives têm recovery/fallback;
quest items têm proteção ou reentrega;
rare weather/lunar requirements têm pista e fallback;
reward idempotency é obrigatório;
save/load mid-event é seguro ou bloqueado por policy.
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
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Quest crítica precisa de anti-softlock.
- Main quest não falha por tempo e não expira por calendário.
- Main quest não pode ficar impossível por item vendido, NPC indisponível, morte, clima/lua perdido ou inventário cheio.
- Soluções permitidas: reentrega, item protegido, NPC alternativo, fallback de calendário, trigger retroativo e normalização no load.
- Main quest usa quatro atos e fragmentos; progressão é híbrida entre fazenda, cidade, caverna, Fonte, NPCs e luas.

### Deferred / future from directions

- Repair/admin tool completo.
- Conteúdo final de main quest.
- Todos os fallbacks narrativos específicos.
- Boss gates finais.
- NPC schedule complete implementation.

### Explicitly not redefined here

- Quest runtime base.
- Inventory key item implementation.
- Calendar/weather/lunar runtime.
- Death/corpse runtime.
- MainProgression content.

## 4. Estado atual do repo

```text
Quest system está sendo especificado na WAVE 03.
Main quest progression é direction/refinement.
Anti-softlock ainda precisa virar validator/policy.
```

A confirmar localmente:

```text
Quest critical flag;
QuestDefinition data;
quest item/key item protection;
NPC schedule APIs;
calendar/weather/lunar condition APIs;
death/corpse/cave state APIs.
```

---

## 5. User stories

```text
Como jogador, não quero perder a main quest por vender item, morrer ou perder lua rara.
Como designer, quero marcar quest crítica e receber erro se faltar fallback.
Como sistema de quest, quero trigger retroativo quando objective já aconteceu.
Como save/load, quero normalizar estado quebrado quando possível.
```

---

## 6. Escopo

```text
SoftlockPolicy contract;
critical quest validation;
recovery/fallback metadata;
retroactive trigger checks;
key-item protection requirements;
calendar/weather/lunar rare-condition guardrails;
report de riscos.
```

---

## 7. Fora de escopo

```text
Implementar todos os fallbacks concretos;
repair/admin tool;
conteúdo final de main quest;
NPC schedule final;
quest item respawn final.
```

---

## 8. Regras de não duplicação

```text
Não usar QuestFlag solta como softlock policy.
Não transformar validator em gameplay runtime pesado.
Não bloquear side quests leves com regras de main quest.
Não tornar hidden quest obrigatória sem pista razoável.
```

---

## 9. Critérios de aceite

- Quest crítica tem SoftlockPolicy.
- Validator acusa critical quest sem fallback.
- Main quest não pode ser expirable por calendar.
- Rare lunar/weather condition precisa hint/fallback.
- Reward idempotency obrigatório.
- Report lista UNKNOWN com risco, não inventa cobertura.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Validation/QuestSoftlockPolicy.cs
Assets/_Game/Scripts/Quests/Validation/QuestAntiSoftlockValidator.cs
Assets/_Game/Tests/EditMode/Quests/QuestAntiSoftlockValidationTests.cs
```

---

## 11. Contratos

### Data

```text
QuestDefinition may declare Criticality and SoftlockPolicy.
Objective may declare recovery/fallback requirement.
```

### Runtime

```text
Validator runs in editor/dev/test; not heavy gameplay path.
```

### Save

```text
Normalização no load pode ser recomendada, mas schema changes require save spec.
```

### UI

```text
Quest Log may show hints for discovered recovery paths.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/03_spec_quest_anti_softlock_validation_future_execution_report.md
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar quest contracts/data.
2. Definir SoftlockPolicy.
3. Criar validator para critical quests.
4. Adicionar tests para main quest expirable, missing fallback, rare event condition.
5. Criar report.
```

---

## 15. Ordem segura

```text
Quest contracts -> State save/load -> Rewards -> Anti-softlock validation.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with main progression or quest contracts changes.
- Reason: validator depende dos contratos finais.

---

## 17. Impacto save/load

```text
Does this change save schema? NO by default.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: NO.
Changes existing events: NO.
Requires unsubscribe pattern: NO.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO.
Scenes/prefabs/assets: NO.
Human validation: NOT REQUIRED.
```

---

## 20. Riscos

```text
Risco: validator bloquear quests opcionais.
Mitigação: aplicar regras fortes só em Critical/Main.

Risco: false sense of safety.
Mitigação: report UNKNOWN and residual risks.

Risco: content final não existir.
Mitigação: future validation profile.
```

---

## 21. Rollback

```text
Remover policy/validator/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar quest contracts.
- [ ] T003 — Definir SoftlockPolicy.
- [ ] T004 — Implementar validator.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu os directions e refinements listados em Source Map Compliance? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | A execução auditou classes existentes antes de criar novas? | Comandos `rg` e achados principais no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão: reuse/harden/create. | BLOCKED se criar duplicata |
| Save/load | A spec altera ou depende de estado persistido? | Declaração explícita de schema/no schema. | PARTIAL |
| Eventos | A spec cria/usa eventos ou subscriptions? | Mapa de publishers/subscribers e unsubscribe policy. | PARTIAL |
| UI/Input | Há foco/modal/PlayMode relevante? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo |
| Testes | Há lógica determinística nova? | EditMode test ou justificativa NOT RUN. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/03_spec_quest_anti_softlock_validation_future_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Softlock|QuestDefinition|Critical|MainQuest|Fallback|Recovery|QuestItem|KeyItem|NPC|Weather|Lunar|Death|Corpse|Save" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a quest anti-softlock validation existe ou foi criado de forma mínima
When o usuário/sistema executa o fluxo principal desta spec
Then o estado visível/resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 3 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 4 — Final human validation deferred

```text
Given o fluxo exige interação visual ou PlayMode integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Main quest marcada como expirable por calendário.
- Critical quest dependente de item vendável/descartável.
- Critical quest dependente de NPC sem fallback de agenda/local.
- Objective dependente de weather/lunar raro sem hint ou alternativa.
- Quest step que exige estado de caverna que pode resetar sem recovery.
- Reward crítico sem idempotency key.
- Save/load mid-event deixando quest em estado impossível.
- Falha ao diferenciar ERROR para main quest e WARNING para side quest opcional.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

```md
# Execution Report — Quest Anti-Softlock Validation Future

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
- Edge cases:
- Negative cases:

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
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação criar conflito com 01Q, input focus, save ownership ou registry.
6. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Softlock Policy Matrix

| Criticality | Expiry permitido | Fallback obrigatório | Exemplo |
|---|---|---|---|
| MainCritical | NO | YES | Fragmento/Fonte/MainProgression. |
| CriticalSide | CONDITIONAL | YES | Quest pessoal que desbloqueia serviço. |
| TimedOptional | YES | SHOULD | Festival/order opcional. |
| RepeatableOrder | YES | NO, mas precisa repeat policy | Encomenda agrícola. |
| HiddenLore | CONDITIONAL | SHOULD | Rumor/observação. |

## 23H. Recovery Types

Recovery permitido:

```text
RegrantQuestItem
  Reentrega item crítico perdido.

AlternateNpcOrLocation
  Permite completar com outro NPC/local se schedule falhar.

RetroactiveTrigger
  Completa objective se evento já ocorreu antes da quest ficar ativa.

CalendarFallback
  Reagenda evento raro ou permite alternativa após descoberta.

StateNormalizationOnLoad
  Corrige estado inconsistente detectável no load.

ManualRepairFuture
  Apenas dev/admin, nunca depender disso para fluxo normal.
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Quest|Objective|Condition|Trigger|Reward|Flag|Softlock|Debug|InputFocus|Modal|HUD|Notification" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário integrado, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if validator/policy logic is created.
- Requires EditMode tests: YES for validator logic.
- Requires PlayMode automated or final human scenario: NO, validator-only.
- Requires regression test: YES if fixing known softlock bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode validation tests PASS or NOT RUN justified; main quest cannot be expirable by calendar; report created.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/03_spec_quest_anti_softlock_validation_future_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não usar UI como fonte de verdade.
Não revelar spoilers antes de discovery/visibility policy.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
