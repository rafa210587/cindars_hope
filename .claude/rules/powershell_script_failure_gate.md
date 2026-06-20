# Rule: Gate de Falha de Script PowerShell (stub)

> Consolidated into [.claude/rules/validation-truth.md](./validation-truth.md). This stub preserves the historical path referenced by docs/ (ADRs, validation reports).

**Invariante:** A falha de um validation script (exception, exit não-zero, $? = false) nunca é secundária. Pare imediatamente; não faça commit.
