# Cindar's Hope Agent Rules

Rules are project invariants (the *what must not drift*). Skills explain workflows; hooks enforce mechanically. Consolidated 2026-06-12 — absorbed rules remain as stubs to preserve historical links from docs/.

## Active Rules

**Validation & Spec Lifecycle**
1. [Validation Truth](./validation-truth.md) — exit code 0 or it didn't pass; script failure is never secondary; no premature acceptance claims; honest validation levels
2. [Spec Quality Gate](./spec_quality_gate.md) — canonical status taxonomy; BUILD_VALIDATED checklist; promotion requires evidence; loop batch policy
3. [Spec Dependency Resolution](./spec_dependency_resolution.md) — auto-resolve same-wave chains; return-to-origin; forbidden scopes
4. [Testing Quality Gate](./testing-quality-gate.md) — code changes require automated tests, Play Mode scenario, or documented residual risk

**Code Architecture**
5. [Unity Architecture Invariants](./unity-architecture.md) — no runtime global search; GameEventBus only; save DTOs simple types + IDs; forbidden namespaces
6. [Cave Stable Run](./cave-stable-run.md) — FASE9F stable-run contract for cave procedural

**Unity, Git & Environment Safety**
7. [Unity Assets & Editor Safety](./unity-assets.md) — no manual YAML; generated asset evidence; no parallel batchmode
8. [No Unsafe Git](./no-unsafe-git.md) — destructive git requires per-instance human authorization (enforced via permissions.ask)
9. [Windows / PowerShell Only](./windows_powershell_only.md) — PowerShell syntax; exit code checks; env failure policy

**Context & Docs**
10. [Context Reading Policy](./context-reading-policy.md) — minimal context; no PROJECT_LOG by default
11. [Docs Governance](./docs-governance.md) — canonical paths only; no delete without candidate; ADRs/game_rules canonical

**Code Craft, Resilience & Safety** (general engineering quality — complements the architecture invariants; reviewed, not all hook-gated)
12. [C# Style & Craft](./csharp-style.md) — naming/immutability; failure signaling via `bool`+`FailureReason` (no parallel `Result<T>`); collections; async/frame-cost
13. [Error Handling & Resilience](./error-handling-resilience.md) — four-category failure taxonomy (expected gameplay / config-asset / infra / bug-invariant); logs with context; fail fast in dev
14. [Gameplay Design Pattern Selection](./gameplay-design-patterns.md) — which pattern to reach for, mapped to existing precedents; domain in pure C#, engine as adapter
15. [Security & File Safety](./security-and-files.md) — no secrets in code/configs/logs; sensitive files & real saves not edited without explicit request; read build/deploy scripts fully first

## Mechanical Enforcement (hooks in .claude/settings.json)

| Hook | Event | Enforces |
|------|-------|----------|
| `pre-bash-guard.ps1` | PreToolUse (Bash/PowerShell) | no parallel Unity batchmode; no filtered `dotnet build` |
| `protected-path-guard.ps1` | PreToolUse (Edit/Write) | docs_old/, legacy paths, root specs/, tests outside Tests/EditMode |
| `guard-secrets.ps1` | PreToolUse (Edit/Write) | blocks writing real secrets (cloud keys, private keys, provider tokens, hardcoded credentials) — rule: security-and-files |
| `guard-large-files.ps1` | PreToolUse (Edit/Write) | blocks a single Write > 1 MB (accidental data/log/blob dumps) |
| `runtime-code-guard.ps1` | PostToolUse (Edit/Write) | forbidden search APIs, forbidden namespaces, duplicate class names |
| `detect-change-scope.ps1` + `stop-summary-check.ps1` | Stop | change-scope snapshot + adaptive closeout checklist; blocks stop on forbidden paths |
| `permissions.ask` (settings.json) | — | per-instance human authorization: unsafe git, .unity/.prefab/.asset edits, Packages/, ProjectSettings/ |

Manual hooks invoked by commands (not auto): `spec-promotion-guard`, `delete-guard`, `docs-status-honesty-check`, `test-scenario-required-guard`, `run-required-validations`, `check-csproj-includes`, `check-runtime-forbidden-search` (full-diff version), `check-cave-stable-run-scope`, `context-policy-check`, `decision-rule-reference-guard`, `post-edit-docs-validate`.

## Application

- Apply these rules to every agent-run task in this repository.
- If a task conflicts with a rule, pause and request explicit human authorization.
- If a rule is intentionally bypassed, record the reason in `PROJECT_LOG.md` and the relevant `docs/validation/*.md`.
