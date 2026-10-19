---
doc_type: adr
status: proposed
adr_id: ADR-0017
title: Reputation Absorbed by Friendship
date: 2026-06-20
source_documents:
  - .specs/a_implementar/fable/fable_57_spec_living_city_birthdays_inn_reputation_adr.md
  - Assets/_Game/Scripts/NPC/Friendship/FriendshipState.cs
  - Assets/_Game/Scripts/NPC/Friendship/FriendshipService.cs
  - docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
  - docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
supersedes: []
superseded_by: []
applies_to:
  - npc-social-systems
  - city-design
relates_to:
  - .specs/a_implementar/fable/fable_26_spec_friendship_state_contract.md
---

# ADR-0017 — Reputation Absorbed by Friendship

## Status

**proposed** (2026-06-20) — requires human approval before becoming `accepted`.

> The city directions (README/CITY_DESIGN) flag social-systems direction as needing human
> sign-off, and the spec (fable_57) explicitly asks for this decision to be recorded as a
> **proposal pending human review**. No textual cleanup of the city directions is performed
> by this ADR (that is a separate docs follow-up, see Migration Notes).

## Context

Two competing social concepts have lived in the design space without a single owner:

- **"Reputação" (reputation).** `CITY_DESIGN_DIRECTION` and several city directions mention
  "reputation" as if it gated farm visits, town hall, guard interactions, and the social hub.
  **No system ever defined it** — there is no `ReputationService`, no `ReputationState`, no
  reputation save section, no reputation score anywhere in the code. It is a direction-level
  word only.
- **Amizade (friendship), F26.** `fable_26` implemented a concrete, tested, per-NPC
  **friendship** model: points 0–150 mapped to levels 0–5 (thresholds 10/30/60/100/150),
  daily-capped sources (conversation/quest/gift/purchase), a stable gate API
  (`FriendshipService.IsAtLeast(npcId, level)`), and save round-trip
  (`FriendshipSaveData`). Friendship is the social state of record.

Having a named-but-undefined concept ("reputation") next to a real, shipping system
("friendship") is a permanent duplication hazard: a future spec could implement a parallel
reputation tracker, which is exactly what the `system-reuse-audit` discipline exists to
prevent. fable_57 (Living City: birthdays + inn + reputation ADR) resolves the ambiguity.

## Decision

**Per-NPC friendship (F26) is the single social-standing tracker in v1. The concept of a
separate "reputation" system is retired: "reputation" is absorbed by friendship, and any
direction-level gate previously attributed to "reputation" reads friendship instead.**

- **No reputation system exists or may be created.** There is no `ReputationService`,
  `ReputationState`, reputation score, or reputation save section, and none may be added.
  A standalone reputation tracker parallel to `FriendshipService` is prohibited.
- **Friendship is the social gate.** Where a direction says "requires reputation X", the
  canonical reading is "requires friendship level N with the relevant NPC" (or, for
  town-wide framing, an aggregate over friendship — to be specified by the consuming spec,
  not by inventing a new score).
- **fable_57 honored this in code:** birthdays are a *multiplier inside the existing gift
  flow* of `FriendshipService` (gift on the NPC's birthday is worth ×2, daily cap intact),
  not a new social tracker; no reputation code was written.
- **Termo aposentado.** "Reputação" is a deprecated term in the city directions; the next
  docs pass replaces remaining mentions with "amizade" / friendship-level wording.

## Consequences

### Positive

- One canonical social system. An agent reading the city directions will no longer
  "implement reputation" and fork the social model.
- The `system-reuse-audit` has a citable barrier: any future PR adding a reputation tracker
  is a non-regression violation against this ADR.
- fable_57's birthday ×2 lands as a thin consumer of F26, with zero new save state.

### Negative

- Direction text still says "reputation" in places until the docs follow-up runs; until
  then this ADR is the canonical reconciliation note. (Tracked in Migration Notes.)
- A future design may genuinely want a town-wide standing distinct from per-NPC friendship.
  This decision is **reversible by a superseding ADR** that specifies such a system
  deliberately, rather than letting it appear by accident.

### Operational

- `FriendshipService` / `FriendshipState` (F26) remain the implementation of record for
  social standing; no code changes are mandated by this ADR (fable_57's birthday multiplier
  is additive within the existing gift flow).
- Anti-regression: the diff for fable_57 (and future specs) must contain **no**
  `ReputationService`/`ReputationState`/reputation save section. An EditMode guard
  (`BirthdayInnTests.NoParallelReputationTracker_*`) asserts no such type is loaded.
- Docs follow-up (separate task): replace "reputation" wording in `CITY_DESIGN_DIRECTION`
  and related city directions with friendship-level wording, citing this ADR.

## Applies To

- NPC social systems (friendship as the single social tracker)
- City design directions (reputation wording)
- Future specs that gate on "social standing"

## NOT Applicable To

- Romance/marriage (fable_46) — a separate relationship axis, out of scope here.
- Per-NPC gift preferences (fable_72) — already a friendship consumer.
- Any runtime code beyond the prohibition (this ADR creates no system; it forbids one).
- Historical documents (docs_old), superseded specs.

## Source Documents

- [`fable_57` spec](../../.specs/a_implementar/fable/fable_57_spec_living_city_birthdays_inn_reputation_adr.md) — requested this ADR (CA-4)
- [`fable_26` spec](../../.specs/a_implementar/fable/fable_26_spec_friendship_state_contract.md) — the friendship contract that absorbs reputation
- [`FriendshipState.cs`](../../Assets/_Game/Scripts/NPC/Friendship/FriendshipState.cs) — the social state of record (levels/caps/save)
- [`FriendshipService.cs`](../../Assets/_Game/Scripts/NPC/Friendship/FriendshipService.cs) — the gate API + gift flow (birthday ×2 added by fable_57)
- `CITY_DESIGN_DIRECTION_v1.2.md`, `SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md` — sources of the "reputation" wording

## Migration Notes

- No code migrated (no reputation code ever existed).
- City directions are **not edited by this ADR** (fable_57 keeps them out of scope); a docs
  follow-up will replace "reputation" wording with friendship-level wording and cite ADR-0017.
- No amendment retired; no document deleted.

---

*Created: 2026-06-20 (fable_57)*  
*Last reviewed: —*  
*Status: proposed (human approval pending)*
