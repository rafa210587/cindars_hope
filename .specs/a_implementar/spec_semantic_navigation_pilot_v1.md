# Semantic navigation pilot v1

Spec ID: `spec_semantic_navigation_pilot_v1` · Wave: efficiency E7 · Type: Tooling · Domain: architecture navigation · Priority: pilot · Parallelizable: YES, isolated lock · Executor: tooling owner.

Authorization: human request of 2026-09-12 to continue the efficiency queue ("siga"), delegated as E7 semantic navigation pilot. Status at authoring: approved for isolated tooling execution, not accepted as a production navigator.

Ordem de execucao: E7 da fila de eficiência; não modifica a ordem das waves.
Depende de: contrato SEMANTIC_PILOT, PowerShell 7/Roslyn e reference pack .NET compatível instalado.
Bloqueia: somente a decisão de adoção do piloto; nenhuma wave de gameplay.
required_adrs: []
required_game_rules: []

ADR/game-rule dependencies: none; isolated development tooling, with no gameplay or architecture contract changes.
Closeout status: PARTIAL for production, experimental only; evidence in `docs/validation/efficiency_20260912/SECOND_TRANCHE.md`. No physical promotion.

# /speckit.specify

Implement a read-only, non-cached semantic reference query in `tools/architecture/semantic-pilot/**`. Use the Roslyn assemblies bundled with PowerShell 7 and explicit generated `.csproj` inputs. No Unity build, installation, package, asset, project, or existing `Get-ClassContext.ps1` edit. The pilot is not a full MSBuild evaluator. `docs/validation/efficiency_20260912/semantic/**` holds fixtures and evidence.

## Contract

Entry point: `pwsh -File tools/architecture/semantic-pilot/Get-SemanticReferences.ps1 -Project <path.csproj> -Symbol <fully-qualified type or doc-id member> [-MaxResults 1..1000]`. The script accepts explicit `Compile`, `Reference` with literal existing `HintPath`, `ProjectReference`, `AssemblyName`, `DefineConstants` and `LangVersion`; rejects unsupported dynamic properties/globs/imported source evaluation rather than guessing. Project references are built recursively as Roslyn compilations. `-Symbol` uses Roslyn documentation IDs (`T:Namespace.Type`, `M:Namespace.Type.Method(System.Int32)`) to distinguish overloads; no simple-name fuzzy matching. Output is one JSON object with `status`, `complete`, `assembly`, `symbol`, `configuration`, `inputHash`, `matches`, `totalMatches`, `omittedMatches`, `diagnostics`, and `limitations`. Match includes relative path, line, column, reference kind and bound symbol ID. Deterministic path/position sort. No file writes by query; idempotent for equal inputs. Exit 0 only for complete, untruncated query; exit 2 for invalid/incomplete/truncated; exit 1 for invocation/internal error. Incomplete output must never claim a complete caller set.

Compilation errors, unavailable metadata, unresolved/ambiguous target, unsupported csproj constructs, and truncation set `complete=false`, with explicit diagnostics and counts. Reflection, serialized IDs and GameEventBus events are out of scope. Symbol references include type mentions and member accesses/invocations, but not declarations.
`complete=true` means complete only for explicitly bound `SimpleName` references within the selected project-reference graph and declared configuration, never the entire workspace or all Unity configurations. Implicit/pattern/dynamic references and `using` directives are excluded; constructor/operator/conversion/indexer targets fail closed. This pilot requires PowerShell 7 plus an installed .NET SDK reference pack matching the PowerShell runtime major version for compiling the isolated helper; no installation is authorized.

## Phase 0 / reuse

`Get-ClassContext.ps1` already provides syntax declarations and hashes, explicitly no semantic references. Generated Foundation, Gameplay and Runtime projects expose explicit source/metadata/project references; inspected HintPaths existed on 2026-09-12, but compilation success is unproven. Pilot must not clone or change that script.

# /speckit.plan

Isolate compilation-backed discovery from the existing syntax navigator. Use the explicit project graph and fail closed on unsupported evaluation; collect diagnostics before considering completeness. Keep fixtures outside Unity and require an independent review of the completeness contract.

# /speckit.tasks

1. Create the isolated query script and helper. Parse only the bounded project contract; hash bytes of all source/csproj/reference inputs plus analyzer version and defines. Build compilations dependency-first, collect all Error diagnostics before reporting complete. Resolve documentation ID to one symbol in the selected project or referenced compilations. Walk bound syntax references using semantic model and original-definition equivalence, including reduced extension methods and aliases.
2. Create tiny fixtures for partial, alias, overload, extension, `#if`, missing reference, syntax/compile error and truncation. Tests assert exact match positions/counts where stable and always assert `complete=false` on invalid inputs.
3. Probe a small real Foundation/Gameplay project and, if practical, a Runtime consumer. Record command, elapsed wall time, result, limitations and hashes in an individual report. Do not infer Unity compile from this probe.

## Acceptance / failure semantics

- AC01: fixture valid query gives exact expected references for partial/alias/overload/extension and conditional branches, with stable deterministic JSON and source/configuration hash.
- AC02: missing reference, compile error, ambiguous/unresolved symbol and truncation cannot return `complete=true` or exit 0.
- AC03: output is bounded by `MaxResults`, reports omitted count, and query has no persistent cache/write.
- AC04: real read-only probe and report name actual results, duration and any blockers, without claiming unsupported generated-project compatibility.

Validation: PowerShell parse and fixture contract runner; scoped tooling only. Unity validation: NOT RUN (no Unity/Assets changes). Risks: generated csproj may rely on unsupported MSBuild evaluation or source generators; explicit failure is preferable to false completeness. Repo lock: spec file, `tools/architecture/semantic-pilot/**`, `docs/validation/efficiency_20260912/semantic/**`. Do not alter shared registries, order, csproj, Assets, Packages, ProjectSettings or existing navigator. Depends on: available PowerShell 7 Roslyn. Blocks: no other task. Human acceptance: not required for tooling correctness.
