# Serena Unity read-only integration

> Status: CODE_COMPLETE — project-local MCP enabled; scoped integration checks PASS; no global acceptance.
> Type: Tooling / local integration
> Authorization: human request "faça" after proposing a real Unity snapshot pilot and conditional project-only read integration, 2026-09-12.
> Ownership: Sol tools/serena and probe evidence; root project config, routing and closeout. Parallelizable only with disjoint files.
> Ordem de execucao: snapshot/probe, guard review, project-only enablement.
> Depende de: installed Serena 1.7.0, Roslyn LSP cache, generated Foundation/Gameplay/Runtime projects and referenced metadata.
> Bloqueia: only adoption of this optional navigator.

required_adrs: []
required_game_rules: []

# /speckit.specify

Provide optional semantic navigation over the actual Unity C# project graph without backend writes in the active checkout. Earlier two-project synthetic fixture passed, but is not production-project evidence. Reuse Serena rather than extend the experimental Roslyn navigator. Sources remain byte-identical in an external snapshot; any project-path rewrites affect copies only. A generated csproj graph is not proof of all Unity configurations or unseen/new sources.

AC01: compare actual StableHash32.Compute and GameplayInputBlocker references across Foundation/Gameplay/Runtime with independently inspected source results; archive queries, tool list, hashes and diagnostics. XML cref is a reference, not automatically a caller.
AC02: only read tools exposed; source/config drift must block stale responses, including drift during a query. Separate session snapshots prevent concurrent mutation. Query bounds and timeout must be explicit; failure falls back to rg, not fake completeness.
AC03: no edits to original Assets, csproj, Packages, ProjectSettings, global Codex config or user worktrees. Backend state/restore stays external. Subprocesses shut down cleanly. No implicit installer during startup.
AC04: enable only project-local MCP after applicable probe/guard checks; Codex configuration parsing/discovery and fresh-session limitations reported. No assertion that a currently running task hot-reloaded tools.

# /speckit.plan

Allowed: tools/serena/**, .codex/config.toml (local preferences preserved by generator), .claude/rules/context-reading-policy.md and its generated copy, docs/architecture/AI_CODE_CONTEXT.md, docs/validation/serena_unity_20260912/**, this spec and its registry entry. External runtime under D:/Projetos/Cindars_Hope/.serena-runtime; reuse already downloaded packages/cache. Prohibited: gameplay/asset/source-project edits, global settings, unrelated cleanup, push/PR/merge. Do not recreate the full Unity asset workspace or use a Git checkout to copy source.

Snapshot builder reads explicit project graph, copies relevant C# and metadata or rewrites copy-only metadata paths. Launcher/proxy checks source identity before/after reads and filters tools. Each session owns its backend state. Exact flags and limitations are documented in tools/serena README after implementation. Root appends an optional project MCP section (required=false), retains all pre-existing preferences, and publishes a short route with fallback. No new mandatory skill or broad planning framework.

# /speckit.tasks

1. Build snapshot/proxy and exercise actual Unity symbols (AC01–03).
2. Check denied tools, drift, limits, isolation and cleanup; independent risk review of guard boundary (AC02–03).
3. Configure scoped MCP only if gates pass; verify parser/discovery and record how next sessions load it (AC04).
4. Update concise documentation, regenerate only affected harness outputs through existing generator, run docs/pertinent contracts and preserve unrelated global failures.

Unity Editor/test execution is NOT APPLICABLE to tooling changes; design-time LSP loading of copied projects does not validate gameplay. Risks: stale generated csproj, missing imports/metadata, Roslyn restore side effects, unsupported source generation, background source changes, cold-start latency. Stop enablement on unresolved reference correctness or isolation failure; retain exact failure rather than silently broadening scope. Acceptance and promotion are decided at closeout, not preset here.

## Closeout

AC01–04: scoped PASS; evidence in `docs/validation/serena_unity_20260912/ORCHESTRATOR_REVIEW.md`.
Enabled for new project tasks; current task not hot-reloaded. Final semantic probe used a
hash-verified captured real graph; separate live-source tests correctly blocked concurrent drift.
Promotion: NO. Global docs gate has 173 unrelated errors (none mentioning Serena); no global
acceptance or Unity validation claimed. No remaining implementation blocker for the optional
read-only navigator. Token savings unmeasured; final queries took 20–32 seconds.
