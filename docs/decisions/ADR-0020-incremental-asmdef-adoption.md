---
doc_type: adr
status: proposed
adr_id: ADR-0020
title: Incremental Assembly Definition (.asmdef) Adoption
date: 2026-07-03
source_documents:
  - .specs/a_implementar/spec_codex_08_convergence_decisions.md
supersedes: []
superseded_by: []
applies_to:
  - build-pipeline
  - compile-time
  - project-structure
---

# ADR-0020 — Incremental Assembly Definition (.asmdef) Adoption

## Status

**proposed** (draft — requires human decision before any implementation)

## Context

Reconfirmed 2026-07-03 via `Get-ChildItem -Recurse -Filter *.asmdef` under `Assets/`: **zero**
`.asmdef` files exist in the project today. All C# code compiles into the two implicit Unity
assemblies, `Assembly-CSharp` (runtime) and `Assembly-CSharp-Editor` (editor), exactly as the
`run_strict_validation.ps1` script and every validation report in `docs/validation/` already
assume (`dotnet build .\Assembly-CSharp.csproj` / `.\Assembly-CSharp-Editor.csproj`).

As the codebase has grown (thousands of files across `Assets/_Game/Scripts/**`), the implicit
assembly model means **any single script change forces Unity to recompile the entire
`Assembly-CSharp` assembly**. There is no compiler-enforced dependency boundary between domains
(e.g. `Cave/`, `Farm/`, `Combat/`, `Editor/`) — a `Core` type can be referenced from anywhere, and
nothing prevents an `Editor/` script from silently depending on gameplay-only code or vice versa
beyond folder convention and code review.

## Decision (proposed, not accepted)

Introduce `.asmdef` files **incrementally**, starting with the assemblies that already have the
smallest surface and the clearest existing folder-level segregation:

1. **Phase 1 (lowest risk):** `Editor/` → `CindarsHope.Editor.asmdef` (editor-only, already
   physically isolated under `Assets/_Game/Scripts/Editor/**`, already compiled separately by
   Unity's implicit Editor-folder rule — an explicit `.asmdef` here mainly gains cross-references
   control and per-assembly compile timing, not a new boundary).
2. **Phase 2 (medium risk):** `Tests/EditMode/` → `CindarsHope.Tests.EditMode.asmdef` (test-only,
   already the only directory the `protected-path-guard.ps1` hook treats as the canonical test
   location; would also let `dotnet test`/Unity Test Runner target it without touching the runtime
   assembly boundary).
3. **Phase 3 (highest risk, NOT recommended without a dedicated spec):** splitting gameplay code
   itself (`Core`/domain assemblies) — this requires resolving every current cross-domain
   `using` dependency explicitly and would very likely surface hidden coupling that today compiles
   silently inside one assembly.

### Alternatives considered

- **Do nothing (status quo):** zero migration cost, but incremental compile time keeps degrading
  as the project grows; no compiler-enforced boundaries.
- **Full split now (Core/Farm/Combat/Cave/UI/Editor/Tests):** best long-term compile time and
  boundary enforcement, but highest one-shot risk — every `run_strict_validation.ps1` script,
  every `.csproj` reference, and CI/build tooling that assumes exactly two assemblies would need
  simultaneous updates, and hidden cross-domain coupling would surface as compile errors across
  the whole codebase at once.
- **Editor + Tests only (this proposal):** captures most of the safety win (isolating
  editor-only and test-only code, which by convention already do not leak into runtime gameplay)
  with the least risk of a big-bang breakage.

## Cost

- Editor-only asmdef: low — mechanical, folder boundary already exists; must audit for any
  `Editor/` script referenced from runtime code (would need to move or expose differently).
- Tests asmdef: low-medium — must audit all 60+ EditMode tests' `using` dependencies on
  non-test code; some may need public API surface, not `internal`.
- Any deeper split: high — requires a dedicated spec, dependency graph audit, and re-verification
  of `run_strict_validation.ps1` and all CI-adjacent tooling.

## Risk

- Unity recompiles more aggressively on `.asmdef` boundary changes than on typical script edits;
  a botched first attempt can produce confusing "type not found" errors that look like real bugs.
- `run_strict_validation.ps1` and `check-csproj-includes` hook assume the current two-assembly
  model; introducing new assemblies requires updating those scripts in the same change, or the
  gate becomes silently stale (violates `validation-truth`).
- Existing `.csproj` files (`Assembly-CSharp.csproj`, `Assembly-CSharp-Editor.csproj`) are
  Unity-generated; adding `.asmdef` changes what Unity generates and must be re-verified.

## Requer decisão humana

**SIM** — motivo: introducing any `.asmdef` is a structural build-pipeline change that affects
every future compile and the existing validation scripts (`run_strict_validation.ps1`,
`check-csproj-includes` hook). It also changes what "the codebase compiles" means going forward.
No `.asmdef` should be created without the human choosing a phase (Editor-only vs. Editor+Tests vs.
full split) and approving the validation-script updates that must accompany it.

## Source Documents

- [spec_codex_08_convergence_decisions.md](../../.specs/a_implementar/spec_codex_08_convergence_decisions.md)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
