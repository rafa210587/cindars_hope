# Rule: Estilo & Craft em C# (stub)

> Detalhe completo movido para a skill [non-regression-review](../skills/non-regression-review/SKILL.md) — carregada on-demand em /code-review e closeout. Stub preserva path + invariante.

**Invariante:** Naming de domínio explícito, sinalização de falha via `bool`+`FailureReason` sem `Result<T>` paralelo, sem allocation/LINQ em hot paths de Update/FixedUpdate.
