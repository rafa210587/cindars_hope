# SPEC — Final Human Validation By Wave Consolidation Docs

> **Spec ID:** `12_spec_final_human_validation_by_wave_consolidation_docs`  
> **Status:** A implementar  
> **Wave:** WAVE 12 — Closeout / Registry / Validation Documentation  
> **Priority:** P0  
> **Type:** Docs-only / Final Human Validation / Wave Checklist  
> **Domain:** docs/validation / final manual Unity scenario checklist by wave  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_12_DOCS_CLOSEOUT  
> **Can run with:** registry reconciliation if no same file is edited.  
> **Must not run with:** qualquer spec runtime, qualquer alteração em Assets/Packages/ProjectSettings, qualquer declaração de PlayMode PASS real, qualquer alteração de status para ACCEPTED sem evidência.  
> **Repo lock scope:** `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`, `docs/validation/12_spec_final_human_validation_by_wave_consolidation_docs_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
  - `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md`
  - `docs/specs/SPEC_EXECUTION_ORDER.md`
  - `docs/specs/SPEC_SOURCE_OF_TRUTH.md`
  - `docs/specs/README.md`
  - `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`
  - `.claude/rules/testing-quality-gate.md`
  - `docs/project/CURRENT_STATE.md`
  - `PROJECT_LOG.md`
> **Blocks:**  
  - human final Unity validation;
  - wave acceptance checklist;
  - execution prompts for Codex/Claude;
  - release readiness review.
> **Scope:** consolidar passos humanos finais por wave para validação no Unity depois que as specs forem implementadas, sem pedir validação spec a spec.  
> **Out of scope:** rodar Unity, declarar PASS real, alterar runtime, aceitar specs, mover implementados.

---

# /speckit.specify

## 1. Contexto

A regra consolidada é não pedir validação humana spec a spec. Specs runtime podem documentar cenário final, mas a validação real fica deferida para o final do lote/wave. O arquivo `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md` deve concentrar os passos humanos finais no Unity.

Esta spec consolida esse documento com base nas specs geradas.

---

## 2. Problema

Sem checklist final por wave:

```text
cada spec pode pedir teste humano isolado;
validação humana pode ser esquecida;
PlayMode pode ser declarado PASS sem execução real;
cenários visuais podem ficar espalhados em execution reports;
humano não sabe o que testar no Unity no final;
ACCEPTED pode ser aplicado sem comportamento validado;
quality gate pode ser interpretado como compile suficiente.
```

---

## 3. Objetivo

Atualizar docs-only:

```text
FINAL_HUMAN_VALIDATION_BY_WAVE;
checklists por wave;
pré-condições;
ordem de validação;
evidência esperada;
status permitido antes/depois;
falhas bloqueantes;
regra de screenshots/logs/observações;
regras de não validar spec a spec.
```

---

## 4. Regras de design

```text
Humano testa no final do lote/wave, não spec a spec.
Cenário humano documentado não é PASS real.
DEFERRED_TO_FINAL_HUMAN_VALIDATION permanece até execução real.
Compile/build não é ACCEPTED para runtime.
UI/cena/input exige PlayMode automatizado ou cenário humano final.
Bugfix exige regressão ou risco residual.
```

---

## 5. User stories / engineering stories

```text
Como humano, quero uma lista por wave do que testar no Unity.
Como executor, quero registrar cenário final sem interromper o fluxo.
Como reviewer, quero saber o que bloqueia ACCEPTED.
Como projeto, quero evitar falso positivo por compile.
Como roadmap, quero consolidar validação final de specs geradas.
```

---

## 6. Escopo

Inclui:

```text
checklist final por wave;
ordem sugerida de teste no Unity;
evidência mínima por tipo de fluxo;
status rules;
failure report format;
não declarar PASS sem execução real;
link com reports.
```

Não inclui:

```text
rodar Unity;
gerar screenshots reais;
aceitar specs;
mover specs para implementados;
alterar runtime;
criar PlayMode tests.
```

---

# /speckit.plan

## 7. Arquivos permitidos

```text
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
docs/validation/12_spec_final_human_validation_by_wave_consolidation_docs_execution_report.md
```

Opcional apenas se já existir padrão claro:

```text
docs/validation/FINAL_HUMAN_VALIDATION_INDEX.md
```

## 8. Arquivos proibidos

```text
Assets/**
Packages/**
ProjectSettings/**
docs/specs/implementados/**
docs/refinements/implementados/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
PROJECT_LOG.md
docs/project/CURRENT_STATE.md
```

---

## 9. Estratégia

```text
1. Ler specs em a_implementar e reports existentes.
2. Extrair de cada wave os cenários humanos finais já declarados.
3. Consolidar por wave, não por spec isolada.
4. Definir ordem de teste no Unity.
5. Definir evidência mínima: observação, log, screenshot opcional, vídeo opcional, erro encontrado.
6. Definir bloqueadores por wave.
7. Registrar que PASS humano real só ocorre após execução manual.
8. Rodar docs validation.
9. Criar execution report.
```

---

## 10. Estrutura obrigatória do checklist final

```md
# Final Human Validation By Wave

## Status policy
- ...

## Before starting
- branch:
- build/compile:
- required reports:
- 01Q gate:
- known blockers:

## WAVE 01
### Goal
### Preconditions
### Steps
### Expected result
### Evidence
### Blockers

## WAVE 02
...
```

---

## 11. Waves mínimas

```text
WAVE 01 — Core / Testing Quality Gate / IDs / Events / Save
WAVE 02 — Time / Calendar / Weather / Lunar
WAVE 03 — Quest / Objective / Event / Rewards
WAVE 04/11 — UI / Input / HUD / Menus
WAVE 05 — Inventory / Items / Equipment / Hotbar
WAVE 06/07 — Farm / Economy / Crafting / Orders
WAVE 08 — City / NPC / Dialogue / Services
WAVE 09 — Player / Skills / Magic
WAVE 10 — Cave / Combat / Enemies / Death
WAVE 11 — Bestiary / Knowledge
WAVE 10/Endgame — Main Progression / Fonte / Level 100/101
```

Se o repo usa prefixos históricos diferentes, preservar nomes reais e explicar.

---

## 12. Exemplo de evidência

```text
Observation:
  O que foi feito e resultado.

Screenshot/video:
  Opcional, recomendado para UI, cave, Fonte, final choice.

Logs:
  Unity console sem erros novos relevantes.

Issue:
  Se falhou, registrar spec/wave, passo, resultado esperado, resultado obtido, log e severidade.
```

---

## 13. Status rules

```text
Antes da execução humana real:
  DEFERRED_TO_FINAL_HUMAN_VALIDATION.

Depois da execução humana real com PASS:
  PLAYMODE_VALIDATED ou ACCEPTED se todos os outros gates também passaram.

Se falhar:
  PARTIAL ou BLOCKED conforme severidade.

Se não foi testado:
  não declarar PASS.
```

---

## 14. Criteria

```text
FINAL_HUMAN_VALIDATION_BY_WAVE consolidado.
Checklists por wave existem.
Passos são executáveis por humano no Unity.
Status policy impede PASS falso.
Evidência esperada clara.
Falhas bloqueantes claras.
Nenhum runtime alterado.
Execution report criado.
```

---

# /speckit.tasks

## 15. Tasks

- [ ] T001 — Ler fontes canônicas.
- [ ] T002 — Enumerar specs com final human scenarios.
- [ ] T003 — Consolidar por wave.
- [ ] T004 — Atualizar FINAL_HUMAN_VALIDATION_BY_WAVE.md.
- [ ] T005 — Rodar docs validation.
- [ ] T006 — Criar execution report.

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
- docs/specs/SPEC_REGISTRY_IMPLEMENTED.md
- docs/specs/SPEC_EXECUTION_ORDER.md
- docs/specs/SPEC_SOURCE_OF_TRUTH.md
- docs/specs/README.md
- docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
- .claude/rules/testing-quality-gate.md
- docs/project/CURRENT_STATE.md
- PROJECT_LOG.md

### Domain directions read

- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md

### Required interpretation

```text
Esta spec é docs-only.
Ela não autoriza alteração de runtime, Unity, Assets, Packages ou ProjectSettings.
Ela serve para reconciliar os documentos de controle após a geração das specs implementáveis.
Quando houver conflito entre roadmap, registries, source map e specs concretas, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Preserva a regra de que roadmap organiza sequência, mas directions e SPEC_SOURCE_MAP continuam fontes canônicas.
- Preserva a regra de que SPEC_EXECUTION_ORDER não deve ser atualizado como se specs ainda não executadas já estivessem implementadas.
- Preserva a regra de Testing Quality Gate obrigatório para specs runtime.
- Preserva a regra de não pedir validação humana spec a spec.
- Preserva a regra de que validação humana real fica no final do lote/wave.
- Preserva a regra de docs-only não alterar runtime nem status de implementação.

### Deferred / future from directions

- Execução de runtime.
- Aplicação de patches de código.
- Movimento de specs para `implementados/`.
- Alteração de cenas/prefabs/assets.
- Validação PlayMode real.
- Atualização de CURRENT_STATE.md/PROJECT_LOG.md como se runtime estivesse implementado.

### Explicitly not redefined here

- Regras canônicas de gameplay.
- Contratos de Save/Load runtime.
- Contratos de Quest/Objective/Event runtime.
- Contratos de UI runtime.
- Ordem final de execução real se o repo mostrar bloqueador posterior.
- Conteúdo futuro/pós-MVP.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu roadmap, registries, source map, wave protocol e validation matrix? | Lista no execution report. | PARTIAL |
| Estado real do repo | Arquivos reais em `docs/specs/a_implementar/` foram enumerados? | Lista/contagem ou comando `find/ls`. | PARTIAL |
| Não duplicação | Specs duplicadas/obsoletas/future foram identificadas? | Tabela de ação: keep/rename/merge/future/archive-candidate. | PARTIAL |
| Sem runtime | Nenhum arquivo em Assets/Packages/ProjectSettings foi alterado? | `Forbidden files touched: NO`. | BLOCKED se violar |
| Registry seguro | Registry reflete specs criadas sem marcar implementado? | Diff/review. | PARTIAL |
| Execution order seguro | SPEC_EXECUTION_ORDER não trata specs novas como executadas. | Checklist explícito. | BLOCKED se violar |
| Quality gate | Specs runtime continuam exigindo Testing Quality Gate. | Checklist/amostragem. | PARTIAL |
| Validação humana final | Cenários finais ficam consolidados por wave/lote. | FINAL_HUMAN_VALIDATION atualizado se aprovado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/12_spec_final_human_validation_by_wave_consolidation_docs_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo docs-only, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "FINAL_HUMAN_VALIDATION|DEFERRED_TO_FINAL_HUMAN_VALIDATION|PLAYMODE_VALIDATED|ACCEPTED|human scenario|PlayMode|Unity" docs/specs docs/validation .claude/rules .claude/skills CLAUDE.md
rg -n "ACCEPTED|IMPLEMENTED|implementados|SPEC_CREATED|READY_FOR_IMPLEMENTATION|DEFERRED_TO_FINAL_HUMAN_VALIDATION|BUILD_VALIDATED|PARTIAL|BLOCKED|Testing Quality Gate|FINAL_HUMAN_VALIDATION" docs/specs docs/validation .claude/rules .claude/skills CLAUDE.md
rg -n "Assets/|Packages/|ProjectSettings/" docs/specs/a_implementar docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
```

Se ambiente local permitir:

```bash
find docs/specs/a_implementar -maxdepth 1 -type f -name "*.md" | sort
```

No PowerShell:

```powershell
Get-ChildItem docs/specs/a_implementar -Filter *.md | Sort-Object Name | Select-Object -ExpandProperty Name
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given as specs geradas existem em docs/specs/a_implementar/
When o executor faz a reconciliação docs-only
Then registries/checklists/validation docs passam a refletir as specs criadas
And nenhuma spec é marcada como implementada
And nenhum arquivo Unity/runtime é alterado
And o execution report registra evidência.
```

### Scenario 2 — Duplicate or obsolete spec

```text
Given uma spec antiga/duplicada/obsoleta permanece em a_implementar
When o executor encontra duplicidade
Then ele não deleta nem arquiva automaticamente
And marca como archive-candidate/merge-candidate em relatório ou patch proposto
And solicita decisão humana antes de remoção.
```

### Scenario 3 — Quality gate enforcement

```text
Given uma spec runtime/gameplay nova existe
When o executor audita estrutura
Then ela contém Testing Quality Gate
And se faltar, a spec é marcada como STRUCTURAL_FIX_REQUIRED, não READY_FOR_IMPLEMENTATION.
```

### Scenario 4 — Safe execution status

```text
Given uma spec foi apenas criada, não executada
When registry/roadmap for atualizado
Then status máximo documental é SPEC_CREATED ou READY_FOR_IMPLEMENTATION
And nunca IMPLEMENTED, ACCEPTED, UNITY_VALIDATED ou PLAYMODE_VALIDATED.
```

### Scenario 5 — Final validation deferred

```text
Given uma spec exige PlayMode/cenário humano final
When a validação humana ainda não ocorreu
Then status humano permanece DEFERRED_TO_FINAL_HUMAN_VALIDATION
And não é declarado PASS humano antes da execução real.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Checklist humano continua espalhado por spec.
- Final human scenario documentado vira PASS falso.
- Wave não tem passos suficientes para humano executar.
- UI/Input/Cave/Fonte sem cenário final.
- Falha visual não tem formato de registro.
- ACCEPTED aplicado sem PlayMode/final human evidence.
- Humano é solicitado a validar spec a spec.
- Docs-only patch altera runtime por acidente.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Final Human Validation By Wave Consolidation Docs

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
- Specs found:
- Duplicates:
- Archive candidates:
- Future candidates:
- Conflicts:

## Docs-only safety
- Assets changed: YES/NO
- Packages changed: YES/NO
- ProjectSettings changed: YES/NO
- Runtime/code changed: YES/NO
- Specs moved to implementados: YES/NO
- SPEC_EXECUTION_ORDER changed as implemented: YES/NO

## Reconciliation decisions
- Keep:
- Rename proposed:
- Merge proposed:
- Future proposed:
- Archive proposed, not executed:
- Registry updates:

## Validation
- Docs validation:
- Structural spec audit:
- Links/paths consistency:
- Testing Quality Gate presence:
- Final human validation entries:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next actions
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A alteração tocar Assets/, Packages/ ou ProjectSettings/.
2. A alteração mover spec para implementados/.
3. A alteração marcar specs recém-criadas como IMPLEMENTED, ACCEPTED, UNITY_VALIDATED ou PLAYMODE_VALIDATED.
4. A alteração atualizar SPEC_EXECUTION_ORDER como se specs ainda não executadas já estivessem concluídas.
5. A alteração deletar/arquivar docs sem decisão humana explícita.
6. A alteração remover o bloqueio da 01Q antes de implementação ou exceção humana explícita.
7. A alteração enfraquecer Testing Quality Gate.
8. A alteração declarar validação humana final PASS sem execução real.
9. A contagem de specs não bater com arquivos reais e o executor não conseguir reconciliar.
10. Houver conflito entre roadmap, registry e source map que não possa ser resolvido docs-only.
```

---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Testing Quality Gate|DEFERRED_TO_FINAL_HUMAN_VALIDATION|SPEC_CREATED|READY_FOR_IMPLEMENTATION|IMPLEMENTED|ACCEPTED|implementados|Assets/|Packages/|ProjectSettings/" docs/specs docs/validation .claude/rules .claude/skills CLAUDE.md
```

Estrutural:

```text
Enumerar specs em docs/specs/a_implementar/.
Checar presença de Testing Quality Gate nas specs runtime.
Checar se nenhuma spec nova foi movida para implementados/.
Checar se nenhum status de execução foi promovido sem evidence.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: NO
- Requires EditMode tests: NO
- Requires PlayMode automated or final human scenario: NO
- Requires regression test: NO
- Human validation timing: NOT REQUIRED
- Minimum validation evidence for ACCEPTED: docs validation PASS; links/paths consistent; no runtime files changed; execution report confirms docs-only safety.

---

## 27. Definition of Done

```text
Spec executada como docs-only.
Execution report criado em docs/validation/12_spec_final_human_validation_by_wave_consolidation_docs_execution_report.md.
Nenhum runtime/code/Unity file alterado.
Nenhuma spec movida para implementados.
Nenhum status de execução promovido indevidamente.
Docs validation PASS ou NOT RUN com motivo aceitável.
```

---

## 28. Anti-regressão

```text
Não alterar runtime.
Não arquivar/deletar sem decisão humana.
Não mover specs para implementados.
Não mudar SPEC_EXECUTION_ORDER como concluído.
Não remover Quality Gate.
Não pedir human test por spec.
Não declarar PlayMode/final human PASS antes da execução real.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex como **docs-only**.

Se qualquer alteração precisar sair de docs/specs, docs/validation, .claude/rules ou .claude/skills, parar e pedir decisão humana.
