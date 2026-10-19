# E8 — External feasibility (2026-09-12)

Subsequent authorized execution: [Serena isolated pilot](serena/REPORT.md), [final experiment queue](FINAL_TRANCHE.md). Installation/non-execution statements below describe the research-stage snapshot; consult the pilot report for the versions and outcomes actually observed during execution.

## Decision

| Candidate | Decision | Why |
|---|---|---|
| Serena | **Pilot** (read-only, isolated) | The canonical `oraios/serena` supports C# via Roslyn LSP; this host meets its documented C# prerequisites. Semantic correctness on Unity-generated projects remains unproven and the decision depends on E7's baseline comparison. |
| Stryker.NET | **Defer** | It runs through `dotnet`/VsTest, whereas this repository's tests are Unity Test Framework assemblies. A production `netstandard2.1` csproj is not a runnable .NET test project. First prove one extracted pure-.NET test fixture. |
| GameCI Unity Test Runner | **Candidate for a conditioned CI pilot** | It is the purpose-built Unity test action and supports version detection. It needs a chosen GitHub runner, Docker/image pull and Unity activation; none are validated here. |

## Canonical sources and pins

* **Serena:** canonical repository is [`oraios/serena`](https://github.com/oraios/serena), not a fork. License is MIT ([repository license](https://github.com/oraios/serena/blob/main/LICENSE)); the maintained package is `serena-agent`. Pin the pilot to PyPI **1.7.0**, wheel SHA-256 `6dbf1459670d96fb0595f84932adef34260a6fe14ba5135b901fdb3c8c76e891` ([versioned PyPI release](https://pypi.org/project/serena-agent/1.7.0/)). Do not use moving Git install instructions.
* **Stryker.NET:** canonical [`stryker-mutator/stryker-net`](https://github.com/stryker-mutator/stryker-net), Apache-2.0. Pin **dotnet-stryker 4.14.2** to annotated-tag target `cc7fdf3433025a4b63c2c091c07b2177d117359e` ([release](https://github.com/stryker-mutator/stryker-net/releases/tag/dotnet-stryker%404.14.2)).
* **GameCI:** canonical [`game-ci/unity-test-runner`](https://github.com/game-ci/unity-test-runner), MIT ([README](https://github.com/game-ci/unity-test-runner/blob/main/README.md#license)). Pin **v4.3.2** to `fa6ced25861c16ef56187828c43f76d00df43a23` ([release](https://github.com/game-ci/unity-test-runner/releases/tag/v4.3.2)); avoid mutable `@v4`/`@main`.

## Serena feasibility and read-only boundary

**Documented.** Serena's default C# backend is Roslyn LSP and requires .NET 10+ and, on Windows, PowerShell 7+ ([language support](https://oraios.github.io/serena/01-about/020_programming-languages.html)). It installs via `uv`; package metadata requires Python >=3.11,<3.15 ([installation](https://oraios.github.io/serena/02-usage/010_installation.html)). It can expose an LSP-backed symbol/reference toolset and also editing, shell and file tools ([README](https://github.com/oraios/serena/blob/main/README.md#features)). Config supports `excluded_tools` or a replacement `fixed_tools` list, and project data defaults to `$projectDir/.serena` ([template](https://github.com/oraios/serena/blob/main/src/serena/resources/serena_config.template.yml)).

**Locally verified (read-only inspection):** `dotnet` is 10.0.1126; `pwsh` is 7.6.5; `uv` and Git exist. `serena` is absent. Unity is 6000.5.7f1; generated project/test csproj target `netstandard2.1`, and package manifest declares Unity Test Framework 1.7.0. The existing semantic pilot already establishes Unity metadata references exist, but not semantic compilation or Serena compatibility ([SEMANTIC_PILOT.md](SEMANTIC_PILOT.md)).

**Smallest Serena trial:** after E7 supplies the baseline, authorize an isolated `uv tool install` of the exact wheel/version; configure `project_serena_folder_location` outside the worktree, `trusted_project_path_patterns` for this repository only, and `web_dashboard: false` (all three fields are present in the canonical template). Use `fixed_tools` only after the installed version reveals exact symbol/read/reference tool IDs; omit edit, replace, shell, filesystem-write and memory APIs. Before querying, capture the MCP-exposed-tool list and reject the pilot unless it proves that allowlist. Then test one known C# declaration, cross-assembly reference and partial/overload fixture against E7. Record answers, diagnostics, elapsed time, server logs/downloads and changed-file list; uninstall/retain only after review.

An MCP allowlist limits callable tools, **not** all server-side effects: initialization/indexing may create project data/cache and Serena documentation describes language servers that can download dependencies. Therefore this is only a read-only *candidate configuration* until the pilot verifies paths, process/network activity and a clean worktree. No claim of a side-effect-free Serena mode is made.

## Stryker feasibility

Stryker supports .NET Core/.NET Framework/.NET Standard targets and defaults to the VsTest runner; its MTP runner is documented as preview ([compatibility](https://github.com/stryker-mutator/stryker-net), [runner configuration](https://github.com/stryker-mutator/stryker-net/blob/master/docs/configuration.md#test-runner)). That describes supported target frameworks, not Unity Test Framework execution. Here, tests reference Unity editor/runtime assemblies and are normally launched by Unity, so `dotnet test`/VsTest compatibility is unproven. `dotnet-stryker` is absent.

Trial only after an explicitly created, separately runnable `net8.0`/NUnit fixture containing a small deterministic extracted class and `dotnet test` passes. Run pinned Stryker against that fixture with a tight `mutate` glob and output outside the repository; do not target Unity csproj. Promote only if it produces valid mutation results and adds review value; otherwise retain Unity tests alone.

## GameCI feasibility

The action detects a full project's Unity version from `ProjectSettings/ProjectVersion.txt`, runs EditMode/PlayMode choices, pulls `unityci/editor` by default, and documents configurable CPU/memory/isolation; it uses Node 24 ([action inputs](https://github.com/game-ci/unity-test-runner/blob/main/action.yml)). Unity license-server support is an explicit input; activation credential/licensing choice must be supplied through CI secrets or the approved licensing route, which was intentionally not inspected. Its latest stable-release notes flag Unity 6.6+ shared-memory needs; this project is 6000.5.7f1, so that specific fix is not evidence of a run here ([v4.3.2 notes](https://github.com/game-ci/unity-test-runner/releases/tag/v4.3.2)). Docker exists locally, but no image pull, Docker health check, CI workflow, runner, license or secret was inspected/validated.

Smallest CI pilot, conditional on an approved runner/license/image path: a new non-default GitHub Actions workflow on a disposable branch, `game-ci/unity-test-runner@fa6ced25861c16ef56187828c43f76d00df43a23`, `projectPath: .`, detected 6000.5.7f1, `testMode: editmode`, coverage disabled initially, artifacts retained. Use the approved Unity activation mechanism without exposing values; report only action/Unity exit status and XML artifacts. Add PlayMode and coverage only after EditMode is green.

## Non-claims

No candidate was installed, executed, connected, or granted workspace access. No claim is made about token savings, performance, semantic completeness, test fidelity, licensing entitlement, or CI success.
