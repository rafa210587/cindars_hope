# /execute-spec-strict

> **NOTA DE RECONCILIAÇÃO (2026-06-12):** a fila wave-based foi executada e movida para executadas_build_validated/. A fila ativa é .specs/a_implementar/fable/. Exemplos com paths NN_spec_* abaixo são históricos.


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
/execute-spec-strict .specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md
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

## Preflight Obrigatório (Windows / PowerShell)

Antes de executar qualquer spec:

1. **Leia as rules obrigatórias:**
   - `.claude/rules/windows_powershell_only.md`
   - `.claude/rules/spec_dependency_resolution.md`
   - `.claude/rules/spec_quality_gate.md`

2. **Rode o preflight PowerShell:**
   ```powershell
   Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
   git status --short | Select-Object -First 50
   git branch --show-current
   ```

3. **Verifique:**
   - ✓ Diretório correto
   - ✓ Branch correto (`dev`)
   - ✓ Estado uncommitted esperado
   - ✓ Use apenas sintaxe PowerShell (sem comandos Unix/Bash)

Se um comando falhar usando sintaxe Unix, refaça em PowerShell antes de tratar como falha.

---

## Fluxo Obrigatório

### 1. Leitura de Context (5 min)

Ler em ordem:
1. `docs/project/CURRENT_STATE.md`
2. `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
3. `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
4. `.claude/rules/spec_quality_gate.md`

### 2. Identificação da Spec (2 min)

Identificar arquivo `.md` alvo em `.specs/a_implementar/`.

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

### 3.5. Checagem de Dependency Chain (5 min) — RESOLUÇÃO AUTOMÁTICA

**Se a spec depende de outra spec não resolvida na mesma wave:**

1. **Não pergunte ao usuário; resolva automaticamente.**
2. Marque a spec atual como `BLOCKED_BY_DEPENDENCY_PENDING` (status temporário).
3. Extraia a dependency chain da spec alvo (leia `.claude/rules/spec_dependency_resolution.md`).
4. Procure specs same-wave em `.specs/a_implementar/<wave>_spec_*.md`.
5. Construa o dependency DAG (directed acyclic graph).
6. Se a dependência for **proibida** (future/pets/HOLD/requer Packages/ProjectSettings/requer scene/prefab):
   - Pare com `BLOCKED_BY_FORBIDDEN_SCOPE`
   - Documente o motivo
7. Se a dependência for **same-wave e permitida**:
   - Identifique a **root** (spec sem dependências)
   - Execute a root primeiro via `/execute-spec-strict`
   - Continue para cima até alcançar a spec original
   - Não pivote para specs não relacionadas
8. Atualize `docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md`
9. Atualize `docs/validation/WAVE_<wave>_BATCH_STATE.md`
10. Volte para a spec original depois que a chain for resolvida

**Cláusula obrigatória no execution report:**
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

Usar template `.specs/SPEC_EXECUTION_REPORT_TEMPLATE_STRICT.md`.

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

### 9.5. Documentar o Método de Validação no Report

Todo execution report deve incluir:

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

Não afirme sucesso de build sem essa evidência.

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

## Quando parar e reportar

**PARE IMEDIATAMENTE com status BLOCKED se:**

1. A spec exige future/pets/mapped specs
2. A spec exige criação de scene/prefab/asset
3. A spec exige alteração de Packages/ ou ProjectSettings/
4. O build falha com novos erros
5. A docs validation falha com novos erros
6. O quality check falha com erros críticos (não warnings)
7. Uma spec P0 tem status NEEDS_REWORK ou BLOCKED
8. A spec não pode ser executada sozinha (exige spec paralela)

Quando BLOCKED:
- Crie o execution report com status BLOCKED
- Documente o blocker na seção "Remaining work"
- NÃO faça commit (a não ser que um commit report-only seja significativo)
- Pare

## Uso Loop-Safe — Até 10 Specs

Este command é seguro em `/loop` para batches de até 10 specs:

### Exemplo: batch de 10 specs

```text
/loop
Run /execute-spec-strict next --wave 05.
Max specs this batch: 10.
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, build failure, docs validation new failure, quality check failure.
Commit after each successful spec.
Do not start next wave in this loop.
Do not mark ACCEPTED.
```

### Regras

1. **Uma spec por iteração** — cada invocação do loop executa exatamente uma spec
2. **Máximo 10 por batch** — pode re-invocar até 10 vezes no mesmo `/loop`
3. **Máximo recomendado por wave:**
   - 3 specs para waves novas/instáveis
   - 10 specs para waves estabelecidas com patterns conhecidos
4. **Quality gates por spec** (não no fim do batch):
   - Docs validation PASS
   - Assembly builds 0E
   - Quality check PASS
   - Status honesto (sem inflar)
5. **Pare imediatamente em:**
   - Status `BLOCKED`
   - Status `NEEDS_REWORK`
   - `CONTRACT_ONLY_NEEDS_INTEGRATION` em spec fundacional
   - Falha de build
   - Novo erro de docs validation
   - Falha crítica de quality check
   - Arquivo proibido alterado
   - Report ausente/incompleto

### Formato de Saída

Depois de cada spec no batch do loop, a saída deve ser:

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

O command em si não faz loop; `/loop` vai re-invocá-lo até 10 vezes.

## Fallback para Execução Manual

Se o usuário rodar `/execute-spec-strict` sem argumento:

Pergunte ao usuário:
1. Path da spec?
2. Ou "next" com flags --wave e --after?

Depois prossiga normalmente.

---

*Created: 2026-06-08 (Strict Spec Execution)*  
*Safe to use in `/loop` but does not require it.*
