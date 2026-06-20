# Rule: Sem Busca Global em Runtime (stub)

> Consolidated into [.claude/rules/unity-architecture.md](./unity-architecture.md). This stub preserves the historical path referenced by docs/ (ADRs, validation reports).

**Invariante:** Código de gameplay em runtime não pode usar GameObject.Find / FindObjectOfType / FindObjectsOfType / FindObjectsByType. Use serialized refs, bootstrap injection ou configuração explícita.
