# Rule: Truth Gate de Validação de Build (stub)

> Consolidated into [.claude/rules/validation-truth.md](./validation-truth.md). Este stub preserva o caminho histórico referenciado por docs/ (ADRs, validation reports).

**Invariante:** O sucesso do build só é válido quando o exit code do processo é 0. Nunca infira sucesso de build a partir de output filtrado (nada de dotnet build com pipe para Select-String).
