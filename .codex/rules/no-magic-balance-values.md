---
paths:
  - "**/*.cs"
---
# Rule: Sem Magic Values de Balance (stub)

> Detalhe completo movido para a skill [non-regression-review](../skills/non-regression-review/SKILL.md) — carregada on-demand em /code-review e closeout. Stub preserva path + invariante.

**Invariant:** Tunable gameplay/content values (thresholds, costs, durations, damage, multipliers)
belong in existing balance ScriptableObjects. Named constants are for fixed technical invariants,
not a workaround for content authoring. Preserve legacy values until an approved migration;
do not create a new SO for each number or silently rebalance during refactoring.
