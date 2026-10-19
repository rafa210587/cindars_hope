# Semantic navigation pilot — execução E7

Status: PARTIAL as a production navigator; isolated prototype and fixture contracts PASS. No Unity validation requested or run. No install/network/cache/project or asset edit.

## Acceptance criteria extracted

- AC01: semantic binding and overload/partial/alias/extension/conditional fixture coverage — PASS (13 query cases with multiple position/kind assertions; `Test-SemanticPilot.ps1`, exit 0).
- AC02: invalid inputs and truncation never assert complete — PASS for missing metadata, compile error, unsupported condition/SDK, constructor target, unresolved target, and MaxResults truncation.
- AC03: bounded JSON with total/omitted counts and input hash — PASS by fixture, no query writes. Hash includes source, csproj, metadata, wrapper and helper bytes plus defines. No persistent cache.
- AC04: read-only probe of generated project/consumer — PARTIAL: exact call locations observed but generated projects carry analyzers/source generators not evaluated by pilot, so `complete=false` and exit 2.

## Existing systems audit

`Get-ClassContext.ps1` retained unchanged. It supplies syntax declarations, not references. New script implements compilation-backed reference pilot in isolated folder.

## Spec Compliance Matrix

| Requirement | Implementation/evidence | Status |
|---|---|---|
| Explicit csproj graph, metadata and defines | `SemanticPilot.cs` loader; fixture | OK within bounded contract |
| FQ doc-ID target / overloads | Roslyn declaration ID resolution; int/string fixtures | OK |
| Fail closed on errors, unsupported inputs, truncation | diagnostics + exit 2; fixture | OK |
| Real consumer probe | Runtime query; four matching invocations | DEFERRED as complete caller set because analyzers unsupported |

## Validation

As sondagens de 8555/1029 ms abaixo são anteriores ao hardening do review, não evidência do loader final. Probe final do orquestrador: 8571,1 ms, 3131 bytes, exit 2, quatro invocações e 12 diagnósticos; detalhes e inputHash em [SECOND_TRANCHE.md](../SECOND_TRANCHE.md). Testes finais: 13 casos PASS também conferidos pelo orquestrador. Revisão independente: quatro findings CLOSED.

Command: `pwsh -NoProfile -File tools/architecture/semantic-pilot/Test-SemanticPilot.ps1`. Result: 13 query cases PASS, exit 0. Wrapper requires PowerShell 7 and an installed .NET SDK reference pack matching the PowerShell runtime major version. Fixture project uses installed .NET 10 reference pack (`System.Runtime.dll`) at the recorded host path; this is a local fixture dependency, not a general compiler reference-pack inference.

Real command: `pwsh -NoProfile -File tools/architecture/semantic-pilot/Get-SemanticReferences.ps1 -Project CindarsHope.Runtime.csproj -Symbol 'M:CindarsHope.Foundation.StableHash32.Compute(System.String)' -MaxResults 20`. Wall time including startup: 8555 ms. Exit 2, `complete=false`, 4 matches, 3 diagnostics (unsupported analyzers in Foundation/Gameplay/Runtime), 0 omitted. Matches: `EliteAffix.cs:108`, `EnemyActionExecution.cs:141,143`, `CraftingPassiveRngState.cs:88`; each `kind=invocation`. `EliteAffix.cs:82` XML cref was not counted. Foundation-only query: 1029 ms, exit 2, zero matches and one unsupported-analyzer diagnostic. No Unity build/compile asserted.

## Honest status rationale

Fixture validity supports a small explicit-csproj configuration. Generated Unity projects remain incompatible with a complete query because analyzers/source generators are intentionally unsupported. The real hits are useful candidates for manual review, not a complete caller set. `complete=true` is scoped solely to explicit bound `SimpleName` references in the chosen project graph/defines. Implicit/pattern/dynamic references, `using` directives, reflection, serialized IDs, GameEventBus and projects outside that graph are excluded; constructor/operator/conversion/indexer targets fail closed. No package/MSBuild compatibility or performance benefit over `rg` is claimed. Unity validation: NOT RUN; reason: tooling-only and no Assets changes; command attempted: none; residual risk: Unity compilation not validated locally.
