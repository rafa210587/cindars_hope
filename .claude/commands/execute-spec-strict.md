# /execute-spec-strict

Execute exatamente UMA spec com qualidade rigorosa e pare.

## Propósito

Executar specs de forma leve e confiável:
- Uma por vez
- Com auditoria real de sistemas existentes
- Com status honesto (não inflado)
- Com execution report obrigatório
- Sem obrigar projeto a usar `/loop`

## Entrada Esperada

```text
/execute-spec-strict docs/specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md
```

Ou:

```text
/execute-spec-strict next --wave 04
```

Ou:

```text
/execute-spec-strict next --after 04_spec_ui_calendar_day_detail_runtime
```

## Restrições Obrigatórias

- Executar uma única spec
- Não executar próxima spec automaticamente
- Não executar WAVE futura
- Não executar future/mapped specs
- Não executar pets
- Não rodar Unity Test Runner
- Não fazer PlayMode/human validation
- Não marcar ACCEPTED
- Não mover para implementados
- Não alterar Packages/, ProjectSettings/, scenes, prefabs, assets

## Mandatory Preflight (Windows / PowerShell)

Before executing any spec:

1. **Read mandatory rules:**
   - `.claude/rules/windows_powershell_only.md`
   - `.claude/rules/spec_dependency_resolution.md`
   - `.claude/rules/spec_quality_gate.md`

2. **Run PowerShell preflight:**
   ```powershell
   Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
   git status --short | Select-Object -First 50
   git branch --show-current
   ```

3. **Verify:**
   - ✓ Correct directory
   - ✓ Correct branch (`dev`)
   - ✓ Expected uncommitted state
   - ✓ Use PowerShell syntax only (no Unix/Bash commands)

If a command fails using Unix syntax, retry in PowerShell before treating as failure.

---

## Fluxo Obrigatório

### 1. Leitura de Context (5 min)

Ler em ordem:
1. `docs/project/CURRENT_STATE.md`
2. `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
3. `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
4. `.claude/rules/spec_quality_gate.md`

### 2. Identificação da Spec (2 min)

Identificar arquivo `.md` alvo em `docs/specs/a_implementar/`.

Confirmar que a spec não está em `implementados/` ou `absorvidas/`.

### 3. Leitura Completa da Spec (10 min)

Ler spec inteira. Extrair:

- **Escopo:** o que a spec faz
- **Fora de escopo:** o que NOT faz (listar explicitamente)
- **Critérios de aceite:** acceptance criteria section
- **Dependências:** specs/docs/systems de que depende
- **Stop conditions:** quando parar com BLOCKED
- **Arquivos permitidos:** allow-list de arquivos que podem ser alterados
- **Arquivos proibidos:** no-change list (Packages, ProjectSettings, etc)

### 3.5. Dependency Chain Check (5 min) — AUTOMATIC RESOLUTION

**If spec depends on another unresolved spec in the same wave:**

1. **Do NOT ask the user; resolve automatically.**
2. Mark current spec as `BLOCKED_BY_DEPENDENCY_PENDING` (temporary status).
3. Extract dependency chain from target spec (read `.claude/rules/spec_dependency_resolution.md`).
4. Search same-wave specs in `docs/specs/a_implementar/<wave>_spec_*.md`.
5. Build dependency DAG (directed acyclic graph).
6. If dependency is **forbidden** (future/pets/HOLD/requires Packages/ProjectSettings/requires scene/prefab):
   - Stop with `BLOCKED_BY_FORBIDDEN_SCOPE`
   - Document reason
7. If dependency is **same-wave and allowed**:
   - Identify **root** (spec with no dependencies)
   - Execute root first via `/execute-spec-strict`
   - Continue upward until original spec is reached
   - Do NOT pivot to unrelated specs
8. Update `docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md`
9. Update `docs/validation/WAVE_<wave>_BATCH_STATE.md`
10. Return to original spec after chain is resolved

**Required output clause in execution report:**
```text
## Dependency Chain

Original target: <spec>
Resolved chain: <spec1> → <spec2> → ... → <original>
Root dependency: <spec>
Forbidden dependencies: <none | listed>
Depth: <N specs>
Can continue original target: YES/NO
```

### 4. Auditoria de Sistemas Existentes (10 min)

Se a spec menciona sistema (GameplayInputRouter, ModalManager, InventoryManager, etc):

Pesquisar:
```powershell
Get-ChildItem .\Assets\_Game\Scripts -Recurse -Filter *.cs | 
  Select-String -Pattern "class (GameplayInputRouter|ModalManager|...)"
```

Documentar:
- Existe?
- Qual arquivo?
- Qual contrato?
- Reutilizar ou criar novo?

### 5. Decisão de Estratégia (5 min)

Decidir:

- **REUSE_EXISTING** — sistem canônico existe, reutilizar
- **HARDEN_EXISTING** — sistema existe parcial, completar
- **ADAPTER** — sistema existe, criar adapter/wrapper mínimo
- **CONTRACT_ONLY** — criar DTO/model/interface apenas
- **DEFERRED_UI_VISUAL** — lógica pronta, UI deferred
- **DEFERRED_INTEGRATION** — contrato pronto, integração futura
- **BLOCKED** — parar aqui, não pode continuar

Se decisão for BLOCKED, pular para seção "Stop" abaixo.

### 6. Implementação Mínima (varies)

Implementar a menor alteração segura para o escopo.

Regras:
- Sem scope creep
- Sem abstrações prematuras
- Sem comentários desnecessários
- Testes EditMode se há lógica determinística
- Testes ONLY em `Assets/_Game/Tests/EditMode/**`
- Reutilizar existente quando possível

### 7. Execution Report Individual (15 min)

Criar ou atualizar:
```text
docs/validation/<spec_id>_execution_report.md
```

Usar template `docs/specs/SPEC_EXECUTION_REPORT_TEMPLATE_STRICT.md`.

Preencher:
- Status (um de spec_quality_gate.md)
- Acceptance criteria extracted (tabela)
- Existing systems audit
- Scope executed
- Out of scope respected
- Files changed
- Spec Compliance Matrix
- Validation results
- Honest status rationale
- Remaining work

### 8. Preenchimento da Compliance Matrix

Na seção "Spec Compliance Matrix" do report, mapear:

| Spec Requirement | Implementation Evidence | Status | Notes |
|---|---|---|---|
| (spec says this) | (code does this) | OK / DEFERRED / FAIL / NOT_APPLICABLE | (notes) |

Allowed status:
- `OK` — implemented
- `OK_WITH_WARNINGS` — implemented, warnings documented
- `CONTRACT_ONLY` — contract/DTO only
- `DEFERRED` — explicit defer, documented
- `FAIL` — not implemented, not documented defer
- `NOT_APPLICABLE` — not relevant

### 9. Validation (5 min) — STRICT VALIDATION TRUTH GATE

**Use central validation script:**

```powershell
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Strict validation failed. Do not commit."
    exit 1
}
```

**FORBIDDEN pattern:**
```powershell
❌ dotnet build ... | Select-String "error"
❌ dotnet build ... 2>&1 | Select-String "error|Error"
```

These patterns filter output and lose `$LASTEXITCODE`, allowing false "build pass" claims.

**Expected results:**
- Docs validation: PASS or `EXPECTED_FAIL_LEGACY_ONLY`
- Assembly-CSharp: PASS (exit code 0)
- Assembly-CSharp-Editor: PASS (exit code 0)
- Quality check: PASS (exit code 0)
- Overall: `VALIDATION_PASS` (exit code 0)

### 9.5. Document Validation Method in Report

Every execution report must include:

```text
## Validation

Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS
Assembly-CSharp-Editor: PASS
Quality check: PASS
Docs validation: PASS / EXPECTED_FAIL_LEGACY_ONLY
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Do not claim build success without this evidence.

---

### 10. Classificação Honesta de Status (5 min)

Usar matriz em `.claude/rules/spec_quality_gate.md`.

Honest status:
- Se compliance matrix tem OK para critérios centrais + report + tests + no proibidos → BUILD_VALIDATED
- Se critério central atendido + deferred noted → BUILD_VALIDATED_WITH_WARNINGS
- Se só contrato criado → CONTRACT_ONLY
- Se contrato + integração futura → CONTRACT_ONLY_NEEDS_INTEGRATION
- Se critério central P0 não atendido → NEEDS_REWORK
- Se não pode continuar → BLOCKED

**Nunca inflar status. Melhor errar para baixo (CONTRACT_ONLY) que para cima (BUILD_VALIDATED falso).**

### 11. Atualização de Docs Canônicas (5 min)

Se status justificar, atualizar:
- `docs/project/CURRENT_STATE.md` — listar spec com novo status
- `docs/validation/WAVE_*_LOOP_BATCH_STATUS.md` — se relevante

Não atualizar se status for CONTRACT_ONLY ou deferido.

### 12. Commit (2 min)

Fazer commit ONLY se:
- Build/docs passaram
- Quality check passou (ou falhou com warnings only, documentados)
- Status está honesto
- Nenhum arquivo proibido foi alterado

Mensagem:
```text
feat: execute <spec_id> runtime [<priority>]

<one-line summary of what changed>

Status: <BUILD_VALIDATED / BUILD_VALIDATED_WITH_WARNINGS / CONTRACT_ONLY / NEEDS_REWORK / BLOCKED>

Co-Authored-By: Claude Haiku 4.5 <noreply@anthropic.com>
```

### 13. Parar

Não executar próxima spec.
Não loop automaticamente.

## Saída Obrigatória

Responder com:

```text
SPEC_EXECUTION_RESULT
═══════════════════════════

Spec:                           <spec_id>
Status:                         <BUILD_VALIDATED | CONTRACT_ONLY | NEEDS_REWORK | BLOCKED>
Acceptance criteria matched:    <Y/N + count>
Existing systems audit:         <Y/N>
Execution report created:       <Y/N + path>
Spec Compliance Matrix:         <OK/DEFERRED/FAIL counts>

Validation
──────────
Docs validation:                <PASS | FAIL>
Assembly-CSharp build:          <PASS | FAIL>
Assembly-CSharp-Editor build:   <PASS | FAIL>
Quality check:                  <PASS | FAIL>

Files Changed
─────────────
Count:                          <N>
Tests in correct location:      <Y/N>
Forbidden files altered:        <Y/N>

Commit
──────
Commit hash:                    <hash or NONE>
Message:                        <one-liner>

Next Actions
────────────
Can continue next spec:         <Y/N + reason>
Can start next wave:            <Y/N + reason>
Remaining blockers:             <list or NONE>
```

## Stop Conditions

**STOP IMMEDIATELY with status BLOCKED if:**

1. Spec requires future/pets/mapped specs
2. Spec requires scene/prefab/asset creation
3. Spec requires Packages/ or ProjectSettings/ alteration
4. Build fails with new errors
5. Docs validation fails with new errors
6. Quality check fails with critical errors (not warnings)
7. P0 spec has status NEEDS_REWORK or BLOCKED
8. Spec cannot be executed solo (requires parallel spec)

When BLOCKED:
- Create execution report with BLOCKED status
- Document blocker in "Remaining work" section
- Do NOT commit (unless report-only commit is meaningful)
- Stop

## Loop-Safe Usage — Up to 10 Specs

This command is safe in `/loop` for batches up to 10 specs:

### Example: 10-Spec Batch

```text
/loop
Run /execute-spec-strict next --wave 05.
Max specs this batch: 10.
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, build failure, docs validation new failure, quality check failure.
Commit after each successful spec.
Do not start next wave in this loop.
Do not mark ACCEPTED.
```

### Rules

1. **One spec per iteration** — each loop invocation executes exactly one spec
2. **Max 10 per batch** — can re-invoke up to 10 times in the same `/loop`
3. **Recommended max per wave:**
   - 3 specs for new/unstable waves
   - 10 specs for established waves with known patterns
4. **Quality gates per spec** (not at end of batch):
   - Docs validation PASS
   - Assembly builds 0E
   - Quality check PASS
   - Status honest (no inflation)
5. **Stop immediately on:**
   - `BLOCKED` status
   - `NEEDS_REWORK` status
   - `CONTRACT_ONLY_NEEDS_INTEGRATION` on foundational spec
   - Build failure
   - Docs validation new error
   - Quality check critical failure
   - Forbidden file altered
   - Report missing/incomplete

### Output Format

After each spec in the loop batch, output must be:

```text
SPEC_EXECUTION_RESULT
──────────────────────
Spec: <spec_id>
Status: <BUILD_VALIDATED | BUILD_VALIDATED_WITH_WARNINGS | CONTRACT_ONLY | NEEDS_REWORK | BLOCKED>
Acceptance criteria matched: <count>
Execution report: <path>
Compliance Matrix: <OK/DEFERRED/FAIL counts>
Validations: Docs ✓ | Assembly-CSharp ✓ | Assembly-CSharp-Editor ✓ | Quality ✓
Commit: <hash or NONE>
Can continue next spec: <YES/NO + reason>
Can start next wave: NO (always NO in loop batch)
```

The command itself won't loop; `/loop` will re-invoke it up to 10 times.

## Fallback for Manual Execution

If user runs `/execute-spec-strict` without argument:

Ask user:
1. Spec path?
2. Or "next" with --wave and --after flags?

Then proceed normally.

---

*Created: 2026-06-08 (Strict Spec Execution)*  
*Safe to use in `/loop` but does not require it.*
