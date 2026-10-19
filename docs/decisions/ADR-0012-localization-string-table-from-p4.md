---
doc_type: adr
status: accepted
adr_id: ADR-0012
title: Localization via String Table from Phase P4
date: 2026-06-13
source_documents:
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
supersedes: []
superseded_by: []
applies_to:
  - localization
  - quest-text-authoring
  - dialogue-authoring
  - text-discipline
---

# ADR-0012 — Localization via String Table from Phase P4

## Status

**accepted** (text-authoring discipline; effective from phase P4 onward)

## Context

Quest and dialogue text is currently hardcoded inline in C# and in dialogue data authored across
earlier waves. A full localization framework is heavy and premature for the current scope, but if
all new text keeps being hardcoded inline, a future localization effort becomes a project-wide
retrofit touching every call site.

The owner answered decision **4.7** of `FABLE_DECISOES_RESPOSTAS_v3.0.md` (VINCULANTE,
2026-06-13) as an **OVERRIDE** of the default: introduce a lightweight localization discipline
starting at phase **P4** (the F35 / F36 / F70 + dialogue wave), without adopting a localization
framework. The interpreted ambiguity in the same document clarifies the retrofit boundary
(see "Ambiguidades interpretadas" item 4).

## Decision

**From phase P4 onward, quest and dialogue text is authored via an id → string table (a light
discipline, no framework). Text already hardcoded in earlier waves is NOT retrofitted now; that
retrofit is registered as explicit debt.**

### What the discipline is

- New quest/dialogue text added from P4 onward is referenced by a **stable string id** and resolved
  through an **id → string table** rather than written as an inline literal at the call site.
- The table is a plain data convention (e.g. a simple id-keyed map / data asset). **No localization
  framework, no runtime locale-switching machinery, no .po/.resx tooling is adopted in v1.**
- Only one language (the authoring language) is populated in v1. The discipline exists so a second
  language can be added later by populating the table — not by re-editing call sites.

### What it explicitly does NOT do

- It does **not** retrofit text that is already hardcoded in waves prior to P4. That existing
  hardcoded text remains as-is for now.
- It does **not** introduce locale selection UI, font fallback per locale, or pluralization rules in v1.

## Scope

- **In scope:** the id → string-table convention and the requirement that quest/dialogue text added
  from P4 onward use it; the explicit recording of the pre-P4 hardcoded-text retrofit as debt.
- **Out of scope:** a localization framework; multi-locale runtime switching; retrofitting pre-P4
  text; non-quest/non-dialogue strings (UI chrome, logs) unless a later spec opts them in.

## Implementation

- Specs in the P4 wave and later that author quest or dialogue text must reference string ids and
  populate the id → string table, not embed literals at the call site.
- The exact table location and id-naming convention are defined alongside the P4 wave (input map /
  text convention, coordinated with F67) before P4 execution begins.
- Pre-P4 hardcoded text is left untouched; its eventual migration is tracked as registered debt.

## Consequences

- New text is centralized and translation-ready without a framework dependency.
- A future second language can be added by populating the table, not by a global call-site rewrite,
  for all text authored from P4 onward.
- Mixed state during the transition: pre-P4 text stays inline while post-P4 text lives in the table.
  This is accepted and explicitly documented as debt rather than blocking the wave.
- If a full localization framework is adopted later, this lightweight table is the migration seed;
  changing the convention would require a superseding ADR.

## Registered Debt

```text
Debt: pre-P4 quest/dialogue text is hardcoded inline and is NOT migrated to the id → string table
      by this decision. Migration is deferred; tracked here as explicit technical debt to be
      scheduled if/when a second language or full framework is adopted.
```

## Applies To

- All quest/dialogue text authored from phase P4 onward
- The P4 wave specs (F35 / F36 / F70 and dialogue work) and later text-authoring specs
- The text/input convention coordinated with F67

## Source Documents

- [FABLE_DECISOES_RESPOSTAS_v3.0.md](../design/FABLE_DECISOES_RESPOSTAS_v3.0.md) — decision 4.7 (OVERRIDE, VINCULANTE) and "Ambiguidades interpretadas" item 4

---

*Created: 2026-06-13*
*Status: accepted*
*Source decision: 4.7 (FABLE Decisões v3.0, OVERRIDE)*
