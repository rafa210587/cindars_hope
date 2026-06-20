# Rule: Comunicação de Gameplay Apenas via Event Bus (stub)

> Consolidated into [.claude/rules/unity-architecture.md](./unity-architecture.md). Este stub preserva o caminho histórico referenciado por docs/ (ADRs, validation reports).

**Invariante:** Sistemas de gameplay se comunicam apenas via GameEventBus.Publish()/Subscribe(). Chamadas diretas de MonoBehaviour para MonoBehaviour em gameplay são proibidas.
