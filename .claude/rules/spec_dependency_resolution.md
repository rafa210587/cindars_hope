# Rule: Resolução de Dependência de Spec

## Regra Central

Quando uma spec depende de outra **spec não resolvida na mesma wave**, resolva a cadeia **automaticamente**: não pergunte ao usuário, não pivote aleatoriamente, não marque BLOCKED final a menos que a dependência seja forbidden/out-of-scope.

## Algoritmo

1. Marque a spec atual como `BLOCKED_BY_DEPENDENCY_PENDING` (temporário — nunca uma falha final).
2. Empilhe a dependência no dependency stack; recurse até a raiz (depth-first).
3. Execute a raiz primeiro via `/execute-spec-strict`, depois suba de volta pela cadeia.
4. **Return-to-origin:** depois que a cadeia se resolve, execute a spec-alvo ORIGINAL antes de qualquer outra coisa. Nunca pivote para specs não relacionadas enquanto uma cadeia está aberta.

## Pare imediatamente (`BLOCKED_BY_FORBIDDEN_SCOPE`) se a dependência for

wave futura/mapeada (WAVE 06+), pets, HOLD, BLOCKED_SCOPE, ou exigir: Packages/, ProjectSettings/, criação ou edição de scene/prefab/asset, Unity Test Runner, Play Mode.

## Extração de dependência

Leia estas seções da spec: `Depends on`, `Dependencies`, `Required systems`, `Required specs`, `Blocks`, `Permissions`, e acceptance criteria que nomeiam sistemas/specs.

Statuses de dependência que permitem continuar: `READY` (resolver primeiro), `BUILD_VALIDATED[_WITH_WARNINGS]` (usar resultado), `CONTRACT_ONLY[_NEEDS_INTEGRATION]` (condicional — specs fundacionais devem esperar). Taxonomia completa: `.claude/rules/spec_quality_gate.md`.

## Artefatos obrigatórios (persistir estado entre invocações)

- `docs/validation/WAVE_<wave>_DEPENDENCY_RESOLUTION_PLAN.md` — tabela: Order | Spec | Depends On | Reason | Status | Commit
- `docs/validation/WAVE_<wave>_BATCH_STATE.md` — stack atual, executadas neste batch, specs pendentes, próxima ação

## Cláusula obrigatória nos execution reports

```text
## Dependency Chain
Original target: <spec>
Dependency chain: 1..N with statuses
Forbidden dependencies: [none | listed]
Resolved depth: <N>
Plan file / Batch state: paths above
Can continue original target: YES/NO
```

---

*Updated: 2026-06-12 (condensed — full prior text in git history). Applies to /loop-spec-batch-strict and /execute-spec-strict.*
