# Rule: Build Validation Truth Gate (stub)

> Consolidated into [.claude/rules/validation-truth.md](./validation-truth.md). This stub preserves the historical path referenced by docs/ (ADRs, validation reports).

**Invariant:** Build success is valid only when the process exit code is 0. Never infer build success from filtered output (no dotnet build piped to Select-String).

