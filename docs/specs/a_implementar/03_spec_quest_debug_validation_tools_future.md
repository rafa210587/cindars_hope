# SPEC — Quest Debug Validation Tools Future

> **Spec ID:** `03_spec_quest_debug_validation_tools_future`  
> **Status:** A implementar  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P2 / Future Dev Tool  
> **Type:** Tooling / Runtime Debug / Validation / Quest  
> **Domain:** Quests / Debug / Dev Validation / Diagnostics  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere quest runtime core, QuestState save/load, reward idempotency, QuestFlag registry, Quest Log projection ou debug HUD global.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/03_spec_quest_debug_validation_tools_future_execution_report.md`  
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
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/specs/a_implementar/03_spec_quest_objective_event_contract_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_condition_trigger_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_reward_application_idempotency_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_flags_registry_runtime.md`
> **Blocks:**  
  - future quest authoring validators;
  - quest anti-softlock validation;
  - debug HUD separation;
  - developer repair/admin tooling future.
> **Scope:** criar ou especificar ferramenta dev mínima para inspecionar quests, objectives, conditions, rewards e flags sem virar UI final.  
> **Out of scope:** debug HUD global final, editor custom avançado, admin repair completo, UI de jogador, cheats de produção, localization tool final.

---

# /speckit.specify

## 1. Contexto

O direction de Quests exige validação dev futura para listar quests ativas, objectives, conditions falhando, rewards aplicados, flags setadas e referências quebradas. Essa ferramenta não é UI final.

O direction de UI/UX separa Debug HUD de HUD final: debug pode mostrar IDs, state machines e quest flags, mas HUD final nunca deve depender disso para comunicar regra ao jogador.

Esta spec cria o contrato de debug/validation para o sistema de quests.

---

## 2. Problema

Sem tooling de validação:

```text
quest pode ficar bloqueada sem motivo visível;
condition pode falhar silenciosamente;
reward pode aplicar duas vezes;
QuestFlag pode ser usada como state indevido;
QuestDefinition pode referenciar ID quebrado;
spoiler tier pode estar errado;
softlock pode só aparecer em PlayMode final.
```

---

## 3. Objetivo

Criar uma camada de diagnóstico dev para quests:

```text
listar quests ativas;
listar objectives ativos;
inspecionar conditions falhando;
inspecionar rewards já aplicados;
inspecionar flags;
validar referências quebradas;
validar objective sem target;
validar reward sem target;
validar spoiler tier;
gerar report de problemas.
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
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Debug/dev validation deve listar quests, objectives, conditions, rewards, flags e referências quebradas.
- Debug HUD pode mostrar IDs, state machines, quest flags e save/load state.
- Debug não é UI final e não pode ser necessário para o jogador entender regra.
- Quest validation precisa detectar objective sem target, reward sem target e spoiler tier inválido.

### Deferred / future from directions

- Admin/debug repair future.
- Ferramenta visual completa de authoring.
- Debug HUD global consolidado.
- Cheats/dev console extensivo.
- Localization/text-key validation final.

### Explicitly not redefined here

- Quest runtime core.
- Quest Log UI final.
- Save schema.
- Reward behavior.
- QuestFlag semantics.

## 4. Estado atual do repo

```text
Quest runtime está sendo especificado em WAVE 03.
Debug HUD vs HUD final é definido pelo direction de UI.
Esta spec deve ser future/dev tooling e não requisito de jogador.
```

A confirmar localmente:

```text
existência de debug HUD;
validators existentes;
QuestDefinitionSO/Quest assets;
QuestRuntime APIs;
editor validation folders.
```

---

## 5. User stories

```text
Como designer técnico, quero ver por que uma quest não avança.
Como dev, quero detectar target quebrado antes do PlayMode final.
Como QA, quero relatório de conditions/rewards/flags.
Como jogador, não quero que debug seja necessário para entender o jogo.
```

---

## 6. Escopo

```text
debug/validation service ou editor validator;
read-only diagnostics;
quest reference checks;
condition/reward/flag visibility;
report de issues;
EditMode tests para validators quando possível.
```

---

## 7. Fora de escopo

```text
UI final;
debug console completo;
repair/admin commands;
runtime cheats;
localization editor;
quest authoring visual tool.
```

---

## 8. Regras de não duplicação

```text
Não criar QuestState paralelo.
Não alterar quest runtime para servir debug.
Não misturar Debug HUD com HUD final.
Não criar repair tool que muda save sem spec própria.
```

---

## 9. Critérios de aceite

- Validator detecta references quebradas quando dados existem.
- Debug view/service é claramente dev-only.
- Report lista active quests/objectives/flags/rewards.
- Não há dependência de debug para UI final.
- Execution report lista gaps e future tooling.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/Debug/QuestDebugSnapshot.cs
Assets/_Game/Scripts/Quests/Debug/QuestDebugService.cs
Assets/_Game/Scripts/Quests/Validation/QuestDefinitionValidator.cs
Assets/_Game/Scripts/Editor/Validation/ValidateQuestDefinitions.cs
Assets/_Game/Tests/EditMode/Quests/QuestDebugValidationTests.cs
```

---

## 11. Contratos

### Data

```text
Debug snapshot é derivado de QuestState/definitions.
Não é salvo como estado de gameplay.
```

### Runtime

```text
Debug service é read-only por padrão.
Qualquer repair/admin write fica future.
```

### UI

```text
Debug output não é HUD final.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/03_spec_quest_debug_validation_tools_future_execution_report.md
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
1. Auditar runtime/definitions existentes.
2. Criar diagnostics read-only se seguro.
3. Criar validator de references/targets/rewards/flags.
4. Criar tests de validator.
5. Criar execution report.
```

---

## 15. Ordem segura

```text
Quest contracts -> State save/load -> Rewards/Flags -> Debug validation.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with quest core changes or debug HUD global.
- Reason: validators dependem de contratos finais.

---

## 17. Impacto save/load

```text
Does this change save schema? NO.
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
Changes UI: NO final.
Debug view optional/dev-only.
Scenes/prefabs/assets: NO.
Human validation: NOT REQUIRED.
```

---

## 20. Riscos

```text
Risco: debug virar dependência de gameplay.
Mitigação: dev-only/read-only.

Risco: validator falso positivo bloquear progresso.
Mitigação: severity ERROR/WARNING/INFO.
```

---

## 21. Rollback

```text
Remover debug service/validators/tests/report.
Sem alteração de save/runtime principal.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar quest contracts/runtime.
- [ ] T003 — Criar diagnostics read-only se seguro.
- [ ] T004 — Criar validator de definitions/references.
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
| Report | Execution report foi criado? | `docs/validation/03_spec_quest_debug_validation_tools_future_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "QuestDebug|QuestValidator|QuestDefinition|QuestState|Objective|Condition|Reward|QuestFlag|DebugHud|Validation" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a quest debug/validation existe ou foi criado de forma mínima
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

- QuestDefinition sem ID estável.
- Objective referenciando target inexistente.
- Reward sem ID ou sem idempotency key.
- QuestFlag usada como substituto integral de QuestState.
- Condition impossível por falta de source/evento.
- SpoilerTier ausente em quest oculta.
- Debug tool alterando estado de gameplay sem spec de repair.
- Validator marcando WARNING como ERROR sem justificar severidade.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

```md
# Execution Report — Quest Debug Validation Tools Future

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


## 23G. Quest Debug Snapshot Requirements

O snapshot dev-only deve, quando possível, separar:

| Campo | Obrigatório | Observação |
|---|---|---|
| QuestId | YES | Stable ID da quest. |
| QuestState | YES | Inactive/Active/Completed/Failed/Expired/Hidden. |
| CurrentObjectives | YES | IDs e progresso. |
| BlockingConditions | YES | Conditions falsas com motivo. |
| GrantedRewardIds | YES | Para validar idempotência. |
| GrantedFlagIds | YES | Para validar side effects. |
| VisibilityState | YES | Evitar spoiler. |
| SourceDefinitionPath | SHOULD | Para apontar asset/definition quebrada. |

Regra: snapshot é diagnóstico derivado. Ele não deve ser salvo em `GameSaveData` e não deve ser usado pelo jogador.

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

- Changed deterministic logic: YES, se validators/debug snapshot logic forem criados.
- Requires EditMode tests: YES para deterministic validators quando harness estiver disponível.
- Requires PlayMode automated or final human scenario: NO.
- Requires regression test: YES se corrigir bug conhecido de validation; caso contrário NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS se C# mudou; EditMode validator tests PASS ou NOT RUN justificado; debug claramente dev-only; report criado.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/03_spec_quest_debug_validation_tools_future_execution_report.md.
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
