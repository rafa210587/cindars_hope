# Rule: No Runtime Global Search (stub)

> Consolidated into [.claude/rules/unity-architecture.md](./unity-architecture.md). This stub preserves the historical path referenced by docs/ (ADRs, validation reports).

**Invariant:** Runtime gameplay code must not use GameObject.Find / FindObjectOfType / FindObjectsOfType / FindObjectsByType. Use serialized refs, bootstrap injection, or explicit configuration.

