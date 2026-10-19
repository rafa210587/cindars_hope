---
doc_type: adr
status: proposed
adr_id: ADR-0024
title: Remaining IMGUI to Canvas Migration
date: 2026-07-03
source_documents:
  - .specs/a_implementar/spec_codex_08_convergence_decisions.md
supersedes: []
superseded_by: []
applies_to:
  - ui-architecture
  - imgui-migration
---

# ADR-0024 — Remaining IMGUI → Canvas Migration

## Status

**proposed** (draft — requires human decision before any implementation; documents an already
partially-executed migration and proposes the order for the rest)

## Context

The project has already executed part of this migration once, successfully:
`DeathScreenCanvasController` (Canvas-based, confirmed present in
`Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs`) replaced the older IMGUI
`DeathScreenController` via fable_64 (confirmed in this session by reading the file's own header
comment: "Canvas-based death screen ... (replaces the IMGUI DeathScreenController)"). This is a
working precedent for how to do the remaining migrations, not a fresh design problem.

Reconfirmed 2026-07-03 via `Select-String -Pattern "void OnGUI" -List` across
`Assets/_Game/Scripts/**`: **18 files** still define an `OnGUI` method (matching the prior audit's
figure exactly). This session did not enumerate all 18 by name as part of the doc-only scope of
this ADR draft — the implementer of the eventual migration spec must run
`Select-String -Path (Get-ChildItem -Recurse -Filter *.cs) -Pattern "void OnGUI" -List` again at
that time and produce the authoritative list before starting, since the count (and the exact set
of files) may have shifted since this reconfirmation.

Known from this session's broader context (not an exhaustive list): `DebugHud` is IMGUI and is
explicitly documented across many `docs/validation/WAVE_INTEGRATION_*` reports as the reused,
functional-but-not-final HUD ("DebugHud IMGUI mantido sem alterações"); several
`InventoryPanelController`/`CharacterEquipmentPanelController`/`SkillTreeGameplayPanelController`/
`QuestOfferPanelController`/`QuestLogPanelController` are also documented as IMGUI-based panels
still pending their own Canvas migration (UI debt already tracked in those WAVE reports, not new
information this ADR invents).

## Decision (proposed, not accepted)

Migrate the remaining IMGUI (`OnGUI`) surfaces to Canvas incrementally, following the
already-successful death-screen precedent (one `XCanvasController` MonoBehaviour + Canvas
hierarchy replacing one `XController` IMGUI class, keeping the same GameEventBus contract so no
other system needs to change). Proposed order, most-used/most-visible first (subject to human
reordering):

1. Core gameplay HUD elements surfaced by `DebugHud` (health/stamina/mana/hotbar/feedback toasts)
   — highest visibility, but also the most events/subscriptions to carry over correctly; the
   `hud-canvas-binding` skill already documents the canonical Canvas-binding pattern for this.
2. Inventory / Equipment panels — already have real Canvas-adjacent design intent documented in
   WAVE_INTEGRATION_09/10 reports as debt; migrating these completes work already flagged, not new
   work.
3. Skill tree / Quest offer / Quest log panels — same category, already tracked as UI debt in
   WAVE_INTEGRATION_10/15/19 reports.
4. Any remaining `OnGUI` debug-only tooling (if any of the 18 are dev-only diagnostic overlays,
   not player-facing) — lowest priority; may be acceptable to leave as IMGUI indefinitely since
   dev tooling is explicitly out of the player-facing UI contract.

### Alternatives considered

- **Do nothing (status quo):** IMGUI panels keep working today; no functional regression risk.
  Cost is UX/visual polish debt (already acknowledged across many WAVE reports) and continued
  divergence between "final" Canvas UI and "temporary" IMGUI UI.
- **Migrate everything in one large spec:** highest short-term throughput, but touches the most
  files/events simultaneously and is hardest to Play-Mode-validate incrementally (each panel needs
  its own human checklist per the `testing-quality-gate` rule, since UI/visual changes are not
  meaningfully covered by EditMode tests alone).
- **Incremental, one-panel-per-spec (this proposal):** matches how the death screen migration was
  actually done (fable_64, a dedicated spec) and lets each migration get its own Play Mode human
  checklist without blocking on the others.

## Cost

- Per-panel: low-medium, following the death-screen precedent (MonoBehaviour + Canvas + same
  event contract). Multiplied by however many of the 18 `OnGUI` sites are player-facing (not all
  18 necessarily are — some may be dev-only tools, which this ADR does not require migrating).
- Full enumeration/triage of the 18 files (which are player-facing vs. dev-only) is itself a small
  first task that should happen before scheduling individual migration specs.

## Risk

- Each migrated panel needs its own Play Mode human validation (per `testing-quality-gate`) since
  UI event wiring and modal-stack interaction (`ui-modal-stack` skill) are easy to get subtly wrong
  in ways EditMode tests do not catch.
- Some `OnGUI` panels may have implicit behavior (e.g. exact `ModalManager` push/pop timing,
  keyboard focus routing per `input-gamepad-routing`) that is easy to regress if the Canvas
  replacement does not carry over every subscription exactly.

## Requer decisão humana

**NÃO** para a decisão de continuar seguindo o precedente já estabelecido (death screen) —
isso já é a prática aceita do projeto. **SIM** para a ordem/priorização das migrações restantes e
para decidir se os `OnGUI` de ferramentas dev-only (a serem identificados na primeira tarefa de
triagem) precisam ser migrados ou podem permanecer IMGUI indefinidamente.

## Source Documents

- [spec_codex_08_convergence_decisions.md](../../.specs/a_implementar/spec_codex_08_convergence_decisions.md)
- `Assets/_Game/Scripts/UI/Death/DeathScreenCanvasController.cs` (existing precedent)

---

*Created: 2026-07-03*
*Status: proposed — awaiting human decision*
