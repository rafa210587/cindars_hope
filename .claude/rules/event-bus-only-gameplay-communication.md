# Rule: Event Bus Only for Gameplay Communication (stub)

> Consolidated into [.claude/rules/unity-architecture.md](./unity-architecture.md). This stub preserves the historical path referenced by docs/ (ADRs, validation reports).

**Invariant:** Gameplay systems communicate only via GameEventBus.Publish()/Subscribe(). Direct MonoBehaviour-to-MonoBehaviour gameplay calls are prohibited.

