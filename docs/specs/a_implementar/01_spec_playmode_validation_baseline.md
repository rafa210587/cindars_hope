# SPEC — PlayMode Validation Baseline and Final Scenario Routing

> **Spec ID:** `01_spec_playmode_validation_baseline`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
> **Wave:** WAVE 01 — Core IDs / Events / Save baseline  
> **Priority:** P1  
> **Type:** Validation / Tooling / PlayMode / Human Final Scenario / Quality Gate  
> **Domain:** Testing / Unity Validation / PlayMode / Final Human Validation  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_01_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** any spec that modifies Unity test asmdefs, `Assets/_Game/Tests/**`, `.claude/skills/unity-validation`, `.claude/commands/validate-spec.md`, `.claude/commands/finish-spec.md`, or final validation docs.  
> **Repo lock scope:** `Assets/_Game/Tests/PlayMode/**`, `Assets/_Game/Tests/EditMode/**`, `.claude/skills/unity-validation/SKILL.md`, `.claude/commands/validate-spec.md`, `.claude/commands/finish-spec.md`, `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`, `docs/validation/01_spec_playmode_validation_baseline_execution_report.md`.  
> **Depends on:**  
> - `docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md`  
> - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md`  
> - `docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> **Blocks:**  
> - consistent PlayMode/final-human evidence for Waves 02+;  
> - promotion of runtime/gameplay specs beyond `BUILD_VALIDATED`/`UNITY_VALIDATED`;  
> - wave-level final human validation handoff.  
> **Scope:** create a baseline PlayMode validation routing contract and optional minimal automated PlayMode smoke test if safe, without requiring human validation per spec.  
> **Out of scope:** complete PlayMode coverage for all systems, human testing now, CI setup, scene/prefab edits, gameplay changes or acceptance of MVP Phase 2-3.

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

- docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
- docs/specs/SPEC_GENERATION_ROADMAP_TESTING_QUALITY_GATE_ADDENDUM.md
- docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md
- .claude/rules/testing-quality-gate.md
- .claude/skills/spec-execution/TESTING_QUALITY_GATE_ADDENDUM.md
- .claude/skills/unity-validation/TESTING_QUALITY_GATE_ADDENDUM.md
- docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos e do roadmap macro.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável, com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Baseline de PlayMode/final human validation por wave/lote.
- Separação entre build/compile validation e aceite real de runtime/gameplay.
- Obrigatoriedade de Testing Quality Gate e report de evidência.

### Deferred / future from directions

- Human test por spec individual.
- Automação completa de todos os cenários PlayMode.
- CI/CD final de Unity.

### Explicitly not redefined here

- Conteúdo de gameplay.
- Implementação de runtime de domínio.
- Registry/roadmap final, que será corrigido no fechamento.

---

# /speckit.specify

## 1. Contexto

The quality gate spec defines that runtime/gameplay changes cannot be accepted by compile alone. However, the project also decided that the human user will not validate every micro-spec in Unity. Human validation must be consolidated by wave/lote at the end.

This spec creates the operational baseline connecting automated PlayMode where possible and final human validation where necessary.

It must not ask the user to open Unity per spec. It must only create routing, baseline checks, scenario references and report expectations.

---

## 2. Problema

Without a PlayMode baseline:

```text
runtime specs may stop at compile and claim too much;
UI/input/prefab/cave/scene behavior may remain untested;
final human validation may be too broad and not tied to specs;
agents may create per-spec manual scenarios inconsistently;
PlayMode tests may be created in unsafe folders/asmdefs;
finish-spec may not know whether to use BUILD_VALIDATED, UNITY_VALIDATED or DEFERRED_TO_FINAL_HUMAN_VALIDATION.
```

---

## 3. Objetivo

At the end of this spec, the repo should have a baseline that defines:

```text
where automated PlayMode tests live;
when PlayMode automated is required;
when final human scenario is enough;
how specs reference wave-level final validation;
how reports record deferred human validation;
how status caps apply before final human validation.
```

---

## 4. Fontes obrigatórias lidas

Execution must read:

```text
CLAUDE.md
AGENTS.md
docs/project/CURRENT_STATE.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
.claude/commands/validate-spec.md
.claude/commands/finish-spec.md
```

Conditional reads:

```text
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md
docs/validation/current/LAST_VALIDATION_STATUS.md, if present
```

---

## 5. Estado atual do repo

Documented state:

```text
- Unity compile validation exists as implemented spec.
- 01Q quality gate spec exists and was adjusted to defer human validation.
- FINAL_HUMAN_VALIDATION_BY_WAVE.md exists as wave/lote checklist.
- Current State says Phase 2 Unity validators and Phase 3 Play Mode are NOT RUN and pending local Unity/human execution.
```

Local audit required:

```text
- whether PlayMode test folders exist;
- whether test asmdefs exist;
- whether Unity Test Runner can be invoked locally;
- whether any current PlayMode tests exist;
- whether commands/skills still require human test per spec;
- whether validation docs already have current status path.
```

---

## 6. User stories / engineering stories

```text
As a spec executor, I need to know if my runtime spec needs automated PlayMode or final human scenario.
As a maintainer, I need a baseline so agents do not invent test locations.
As a human validator, I need wave-level scenarios instead of micro-spec validation.
As a reviewer, I need status caps before final human PlayMode.
As a future CI pipeline, I need predictable PlayMode test folders and naming.
```

---

## 7. Escopo

Includes:

```text
- audit Unity test structure;
- define PlayMode baseline location/naming;
- add or update docs/routing for final human scenario by wave;
- create minimal PlayMode smoke test only if safe;
- update command/skill references only if still misaligned;
- create execution report.
```

---

## 8. Fora de escopo

Does not include:

```text
- running human Unity validation now;
- claiming Phase 2-3 passed;
- complete PlayMode coverage for gameplay;
- changing scenes/prefabs;
- altering gameplay code to make tests pass;
- creating CI workflow;
- changing ProjectSettings or Packages.
```

---

## 9. Regras de não duplicação

```text
Do not create a second final human validation checklist.
Do not create another validation matrix.
Do not create new commands if existing validate/finish commands can reference the baseline.
Do not duplicate 01Q; this spec operationalizes PlayMode/final scenario routing only.
Do not create tests outside Assets/_Game/Tests/**.
```

---

## 10. Critérios de aceite

### 10.1 Baseline defined

Document or update a canonical baseline, preferably in the execution report and, if needed, in `docs/validation/PLAYMODE_VALIDATION_BASELINE.md`.

It must define:

```text
automated PlayMode test location;
optional smoke test naming;
wave-level final human scenario reference;
status cap before final human validation;
NOT RUN recording format;
relationship with FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

### 10.2 Commands/skills aligned

Confirm or adjust:

```text
validate-spec uses validation matrix and can recognize PlayMode/final scenario requirement;
finish-spec does not demand human validation per spec;
unity-validation skill distinguishes compile vs PlayMode behavior;
spec-execution skill supports DEFERRED_TO_FINAL_HUMAN_VALIDATION.
```

### 10.3 Test folders audited

Audit:

```text
Assets/_Game/Tests/EditMode/
Assets/_Game/Tests/PlayMode/
asmdefs under test folders;
existing tests;
Unity compile compatibility.
```

### 10.4 Minimal smoke test optional

If safe, add one minimal PlayMode smoke test that does not depend on scenes/prefabs. If not safe, do not create asmdefs blindly; record `NOT IMPLEMENTED` and residual risk.

### 10.5 Report

Create:

```text
docs/validation/01_spec_playmode_validation_baseline_execution_report.md
```

---

# /speckit.plan

## 11. Arquitetura alvo

Possible target files:

```text
Assets/_Game/Tests/PlayMode/Smoke/PlayModeSmokeTests.cs
Assets/_Game/Tests/PlayMode/Smoke/PlayModeSmokeTests.asmdef, only if safe
docs/validation/PLAYMODE_VALIDATION_BASELINE.md, if a persistent doc is necessary
docs/validation/01_spec_playmode_validation_baseline_execution_report.md
```

Prefer report-only if test infrastructure is unclear.

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

N/A.

### 12.2 Runtime contracts

```text
No gameplay runtime contract changes.
Validation routing becomes an execution contract for specs.
```

### 12.3 Event contracts

N/A.

### 12.4 Save contracts

N/A.

### 12.5 UI contracts

```text
UI specs must document PlayMode automated or final human scenario if they change input/modal/canvas/prefab/scene behavior.
```

---

## 13. Sistemas afetados

```text
Unity test structure;
validation commands;
unity-validation skill;
spec-execution skill;
final human validation workflow;
future execution reports.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Tests/PlayMode/**
Assets/_Game/Tests/EditMode/**
.claude/skills/unity-validation/SKILL.md
.claude/skills/spec-execution/SKILL.md
.claude/commands/validate-spec.md
.claude/commands/finish-spec.md
docs/validation/PLAYMODE_VALIDATION_BASELINE.md
docs/validation/01_spec_playmode_validation_baseline_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Assets/_Game/Scripts/**, except test-only helper under test folders if strictly required
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset outside test asmdef/test assets
Packages/**
ProjectSettings/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local

```bash
find Assets/_Game/Tests -maxdepth 5 -type f 2>/dev/null || true
rg -n "DEFERRED_TO_FINAL_HUMAN_VALIDATION|PlayMode|FINAL_HUMAN_VALIDATION_BY_WAVE|human test|docs/05_VALIDATION" .claude docs/validation docs/specs CLAUDE.md AGENTS.md
```

### Fase 1 — Routing

```text
1. Confirm commands/skills already align.
2. Patch only if mismatch exists.
3. Do not duplicate matrices/checklists.
```

### Fase 2 — Test baseline

```text
1. Inspect existing asmdefs/tests.
2. Create minimal PlayMode smoke only if compile-safe.
3. Otherwise report backlog.
```

### Fase 3 — Report

```text
1. Create execution report.
2. Record status caps and next usage rules.
```

---

## 17. Ordem segura de execução

```text
1. Read sources.
2. Audit current test folders and harness references.
3. Patch routing only if needed.
4. Add minimal test/doc only if safe.
5. Run docs validation.
6. Run Unity compile if tests/asmdefs changed.
7. Create report.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_01_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - 01Q execution;
  - any other harness/validation/test folder edit;
  - runtime specs relying on final PlayMode status.
- Shared files/systems requiring lock:
  - `.claude` validation commands/skills;
  - Unity test folders;
  - final validation docs.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO
Does this add a save section? NO
Does this require migration? NO
Does this persist Unity references? NO
```

---

## 20. Impacto em eventos

```text
Adds events: NO
Changes existing events: NO
Requires unsubscribe pattern: NO
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: CONDITIONAL — only test asmdefs/test assets under test folders if safe.
Requires Play Mode final validation: NO for this spec.
Human validation timing: NOT REQUIRED.
```

---

## 22. Riscos técnicos

```text
Risk: asmdef/test folder breaks Unity compile.
Mitigation: inspect current test setup first; skip creation if unclear.

Risk: final human validation becomes too vague.
Mitigation: anchor to FINAL_HUMAN_VALIDATION_BY_WAVE.md and require spec reports to reference wave/domain scenario.

Risk: agents still claim ACCEPTED too early.
Mitigation: finish-spec and validation matrix status caps must apply.
```

---

## 23. Rollback

```text
Remove created PlayMode test/asmdef.
Revert command/skill patches.
Remove baseline doc/report.
No gameplay/runtime state should need rollback.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Read required sources.
- [ ] T002 — Audit PlayMode/EditMode test structure.
- [ ] T003 — Audit commands/skills for final-human routing.
- [ ] T004 — Patch routing only if needed.
- [ ] T005 — Create minimal baseline doc and/or smoke test only if safe.
- [ ] T006 — Run docs validation.
- [ ] T007 — Run Unity compile if tests/asmdefs changed.
- [ ] T008 — Create execution report.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de testing quality gate / validation baseline? | Arquivos alterados e justificativa. | PARTIAL |
| Save/load | Houve schema change? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/01_spec_playmode_validation_baseline_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "EditMode|PlayMode|QualityGate|Testing Quality Gate|BUILD_VALIDATED|PARTIAL|ACCEPTED|Final Human" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a testing quality gate / validation baseline existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing implementation is found

```text
Given existe implementação parcial ou completa no repo
When a execução audita o estado real
Then ela muda para REUSE_EXISTING ou HARDEN_EXISTING
And não recria arquitetura paralela
And documenta residual/future gaps.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige interação visual, PlayMode ou gameplay integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Usar compile/build como aceite de gameplay.
- Pedir human test por spec individual.
- Não registrar cenário final por wave/lote.
- Aceitar runtime sem PlayMode ou cenário final documentado.
- Bugfix sem regression test ou risco residual.
- Lógica determinística sem EditMode quando praticável.
- Status ACCEPTED sem evidência mínima.
- Não atualizar o execution report.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — PlayMode Validation Baseline and Final Scenario Routing

## Summary
- Spec:
- Wave: WAVE 01
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
- Existing implementation handling:
- Missing dependency:
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
6. A implementação executar WAVE 02+ em massa antes da 01Q ou exceção humana explícita.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Alignment search:

```bash
rg -n "docs/05_VALIDATION|human test por spec|Play Mode humano obrigatório por spec" CLAUDE.md AGENTS.md .claude docs/specs docs/validation
rg -n "DEFERRED_TO_FINAL_HUMAN_VALIDATION|FINAL_HUMAN_VALIDATION_BY_WAVE|SPEC_VALIDATION_MATRIX_MASTER" CLAUDE.md .claude docs/specs docs/validation
```

Unity compile only if Unity test files/asmdefs changed.

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if command/skill logic or smoke test code is changed; otherwise NO.
- Requires EditMode tests: NO unless deterministic validator logic is added.
- Requires PlayMode automated or final human scenario: NO for this spec; this spec defines routing.
- Requires regression test: YES if fixing a known finish/validate command bug; otherwise NO.
- Human validation timing: NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; alignment searches PASS or residuals documented; Unity compile PASS if test assets changed; execution report created; no human validation per spec introduced.

---

## 27. Definition of Done

```text
PlayMode validation baseline documented.
Final human validation remains wave/lote based.
No per-spec human validation requirement exists.
Test folders audited.
Minimal smoke created only if safe, otherwise backlog recorded.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Do not claim PlayMode/human validation passed without execution.
Do not require user validation per spec.
Do not edit scenes/prefabs for this spec.
Do not change ProjectSettings/Packages.
Do not let runtime specs become ACCEPTED by compile alone.
```

---

## 29. Notas para execução posterior

Future runtime specs must reference either automated PlayMode evidence or final human scenario coverage by wave/domain before moving beyond build-only status.
