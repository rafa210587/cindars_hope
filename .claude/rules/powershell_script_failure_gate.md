# Rule: PowerShell Script Failure Gate (stub)

> Consolidated into [.claude/rules/validation-truth.md](./validation-truth.md). This stub preserves the historical path referenced by docs/ (ADRs, validation reports).

**Invariant:** A validation script failure (exception, non-zero exit, $? = false) is never secondary. Stop immediately; do not commit.

