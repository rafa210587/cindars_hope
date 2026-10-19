---
doc_type: adr
status: proposed
adr_id: ADR-0025
title: Migrating Resources Folder to Direct References / Addressables
date: 2026-07-03
source_documents:
  - .specs/a_implementar/spec_codex_08_convergence_decisions.md
supersedes: []
superseded_by: []
applies_to:
  - asset-loading
  - resources-folder
  - build-size
---

# ADR-0025 — Migrating `Resources/` to Direct References / Addressables

## Status

**proposed** (draft — requires human decision before any implementation)

## Context

Reconfirmed 2026-07-03 via `Get-ChildItem -Recurse -File | Measure-Object -Property Length -Sum`
on `Assets/_Game/Resources/`: **54.62 MB**, matching the prior audit's 54.6 MB figure.

Everything under a Unity `Resources/` folder is force-included in the build and loaded by string
path via `Resources.Load(...)`, regardless of whether it is actually referenced by any scene or
other asset. This has two costs: (1) build size/startup memory includes every asset in that
folder whether used or not, and (2) references by string path are not compiler-checked — a typo
or a renamed/moved asset silently returns `null` at runtime instead of failing to compile or
import.

Reconfirmed 2026-07-03 via `Packages/manifest.json`: **Addressables (`com.unity.addressables`) is
NOT currently a project dependency.** Any Addressables-based proposal is a new package addition,
not a reuse of existing tooling.

## Decision (proposed, not accepted)

Two-step migration, **step 1 first, step 2 optional and separately gated**:

1. **Step 1 — direct references where possible.** For any asset currently loaded via
   `Resources.Load` that is referenced from a fixed, known context (a specific
   ScriptableObject, prefab, or MonoBehaviour with a serialized field), replace the string-path
   `Resources.Load` call with a direct serialized reference (`[SerializeField]`), following the
   project's existing convention of wiring managers/data through `GameBootstrap` or the relevant
   database SO. This requires **no new package** and is strictly a code-quality/safety
   improvement (compiler/inspector-checked reference instead of a string path).
2. **Step 2 — Addressables for content that is genuinely optional/lazy-loaded (optional).** Only
   for assets that are legitimately not always needed (e.g. large one-off content, or
   variant-selected art), consider `com.unity.addressables` as a second step, **after** step 1 has
   removed most of the direct-reference-eligible `Resources.Load` call sites and shown what
   remains. Do not add Addressables to the manifest before step 1 shows the residual need is real.

### Alternatives considered

- **Do nothing (status quo):** all 54.62 MB stays force-included; string-path loads keep the
  no-compile-check risk profile. Zero migration cost today.
- **Addressables first, skip direct-reference step:** Addressables solves the build-size/lazy-load
  problem well, but is a new package dependency with its own learning curve (asset groups, build
  layouts, remote/local content catalogs) and does not by itself fix the "string path, no
  compile-time check" problem for content that should simply be a direct reference instead of
  loaded lazily at all.
- **Direct references only, no Addressables ever (this proposal's minimum):** covers most of the
  practical benefit (typo-safety, compiler/inspector-checked wiring) for content that is always
  needed, with zero new dependency; only content that is genuinely optional/on-demand gets the
  larger Addressables investment, and only if step 1 shows real residual need.

## Cost

- Step 1: medium — requires enumerating every `Resources.Load` call site (not yet done in this
  session; a dedicated audit/spec must run
  `Select-String -Pattern "Resources\.Load" -Recurse` across `Assets/_Game/Scripts/**` to produce
  the authoritative list), then converting each to a serialized reference at its actual call site,
  which touches whatever MonoBehaviour/SO owns that reference plus its wiring (prefab/inspector
  assignment) — a `.prefab`/`.asset` change per site, gated by `unity-assets` rule evidence
  requirements.
- Step 2 (if pursued): medium-high — new package dependency, new build configuration
  (`Packages/manifest.json` change, Addressables settings asset), and a new mental model
  (`AssetReference`, addressable groups) for whoever authors content going forward.

## Risk

- Step 1: converting `Resources.Load` to a direct reference can miss a call site that was relying
  on late/dynamic resolution (e.g. choosing among several assets by a runtime-computed string) —
  those specific cases may still need `Resources.Load` or an equivalent registry/lookup pattern
  (see `registry-catalog-pattern` skill) rather than a single fixed serialized reference; the
  eventual implementer must classify each call site, not assume all are convertible 1:1.
  the eventual implementer must classify each call site, not assume all are convertible 1:1.
- Step 2: Addressables changes the build pipeline and adds asset-group configuration that must be
  reviewed under `unity-assets`/`editor-generation-orchestration` conventions; skipped or done
  incorrectly, it can produce assets missing at runtime in a built player even though they work
  fine in the Editor (a classic Addressables pitfall).

## Requer decisão humana

**SIM** — motivo: step 2 (Addressables) is a new package dependency and a build-pipeline change
that the project does not currently have; adding it should be the human's call, not an implicit
side effect of cleaning up `Resources.Load` call sites. Step 1 alone (direct references) is lower
risk and could proceed without Addressables, but even step 1 touches `.prefab`/`.asset` wiring
across possibly many call sites and should be scoped as its own dedicated spec once the call-site
audit exists, not executed ad hoc.

## Source Documents

- [spec_codex_08_convergence_decisions.md](../../.specs/a_implementar/spec_codex_08_convergence_decisions.md)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
