# SPEC — Spec Registry Roadmap Reconciliation Closeout Docs

> **Spec ID:** `12_spec_registry_roadmap_reconciliation_closeout_docs`  
> **Status:** A implementar  
> **Wave:** WAVE 12 — Closeout / Registry / Validation Documentation  
> **Priority:** P0  
> **Type:** Docs-only / Registry / Roadmap / Spec Generation Closeout  
> **Domain:** .specs registries / roadmap / README / source-of-truth references  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_12_DOCS_CLOSEOUT  
> **Can run with:** final human validation consolidation if it edits only docs/validation and no shared registry file.  
> **Must not run with:** qualquer spec runtime, qualquer alteração em Assets/Packages/ProjectSettings, qualquer atualização de SPEC_EXECUTION_ORDER como executado, qualquer arquivamento/deleção sem aprovação humana.  
> **Repo lock scope:** `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md`, `.specs/SPEC_GENERATION_ROADMAP_MASTER.md`, `.specs/README.md`, `.specs/SPEC_SOURCE_OF_TRUTH.md`, `docs/validation/12_spec_registry_roadmap_reconciliation_closeout_docs_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `.specs/SPEC_GENERATION_ROADMAP_MASTER.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_REGISTRY_TO_IMPLEMENT.md`
  - `.specs/SPEC_REGISTRY_IMPLEMENTED.md`
  - `.specs/SPEC_EXECUTION_ORDER.md`
  - `.specs/SPEC_SOURCE_OF_TRUTH.md`
  - `.specs/README.md`
  - `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`
  - `.claude/rules/testing-quality-gate.md`
  - `docs/project/CURRENT_STATE.md`
  - `PROJECT_LOG.md`
> **Blocks:**  
  - safe execution batch planning;
  - registry patch final;
  - archive candidate review;
  - Codex/Claude execution prompts.
> **Scope:** reconciliar docs de controle com specs realmente geradas, mantendo tudo como SPEC_CREATED/READY_FOR_IMPLEMENTATION e sem marcar como implementado.  
> **Out of scope:** executar runtime, mover para implementados, arquivar/deletar, alterar código Unity, alterar SPEC_EXECUTION_ORDER como concluído.

---

# /speckit.specify

## 1. Contexto

Após a geração em lotes, o repo terá várias specs novas em `.specs/a_implementar/`. Os registries e roadmap precisam refletir os arquivos concretos para evitar execução errada, duplicação e perda de rastreabilidade.

O roadmap diz que deve ser usado para decidir o próximo lote, que cada spec gerada deve ler source map/directions, e que depois que uma spec existir ela deve ser registrada nos registries corretos. Ele também deixa claro que o roadmap não substitui `SPEC_EXECUTION_ORDER`, registries, implementation status ou PROJECT_LOG.

---

## 2. Problema

Sem reconciliação:

```text
spec existe mas não está no registry;
registry aponta spec antiga/obsoleta;
roadmap mostra item future que virou spec;
spec docs-only pode ser confundida com runtime;
SPEC_EXECUTION_ORDER pode ser atualizado cedo demais;
Claude/Codex pode executar spec duplicada;
humano pode não saber quais zips já foram aplicados;
status pode parecer implementado quando só foi criado.
```

---

## 3. Objetivo

Criar patch docs-only seguro para:

```text
SPEC_REGISTRY_TO_IMPLEMENT;
SPEC_GENERATION_ROADMAP_MASTER;
.specs/README;
SPEC_SOURCE_OF_TRUTH se necessário;
lista de archive candidates sem deletar;
lista de future candidates;
lista de generated specs por wave;
observação de que SPEC_EXECUTION_ORDER só muda quando execução for planejada/aprovada.
```

---

## 4. Regras de design

```text
Spec criada != spec implementada.
Spec criada não pode virar ACCEPTED sem evidência compatível com a matriz de validação.
Registry deve refletir arquivos reais em a_implementar.
Status não deve passar de SPEC_CREATED/READY_FOR_IMPLEMENTATION.
Não mover spec para implementados.
Não deletar/arquivar sem aprovação humana.
SPEC_EXECUTION_ORDER não deve fingir execução.
Roadmap continua macro; registry é lista concreta.
```

---

## 5. User stories / engineering stories

```text
Como executor, quero saber quais specs existem e podem ser implementadas.
Como humano, quero ver duplicatas/future/archive candidates antes de deletar.
Como Claude/Codex, quero evitar executar spec obsoleta.
Como roadmap, quero refletir que geração núcleo chegou ao fechamento.
Como registry, quero mapear cada spec por wave, status e dependências.
```

---

## 6. Escopo

Inclui:

```text
auditar arquivos em .specs/a_implementar;
atualizar registry to implement;
marcar status SPEC_CREATED/READY_FOR_IMPLEMENTATION;
atualizar README de specs;
atualizar roadmap macro com contagem real e notas;
propor archive candidates sem mover/deletar;
criar execution report.
```

Não inclui:

```text
executar specs;
alterar runtime;
mover para implementados;
arquivar/deletar arquivos;
atualizar PROJECT_LOG/CURRENT_STATE como runtime implementado;
criar PR final;
aplicar patches automaticamente fora de docs.
```

---

# /speckit.plan

## 7. Arquivos permitidos

```text
.specs/SPEC_REGISTRY_TO_IMPLEMENT.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/README.md
.specs/SPEC_SOURCE_OF_TRUTH.md
docs/validation/12_spec_registry_roadmap_reconciliation_closeout_docs_execution_report.md
```

Opcional com cuidado, somente se já existir padrão claro:

```text
.specs/SPEC_REGISTRY_GENERATED.md
.specs/SPEC_GENERATION_CLOSEOUT_REPORT.md
```

## 8. Arquivos proibidos

```text
Assets/**
Packages/**
ProjectSettings/**
.specs/implementados/**
docs/refinements/implementados/**
.specs/SPEC_EXECUTION_ORDER.md
PROJECT_LOG.md
docs/project/CURRENT_STATE.md
```

`SPEC_EXECUTION_ORDER.md` fica proibido nesta spec porque a ordem de execução real será decidida depois, com base em dependências, locks e 01Q.

---

## 9. Estratégia

```text
1. Enumerar arquivos reais em .specs/a_implementar.
2. Agrupar por wave/prefixo.
3. Comparar com SPEC_REGISTRY_TO_IMPLEMENT.
4. Comparar com roadmap master.
5. Marcar duplicatas/obsoletas/future candidates em relatório.
6. Atualizar registry/README/roadmap com status de geração, sem execução.
7. Rodar docs validation.
8. Criar execution report.
```

---

## 10. Dados mínimos por spec no registry

```text
Spec file
Spec ID
Wave
Domain
Status: SPEC_CREATED / READY_FOR_IMPLEMENTATION / STRUCTURAL_FIX_REQUIRED / FUTURE / ARCHIVE_CANDIDATE
Parallelizable
Depends on
Blocks
Testing Quality Gate present: YES/NO/N/A docs-only
Human validation deferred: YES/NO/N/A
Notes
```

---

## 11. Critérios de agrupamento

```text
01_* -> WAVE 01 Core / Quality Gate / IDs / Save / Events
02_* -> Time / Calendar / Weather / Lunar / World
03_* or 09_* quest core -> Quest / Objective / Event
04_* / 11_* -> UI / UX
05_* -> Inventory / Equipment / Items
06_* -> Economy / Loot / Crafting
07_* -> Farm
08_* -> City / NPC / Dialogue / Services
09_* -> Quest adapters / progression hooks
10_* -> Main Progression / Fonte / Endgame
12_* -> Docs-only closeout
```

Se prefixo histórico divergir da wave macro, registrar como `Historical prefix` e não renomear sem decisão humana.

---

## 12. Archive candidates

A spec pode ser marcada como archive candidate quando:

```text
foi substituída por spec expandida;
é guia/harness/rule e não spec implementável;
foi consolidada em spec maior;
está duplicada com nome antigo;
depende de future system explicitamente não-MVP.
```

Não deletar. Não mover. Apenas reportar.

---

## 13. Criteria

```text
Registry reflete arquivos reais.
Roadmap contagem/nota de fechamento atualizada.
README explica como aplicar lotes e como não executar em massa sem 01Q.
Archive/future candidates listados, não movidos.
SPEC_EXECUTION_ORDER não alterado.
Docs validation PASS ou NOT RUN aceitável.
Execution report criado.
```

---

# /speckit.tasks

## 14. Tasks

- [ ] T001 — Ler fontes canônicas.
- [ ] T002 — Enumerar `.specs/a_implementar/*.md`.
- [ ] T003 — Comparar com registry e roadmap.
- [ ] T004 — Classificar keep/future/archive-candidate/structural-fix.
- [ ] T005 — Atualizar registry/README/roadmap docs-only.
- [ ] T006 — Rodar docs validation.
- [ ] T007 — Criar execution report.

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- .specs/SPEC_GENERATION_ROADMAP_MASTER.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_REGISTRY_TO_IMPLEMENT.md
- .specs/SPEC_REGISTRY_IMPLEMENTED.md
- .specs/SPEC_EXECUTION_ORDER.md
- .specs/SPEC_SOURCE_OF_TRUTH.md
- .specs/README.md
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
| Estado real do repo | Arquivos reais em `.specs/a_implementar/` foram enumerados? | Lista/contagem ou comando `find/ls`. | PARTIAL |
| Não duplicação | Specs duplicadas/obsoletas/future foram identificadas? | Tabela de ação: keep/rename/merge/future/archive-candidate. | PARTIAL |
| Sem runtime | Nenhum arquivo em Assets/Packages/ProjectSettings foi alterado? | `Forbidden files touched: NO`. | BLOCKED se violar |
| Registry seguro | Registry reflete specs criadas sem marcar implementado? | Diff/review. | PARTIAL |
| Execution order seguro | SPEC_EXECUTION_ORDER não trata specs novas como executadas. | Checklist explícito. | BLOCKED se violar |
| Quality gate | Specs runtime continuam exigindo Testing Quality Gate. | Checklist/amostragem. | PARTIAL |
| Validação humana final | Cenários finais ficam consolidados por wave/lote. | FINAL_HUMAN_VALIDATION atualizado se aprovado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/12_spec_registry_roadmap_reconciliation_closeout_docs_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo docs-only, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "SPEC_REGISTRY|SPEC_GENERATION_ROADMAP|SPEC_CREATED|READY_FOR_IMPLEMENTATION|STRUCTURAL_FIX_REQUIRED|ARCHIVE_CANDIDATE|a_implementar" .specs docs/validation .claude/rules .claude/skills CLAUDE.md
rg -n "ACCEPTED|IMPLEMENTED|implementados|SPEC_CREATED|READY_FOR_IMPLEMENTATION|DEFERRED_TO_FINAL_HUMAN_VALIDATION|BUILD_VALIDATED|PARTIAL|BLOCKED|Testing Quality Gate|FINAL_HUMAN_VALIDATION" .specs docs/validation .claude/rules .claude/skills CLAUDE.md
rg -n "Assets/|Packages/|ProjectSettings/" .specs/a_implementar .specs/SPEC_REGISTRY_TO_IMPLEMENT.md .specs/SPEC_GENERATION_ROADMAP_MASTER.md
```

Se ambiente local permitir:

```bash
find .specs/a_implementar -maxdepth 1 -type f -name "*.md" | sort
```

No PowerShell:

```powershell
Get-ChildItem .specs/a_implementar -Filter *.md | Sort-Object Name | Select-Object -ExpandProperty Name
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given as specs geradas existem em .specs/a_implementar/
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

- Registry aponta arquivo inexistente.
- Arquivo existe mas não entra no registry.
- Spec antiga duplicada não é marcada como archive candidate.
- Spec criada aparece como implementada.
- SPEC_EXECUTION_ORDER é alterado cedo demais.
- Roadmap deixa contagem incompatível com arquivos reais.
- Testing Quality Gate ausente passa despercebido.
- Docs-only closeout altera runtime por acidente.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Spec Registry Roadmap Reconciliation Closeout Docs

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
rg -n "Testing Quality Gate|DEFERRED_TO_FINAL_HUMAN_VALIDATION|SPEC_CREATED|READY_FOR_IMPLEMENTATION|IMPLEMENTED|ACCEPTED|implementados|Assets/|Packages/|ProjectSettings/" .specs docs/validation .claude/rules .claude/skills CLAUDE.md
```

Estrutural:

```text
Enumerar specs em .specs/a_implementar/.
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
Execution report criado em docs/validation/12_spec_registry_roadmap_reconciliation_closeout_docs_execution_report.md.
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

Se qualquer alteração precisar sair de .specs, docs/validation, .claude/rules ou .claude/skills, parar e pedir decisão humana.
