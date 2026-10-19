# Serena integration review — 2026-09-12

Status: ENABLED — scoped integration checks passed. Not a global project validation.

The orchestrator inspected `unity-probe-raw.txt`: initialization 27.789 s,
four read tools, four successful production-symbol queries. These timings belong to
the pre-hardening launcher, not the final integration or a token benchmark.

Independent source check: `rg -n 'StableHash32\.Compute|GameplayInputBlocker\.Acquire' Assets/_Game/Scripts`.
All six textual occurrences match the returned locations after converting zero-based
LSP locations to one-based source lines: EnemyActionExecution 141/143, EliteAffix 82/108,
CraftingPassiveRngState 88, FarmPlotMenuController 132. EliteAffix 82 is XML cref;
the StableHash result has four invocations, not five callers. This spot check does not
prove completeness for every symbol, generated source or Unity configuration.

Independent static reviewer identified unchecked snapshot mutation and mutable metadata
DLLs absent from drift detection. Root also identified path escaping, temporary config
dependency, missing bounds/timeouts and subprocess cleanup. Enablement waits for fixes
and applicable evidence; a successful query alone is insufficient.

Canonical context routing was updated conditionally and regenerated through
`pwsh -NoProfile -File tools/codex/Generate-CodexHarness.ps1`: exit 0, 78 skills,
16 commands, 11 agents, 25 rules, no frontmatter/copy failures.

Unity Editor validation: NOT APPLICABLE (tooling only; no gameplay validation claim).
Existing global documentation failures remain separate from this scoped integration.

## Guard verification

Root executed durable-venv Python `tools/serena/test_guard.py`: exit 0, six groups PASS:
relative-path checks; original source drift; snapshot source drift; snapshot project drift;
actual metadata DLL drift; actual analyzer DLL drift. Existing snapshot destination reuse
was rejected in every filesystem group. These are independent synthetic filesystem fixtures,
not source mutation in the live game or semantic LSP evidence.

An earlier live-source guard run failed its CURRENT assertion. Inspection identified
concurrent changes to QuestRuntimeBootstrap.cs and QuestService.cs relative to that snapshot.
No changes were reverted. This was a legitimate drift rejection; deterministic fixtures
were then used to keep the negative tests independent of other tasks.

The independent reviewer reinspected the final guard implementation and reported no
remaining critical static blocker in the scoped, trusted-repository navigation use case.
This is not an OS sandbox for executing hostile MSBuild projects or protection against
arbitrary external processes. Referenced SDK/Unity tooling remains a trusted prerequisite.

Protocol evidence was inspected directly: valid tools/list and normal response; denied
tool/path never forwarded; modified snapshot during a query rejected; hanging backend
timed out; oversized output rejected with an explicit RPC error after the correction.
The earlier oversized-output case without RPC error remains in the append-only evidence,
alongside the corrected PASS. Fake backend tests exercise the real proxy, not LSP semantics.

Final implementation SHA-256:

- serena_unity_proxy.py: AC01C584F8FA47D0935290E1E31549ADF6988CD2EC8C7442AE62A7935BDC618F
- Sync-UnitySnapshot.ps1: EA6ADDECD4AF0129F06E2E682ACD5E3CB355F30C8DEE57241C35AFDA301F70CE
- test_guard.py: 212A6C0A0AF309641337D19F641A20AFB8FAC2161BCE719BDF750F928CB2528D
- test_protocol.py: EECAA1E59A0BF316C50617A1B64728EFE333AC456B22A6BB126480AB98BFD987

PowerShell parser: zero errors. Scoped tracked diff whitespace check: exit 0.

## Closeout

AC01: PASS on the real captured Unity graph with the final launcher; root inspected
`unity-probe-frozen-source-raw.txt`: four tools and four non-error query results.
The snapshot's 1,390 sources and three project copies were checked against its manifest
before reuse. Original live-checkout freshness is a separate guard, not inferred from this run.
The raw includes a Serena instruction banner starting with `CRITICAL:` that refers to
an excluded onboarding tool; it is not a project-load diagnostic and was not executed.

AC02: PASS in the bounded tests described above; live drift was also observed and blocked.
AC03: PASS in scoped code review and probes; zero matching Python/Serena/dotnet backend
processes remained at the final process check. No runtime, source project or global config
edits were made by this integration. External snapshots are retained, not automatically deleted.
AC04: PASS for local configuration discovery: `codex mcp get serena_unity_readonly --json`
returned enabled=true, four allowed tools, no disabled reason. Native tools in this already
running task did not hot-reload; a new project task is required.

`run_strict_validation.ps1 -Gates docs`: FAIL, 173 global documentation errors, zero errors
mentioning Serena. This is not the earlier 178-count baseline: other tasks are active;
no reduction is attributed to this delivery. No global PASS or Unity acceptance is claimed.
Spec status: CODE_COMPLETE with scoped integration evidence; promotion NO, global documentary
acceptance remains outside this delivery. No feature/runtime or global-document repair added.

Read-only navigation is ready for new project sessions. Use rg when unavailable, after drift,
or when the 20–32 s measured query cost is not justified. Token savings remain unmeasured.
