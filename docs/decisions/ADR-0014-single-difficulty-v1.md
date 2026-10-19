---
doc_type: adr
status: accepted
adr_id: ADR-0014
title: Single Difficulty in v1
date: 2026-06-13
source_documents:
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
supersedes: []
superseded_by: []
applies_to:
  - difficulty
  - balance-tuning
  - telemetry
  - accessibility
---

# ADR-0014 — Single Difficulty in v1

## Status

**accepted** (difficulty/balance decision for v1)

## Context

The project must decide whether v1 ships multiple difficulty levels or an assist mode. Multiple
difficulty tracks multiply the balance/tuning surface (every encounter, drop, and economy value
must hold across each track) and fragment telemetry, making it harder to read whether the single
intended experience is well-tuned. F59 introduces telemetry intended to drive balance tuning.

The owner answered decision **4.6** of `FABLE_DECISOES_RESPOSTAS_v3.0.md` (VINCULANTE,
2026-06-13): a **single difficulty** in v1, with tuning driven by **F59 telemetry**, and **no
assist mode**; the decision must be registered.

## Decision

**v1 ships a single difficulty. There is no difficulty selector and no assist mode in v1. Balance
is tuned via F59 telemetry against that single difficulty.**

- There is **one** difficulty experience in v1 — no Easy/Normal/Hard selector.
- There is **no assist mode** in v1 (no damage-reduction sliders, no enemy-toggle, no
  auto-aim/auto-play accessibility toggles).
- Balance and tuning are iterated using **F59 telemetry**: the single difficulty is the calibration
  target, and telemetry data informs adjustments to encounters, economy, and progression.

## Scope

- **In scope:** declaring one difficulty for v1; excluding a difficulty selector and assist mode
  from v1; pointing balance tuning at F59 telemetry.
- **Out of scope:** the actual balance numbers (owned by the relevant gameplay/economy/progression
  specs); a future post-v1 decision on adding difficulty modes or accessibility/assist options; the
  telemetry implementation itself (F59).

## Implementation

- v1 gameplay specs assume a single difficulty target; they do not branch behavior on a difficulty
  setting and do not add an assist toggle.
- Balance changes reference F59 telemetry as the evidence source for tuning the single difficulty.
- No difficulty/assist UI is added (the F56 system tab is volumes + fullscreen/windowed +
  resolution only — see ADR-aligned F56 amendment; difficulty is explicitly not part of it).

## Consequences

- The balance surface stays single-track, so telemetry reads cleanly and tuning effort is focused.
- Players who want easier/harder modes or assist options are not served in v1; this is a known,
  documented limitation.
- A future difficulty/assist decision can build on a well-tuned single baseline rather than tuning
  multiple tracks at once; adding such modes would require a superseding decision.

## Applies To

- All v1 gameplay/balance specs
- F59 telemetry-driven tuning
- The F56 system tab (which deliberately excludes a difficulty/assist control)

## Source Documents

- [FABLE_DECISOES_RESPOSTAS_v3.0.md](../design/FABLE_DECISOES_RESPOSTAS_v3.0.md) — decision 4.6 (VINCULANTE)

---

*Created: 2026-06-13*
*Status: accepted*
*Source decision: 4.6 (FABLE Decisões v3.0)*
