# /implement-spec (DEPRECATED — use /execute-spec-strict)

> **DEPRECATED 2026-06-12.** Este command foi substituído por `/execute-spec-strict`, que cobre o mesmo fluxo com gates de validação mais rigorosos (run_strict_validation, status taxonomy, dependency resolution). Manter dois caminhos de execução permitia escolher o mais frouxo.

**Arguments:** `$ARGUMENTS` — spec number or filename

## Ação

Execute exatamente o fluxo de `/execute-spec-strict` com o mesmo argumento:

1. Leia `.claude/commands/execute-spec-strict.md` e siga-o integralmente.
2. Aplique `.claude/rules/spec_quality_gate.md` (status taxonomy + checklist BUILD_VALIDATED).
3. Não auto-promova spec para `implementados/` — closeout é phase-gated via `/finish-spec`.

A versão íntegra do antigo `/implement-spec` está no git history, caso precise de referência.
