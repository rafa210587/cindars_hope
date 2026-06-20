# /loop-spec-batch-strict

Texto de referência para rodar um batch controlado de specs via `/loop`.

## Preflight Obrigatório (Windows / PowerShell)

Antes de iniciar qualquer batch loop:

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
   - ✓ Use apenas sintaxe PowerShell (sem comandos Unix/Bash)

## Uso

Use este texto de command dentro de `/loop` para rodar um batch validado de specs.

```text
/loop
Run /execute-spec-strict next --wave <WAVE>.
Max specs this batch: 10.
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, DEFERRED_UI_VISUAL on foundational spec, build failure, docs validation new failure, quality check failure.
Commit after each successful spec.
Do not start next wave in this loop.
Do not mark ACCEPTED.
```

Substitua `<WAVE>` pela wave alvo (ex.: `05`).

## Comportamento da Dependency Chain Dentro do Loop

Se uma spec encontra uma dependência same-wave:

1. **Resolva a chain automaticamente** — não pergunte ao usuário.
2. **Marque a spec atual** como `BLOCKED_BY_DEPENDENCY_PENDING`.
3. **Execute a root dependency primeiro** via `/execute-spec-strict`.
4. **Continue para cima** na dependency chain.
5. **Volte para a spec alvo original** depois que todas as dependências passarem.
6. **NÃO conte** `BLOCKED_BY_DEPENDENCY_PENDING` como falha final.
7. **NÃO pivote** para specs não relacionadas enquanto a chain estiver aberta.
8. **Atualize os arquivos de batch state:**
   - `docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md`
   - `docs/validation/WAVE_<wave>_BATCH_STATE.md`

**Exemplo:** se o loop tenta executar `companion_farm_job_board`, que depende de `farm_animals`, que depende de `farm_buildings`, etc., o loop vai resolver a chain inteira automaticamente antes de voltar para `companion_farm_job_board`.

A profundidade resolvida **não** conta contra o máximo de 10 specs se todas as specs da chain resolverem com sucesso.

---

## Regra do Strict Validation Truth Gate

**Todas as iterações do loop devem usar strict validation com exit codes explícitos.**

Depois de cada spec no loop, rode:

```powershell
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Strict validation failed."
    Stop the loop immediately
}
```

**PROIBIDO no loop:**
```powershell
❌ dotnet build ... | Select-String "error"
```

Se output filtrado for usado em qualquer ponto do loop, o loop deve parar e reportar `VALIDATION_SCRIPT_FAILURE`.

---

## Regra de Ambiente Windows

Todas as iterações do loop devem usar **apenas sintaxe PowerShell**.

Se uma invocação de `/execute-spec-strict` usar comandos Bash/Unix e falhar:

- **NÃO marque a spec BLOCKED imediatamente.**
- Marque como `ENV_COMMAND_RETRY_REQUIRED`.
- Refaça o passo usando o equivalente PowerShell.
- Só se o retry PowerShell também falhar, classifique como `ENV_COMMAND_FAILURE`.
- Falha de comando de ambiente, sozinha, não para o loop.

---

## Regras

### Validação Por Spec (NÃO no fim do batch)

1. **Uma spec por iteração** — cada ciclo de `/loop` executa exatamente UMA spec
2. **Máximo 10 specs por batch** — o loop pode re-invocar `/execute-spec-strict` até 10 vezes
3. **Máximo recomendado por wave:**
   - 3 specs para waves novas/instáveis (WAVE 04 fase 1)
   - 10 specs para waves estabelecidas com patterns conhecidos (WAVE 05+)
   - Nunca >10 sem code review externo

### Quality Gates Por Spec

Cada spec DEVE passar antes de continuar para a próxima:

- ✓ `docs validation` PASS (sem novos erros)
- ✓ `dotnet build Assembly-CSharp` — 0 erros
- ✓ `dotnet build Assembly-CSharp-Editor` — 0 erros
- ✓ `./tools/docs/check_spec_quality.ps1` PASS (sem falhas críticas)
- ✓ Status honesto (não inflado)
- ✓ Execution report criado e completo
- ✓ Nenhum arquivo proibido alterado (Packages/, ProjectSettings/, scenes, prefabs, assets, runtime)

### Stop Conditions (Imediatas)

Pare o loop IMEDIATAMENTE se:

- Status é `BLOCKED`
- Status é `NEEDS_REWORK`
- Status é `CONTRACT_ONLY_NEEDS_INTEGRATION` para uma spec fundacional
- Status é `DEFERRED_UI_VISUAL` para uma spec fundacional
- Build falha com novos erros
- Docs validation falha com novos erros
- Quality check falha criticamente (não apenas warnings)
- Arquivo proibido foi alterado
- Execution report está ausente ou incompleto
- Status está inflado (ex.: `BUILD_VALIDATED` sem evidência)

### Commit Por Spec

Depois de cada spec:

- Se o status permite continuar (BUILD_VALIDATED, CONTRACT_ONLY, etc.):
  - Crie o commit: `feat: execute <spec_id> [<priority>]`
  - Continue para a próxima spec
- Se o status bloqueia (BLOCKED, NEEDS_REWORK):
  - Documente o motivo no execution report
  - Pare imediatamente
  - NÃO faça commit (a não ser que seja documentation-only)

## Formato de Saída Obrigatório

Depois de cada spec no batch, a saída DEVE incluir:

```text
SPEC_EXECUTION_RESULT
═══════════════════════

Spec:                           <spec_id>
Status:                         <BUILD_VALIDATED | BUILD_VALIDATED_WITH_WARNINGS | CONTRACT_ONLY | CONTRACT_ONLY_NEEDS_INTEGRATION | DEFERRED_UI_VISUAL | NEEDS_REWORK | BLOCKED>
Acceptance criteria matched:    <count> / <total>
Execution report created:       YES <path> | NO <reason>
Spec Compliance Matrix:         <OK count> / <DEFERRED count> / <FAIL count>

Validation Results
──────────────────
Docs validation:                PASS | FAIL <reason>
Assembly-CSharp:               PASS (0E, <W>W) | FAIL
Assembly-CSharp-Editor:        PASS (0E, <W>W) | FAIL
Quality check:                 PASS | FAIL <reason>

Files Changed
─────────────
Count:                         <N>
Tests in correct location:     YES | NO <wrong locations>
Forbidden files altered:       NO | YES <files>

Commit
──────
Hash:                          <commit hash or NONE>
Message:                       <one-liner>

Decision
────────
Can continue next spec:        YES | NO <reason>
Can start next wave:           NO (always NO in batch loop)
Remaining blockers:            <list or NONE>
```

## Batch Status NÃO É Wave Acceptance

Concluir um batch de 10 specs NÃO significa:
- `ACCEPTED` (ainda proibido)
- `PLAYMODE_VALIDATED` (ainda proibido)
- Wave pronta para release (ainda exige fases posteriores)

Um batch só pode produzir:
- `BUILD_VALIDATED`
- `BUILD_VALIDATED_WITH_WARNINGS`
- `CONTRACT_ONLY`
- `CONTRACT_ONLY_NEEDS_INTEGRATION`
- `DEFERRED_UI_VISUAL`
- `NEEDS_REWORK`
- `BLOCKED`

## Exemplo de Batch: WAVE 05 Fase 1

```text
/loop
Run /execute-spec-strict next --wave 05.
Max specs this batch: 10.
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, build failure, docs validation new failure, quality check failure.
Commit after each successful spec.
Do not start next wave in this loop.
Do not mark ACCEPTED.
```

Isto vai executar até 10 specs da WAVE 05, cada uma com validação completa.

Se WAVE 05 SPEC 1 for `NEEDS_REWORK`, pare imediatamente.
Se WAVE 05 SPEC 5 for `BLOCKED`, pare imediatamente.
Continue apenas se cada spec passar nos seus gates por spec.

## Loop Fallback

Se o loop encontrar um erro (ex.: network timeout, problema de file system):

1. Cheque o status da última spec que rodou
2. Se ela completou a validação e o report existe: é seguro refazer `/execute-spec-strict next --wave <WAVE>`
3. Se ela falhou no meio da execução: revise o execution report em busca de erros antes de refazer

## Quando usar

- Recomendado para batches de specs homogêneas (ex.: todas as UI view models)
- Recomendado para waves estabelecidas (WAVE 02+) depois que a fase 1 está estável
- Use batches de 3 specs para waves novas (WAVE 04, WAVE 05 fase 1)
- Use batches de 10 specs para patterns bem entendidos (UI VMs, criação de DTO)

## Quando NÃO usar

- NÃO use para specs P0 em waves novas (use execução de 1 spec primeiro)
- NÃO use se qualquer spec fundacional estiver indefinida (blocked, needs rework)
- NÃO use se o batch contém patterns mistos (algumas UI, alguns systems, alguma lógica)
- NÃO use se a wave anterior teve falhas críticas

---

*Created: 2026-06-08 (Loop Batch Reference)*
*Use inside `/loop` to run controlled multi-spec batches.*
