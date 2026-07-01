# Rule: Resolução de Dependência de Spec (stub)

> Detalhe completo movido para a skill [spec-execution](../skills/spec-execution/SKILL.md) — carregada on-demand em execução/closeout de spec. Stub preserva path + invariante.

**Invariante:** Dependência same-wave resolve-se automaticamente depth-first com return-to-origin; só marca `BLOCKED_BY_FORBIDDEN_SCOPE` se a dependência for forbidden/out-of-scope.
