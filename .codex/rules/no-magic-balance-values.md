# Rule: Sem Magic Values de Balance (stub)

> Detalhe completo movido para a skill [non-regression-review](../skills/non-regression-review/SKILL.md) — carregada on-demand em /code-review e closeout. Stub preserva path + invariante.

**Invariante:** Valores tuneáveis de gameplay (thresholds, custos, duração, dano, multipliers) devem viver em ScriptableObjects de balance ou `const` nomeada — nunca literais inline em métodos ou condicionais.
