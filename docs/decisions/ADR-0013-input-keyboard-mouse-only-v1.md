---
doc_type: adr
status: accepted
adr_id: ADR-0013
title: Input — Keyboard and Mouse Only in v1
date: 2026-06-13
source_documents:
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
supersedes: []
superseded_by: []
applies_to:
  - input-handling
  - input-map
  - ui-navigation
  - control-scheme
---

# ADR-0013 — Input: Keyboard and Mouse Only in v1

## Status

**accepted** (control-scheme decision for v1)

## Context

The project must decide which input devices the v1 control scheme supports. Supporting gamepad in
v1 expands the cost surface: every UI screen needs focus/navigation that works without a pointer,
every action needs a gamepad binding, and glyph prompts must reflect the active device. The UI
work (F14 panel, F56 system tab/title flow) is already building keyboard navigation
(UiFocusController, Tab cycling, Esc-close) — keyboard/mouse is the path under active construction.

The owner answered decision **4.5** of `FABLE_DECISOES_RESPOSTAS_v3.0.md` (VINCULANTE,
2026-06-13): v1 is **keyboard/mouse only**; gamepad becomes a post-v1 spec; the decision must be
registered in the `input_map` (coordinated with F67).

## Decision

**v1 supports keyboard and mouse only. Gamepad support is a post-v1 spec and is out of scope for v1.**

- The v1 control scheme targets keyboard + mouse exclusively.
- UI navigation is designed for keyboard (arrows/Tab/Enter/Esc) and mouse — see the F14 panel and
  F56 system tab/title flow, which already build this via UiFocusController.
- **Gamepad is explicitly deferred** to a dedicated post-v1 spec. v1 specs must not add gamepad
  bindings, glyph-swapping, or device-detection machinery.
- This decision is recorded in the **`input_map`** (the input convention coordinated with F67), so
  the input map states "keyboard/mouse only (v1); gamepad post-v1".

## Scope

- **In scope:** declaring keyboard + mouse as the v1 device set; deferring gamepad to post-v1;
  recording the decision in the input_map (F67).
- **Out of scope:** the specific keybindings themselves (defined by gameplay specs and the input
  map); gamepad bindings, glyph prompts, and device detection (post-v1 gamepad spec); touch/other
  input devices.

## Implementation

- v1 input handling and UI navigation assume keyboard + mouse only.
- The input_map (F67) records the device set as "keyboard/mouse only (v1)" with gamepad marked as a
  post-v1 spec, so future contributors see the boundary at the convention level.
- No v1 spec introduces gamepad code paths; a future post-v1 gamepad spec owns that work and may
  supersede this scope clause for the device set.

## Consequences

- v1 input and UI complexity stays bounded to one device class, matching the keyboard-navigation UI
  already under construction (F14/F56).
- Gamepad players are not supported in v1; this is a known, documented limitation, not a bug.
- A clean insertion point exists for the future gamepad spec (it adds bindings/glyphs without
  reworking the v1 keyboard/mouse paths).
- Adding gamepad to v1 later would require a superseding decision; the post-v1 gamepad spec is the
  expected vehicle.

## Applies To

- All v1 input-handling and UI-navigation specs
- The input_map convention (F67)
- The future post-v1 gamepad spec (as the deferred owner)

## Source Documents

- [FABLE_DECISOES_RESPOSTAS_v3.0.md](../design/FABLE_DECISOES_RESPOSTAS_v3.0.md) — decision 4.5 (VINCULANTE)

---

*Created: 2026-06-13*
*Status: accepted*
*Source decision: 4.5 (FABLE Decisões v3.0)*
