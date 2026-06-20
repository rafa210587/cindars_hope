# Rule: Verdade na Validação

Consolida: `build_validation_truth_gate`, `powershell_script_failure_gate`, `unity-validation-honesty`, `no-premature-acceptance-claims` (os originais são stubs apontando para cá).

## 1. Build com sucesso = exit code 0. Nada mais.

Nunca infira sucesso de build a partir de output filtrado.

```powershell
# FORBIDDEN (loses $LASTEXITCODE, hides errors) — pre-bash-guard hook blocks this:
dotnet build ... | Select-String "error"

# REQUIRED:
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

# PREFERRED (runs everything, checks every exit code, writes JSON artifact):
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

Uma spec não pode ser `BUILD_VALIDATED` a menos que `run_strict_validation.ps1` tenha retornado exit code 0. Não faça commit se qualquer build/check falhou, ficou desconhecido, ou foi verificado apenas via output filtrado.

## 2. Uma falha de script PowerShell nunca é secundária

Se qualquer script de validação lança exceção, imprime uma exceção, retorna non-zero, ou tem `$?` = false: **PARE imediatamente**. Sem "secondary issue", sem "proceed anyway", sem commit. Cheque tanto `$?` quanto `$LASTEXITCODE` depois de cada script; envolva em `try/catch` e trate o catch como falha.

## 3. Níveis de validação são claims diferentes — nunca os misture

| Claim | Significa apenas |
|---|---|
| `dotnet build` PASS | C# fallback compile passou |
| Unity batchmode PASS | Unity compile validation passou |
| Play Mode / manual PASS | gameplay validation passou |

Validação bloqueada é reportada, nunca convertida em PASS:

```text
Unity validation: NOT RUN or BLOCKED
Reason: <lock | license | sandbox | timeout | approval>
Command attempted: <command>
Residual risk: <explicit>
```

Triagem: qualquer `error CS` é real a menos que provado stale; "another Unity instance running" é um editor lock, não uma falha de código.

## 4. Sem claims prematuros de aceitação

Nunca escreva sem evidência no repo: "MVP accepted", "100% fulfilled", "Play Mode PASS", "Unity validated" (genérico), "Phase 2/3 PASS", "Human acceptance complete".

Alternativas honestas: `Phase 0-1 COMPLETE`, `BUILD_VALIDATED`, `Phase 2 NOT RUN`, `Phase 3 PENDING`, `ACCEPTED pending Phase 2-3`.

## Required report block

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS | FAIL
Assembly-CSharp-Editor: PASS | FAIL
Quality check: PASS | FAIL
Docs validation: PASS | EXPECTED_FAIL_LEGACY_ONLY | NEW_FAILURE
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Enforcement

- Hook `pre-bash-guard.ps1` (PreToolUse) bloqueia pipes de `dotnet build` filtrados.
- `tools/docs/check_spec_quality.ps1` detecta frases de claim proibidas nos reports.

*Incidentes históricos que motivaram esta rule: commits fcfe6d0/53e9698 (build filtrado escondeu um syntax error) e d053a29 ("secondary issue" commitou código não validado). Detalhes no git history.*
