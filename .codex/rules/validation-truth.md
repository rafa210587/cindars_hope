# Rule: Verdade na Validação

**1. Build com sucesso = exit code 0.** Nunca infira sucesso de output filtrado. Uma spec só pode ser `BUILD_VALIDATED` quando `run_strict_validation.ps1` retornou exit code 0. Não faça commit se qualquer check falhou ou ficou desconhecido.

**2. Falha de script PowerShell nunca é secundária.** Se qualquer script retorna non-zero ou `$?` = false: PARE imediatamente, sem commit.

**3. Níveis de validação são claims diferentes — nunca os misture.**

| Claim | Significa apenas |
|---|---|
| `dotnet build` PASS | C# fallback compile passou |
| Unity batchmode PASS | Unity compile validation passou |
| Play Mode / manual PASS | gameplay validation passou |

Validação bloqueada → reporte como `NOT RUN or BLOCKED` com motivo e residual risk; nunca converta em PASS.

**4. Sem claims prematuros.** Proibido sem evidência: "MVP accepted", "Play Mode PASS", "Unity validated", "Phase 2/3 PASS". Use: `BUILD_VALIDATED`, `Phase 2 NOT RUN`, `ACCEPTED pending Phase 2-3`.

## Report block obrigatório

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

Hook `pre-bash-guard.ps1` bloqueia pipes de `dotnet build` filtrados.
