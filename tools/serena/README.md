# Serena Unity read-only navigation

Status: automatic startup disabled by user request in the project-local `.codex/config.toml`.
Cost non-regression remains unproven. Installation, configuration and test evidence are retained;
new project tasks should not start this server automatically. No global settings were changed.
Earlier enablement reports are historical; this status supersedes their enablement decision.
Manual benchmark scripts can still start Serena and consume resources; disabling automatic
startup does not authorize or launch another experiment.

Latest end-to-end investigation pair: Serena increased native input tokens by 71.0%
(uncached input +70.5%) and total wall time by 3.05x; output fell 16.4%, with no
material answer-quality gain. This sample does not support cost-based enablement.
See `docs/validation/serena_unity_20260912/agent-cost-b6572ee1ed/REVIEW.md` for raw
usage, quality limitations, read-only preflight and the single-pair/cache caveats.

Optional pilot for `CindarsHope.Foundation`, `CindarsHope.Gameplay`, and `CindarsHope.Runtime` only. The launcher `serena_unity_proxy.py` starts a new private session under `D:/Projetos/Cindars_Hope/.serena-runtime/s/`, copies the 1,390 explicit `Compile` sources byte-for-byte, rewrites only snapshot `.csproj` DLL paths to absolute references, restores within that external snapshot, and serves four read-only MCP tools. The original Unity project is never restored or built by this launcher.

Launch with the durable Python environment:

`D:/Projetos/Cindars_Hope/.serena-runtime/venv/Scripts/python.exe D:/Projetos/Cindars_Hope/cindars_hope/tools/serena/serena_unity_proxy.py --repo D:/Projetos/Cindars_Hope/cindars_hope --runtime-root D:/Projetos/Cindars_Hope/.serena-runtime`

`roslyn-cache` and `venv` must already exist in the runtime root; startup never installs them. The proxy hashes original source, `.csproj`, and referenced DLL files before each tool call, then verifies snapshot copies and original input again before forwarding the result. Any drift fails closed: restart the MCP session for a new snapshot. While unavailable, use `rg` against the live repository; do not treat a snapshot answer as current after an edit. A session is private, so concurrent agents do not mutate each other's LSP workspace. Snapshots and caches are external retained evidence, not Unity assets.

This is navigation evidence, not Unity compile/Play Mode validation. Serena references include XML `cref`; callers-only questions require filtering. Source lists are limited to the three assemblies above. See `docs/validation/serena_unity_20260912/REPORT.md` for the bounded pilot and residual risks.

Prefer `rg` for simple searches. On the final captured-source probe, startup took 27.6 s
and queries took 20–32 s including integrity checks; token savings were not measured.
Use this optional navigator when semantic reference resolution justifies that latency.
The proxy caps backend messages at 30,000 bytes and a pending tool call at 90 s;
Serena's default answer bound is 12,000 characters. A blocked result requires a new session.
There is no automatic snapshot cleanup; retained sessions consume disk outside the project.
SDK/tooling and project definitions are trusted prerequisites, not sandboxed hostile code.

Checks: durable Python with `tools/serena/test_guard.py` (filesystem fixtures),
`tools/serena/test_protocol.py` (fake backend, real proxy), and `tools/serena/test_unity_probe.py`
(real LSP). The latter accepts `--repo` for a captured stable source graph. See
`docs/validation/serena_unity_20260912/ORCHESTRATOR_REVIEW.md` for integration closeout.
