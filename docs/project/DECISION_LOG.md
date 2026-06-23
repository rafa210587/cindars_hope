# Decision Log — Cindar's Hope

> Index of Architecture and Design Decision Records.  
> Agents do not read all ADRs by default; specs must cite required ADRs explicitly.

---

## Reading Policy

An agent implementing a spec reads ONLY the ADRs/game rules explicitly listed by the spec.

If a spec conflicts with an ADR or game rule:
- **Stop**
- **Report conflict**
- **Do not implement** until reconciled

If an ADR conflicts with a game rule:
- Game rule is the current operational rule
- ADR explains history and rationale
- Update the ADR or create superseding ADR

---

## Active Decisions (ADR-0001 to ADR-0019)

| ADR | Title | Theme | Status | Canonical Ref | Source |
|---|---|---|---|---|---|
| [ADR-0001](../decisions/ADR-0001-canonical-documentation-structure.md) | Canonical Documentation Structure | Documentation | accepted | docs/decisions/ADR-0001-* | SPEC_DOCS_35-37 |
| [ADR-0002](../decisions/ADR-0002-agent-context-minimum.md) | Agent Context Minimum | Governance | accepted | .claude/rules/context-reading-policy.md | CLAUDE.md, AGENTS.md |
| [ADR-0003](../decisions/ADR-0003-spec-lifecycle.md) | Spec Lifecycle | Governance | accepted | docs/decisions/ADR-0003-* | SPEC_EXECUTION_ORDER.md, CURRENT_STATE.md |
| [ADR-0004](../decisions/ADR-0004-validation-evidence-phase-gates.md) | Validation Evidence and Phase Gates | Validation | accepted | docs/decisions/ADR-0004-* | LAST_VALIDATION_STATUS.md, .claude/rules/* |
| [ADR-0005](../decisions/ADR-0005-cave-stable-run-and-replay.md) | Cave Stable Run and Replay | Gameplay | accepted | docs/decisions/ADR-0005-* | FASE9F Amendment, SPEC_24 |
| [ADR-0006](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) | Save Data Contracts Simple DTOs | Architecture | accepted | docs/decisions/ADR-0006-* | .claude/rules/save-dto-simple-types-only.md |
| [ADR-0007](../decisions/ADR-0007-event-bus-gameplay-communication.md) | Event Bus Gameplay Communication | Architecture | accepted | docs/decisions/ADR-0007-* | .claude/rules/event-bus-only-gameplay-communication.md |
| [ADR-0008](../decisions/ADR-0008-unity-yaml-editing-policy.md) | Unity Scene/Asset YAML Editing Policy | Tooling | accepted | docs/decisions/ADR-0008-* | .claude/rules/unity-yaml-editing-policy.md |
| [ADR-0009](../decisions/ADR-0009-mvp-acceptance-phase-2-3.md) | MVP Acceptance Requires Phase 2-3 | Validation | accepted | docs/decisions/ADR-0009-* | .claude/rules/no-premature-acceptance-claims.md |
| [ADR-0010](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md) | FABLE Skill & Inventory Rules Reconciliation | Gameplay | accepted | docs/game_rules/skill_tree_rules.md, inventory_equipment_rules.md | FABLE_DECISOES_RESPOSTAS_v3.0 (1.8, 2.12) |
| [ADR-0011](../decisions/ADR-0011-pixel-art-scale-32px-per-tile.md) | Pixel Art Scale 32px/Tile | Art | accepted | docs/decisions/ADR-0011-* | FABLE_DECISOES_RESPOSTAS_v3.0 (4.3) |
| [ADR-0012](../decisions/ADR-0012-localization-string-table-from-p4.md) | Localization String Table from P4 | Tooling | accepted | docs/decisions/ADR-0012-* | FABLE_DECISOES_RESPOSTAS_v3.0 (4.7) |
| [ADR-0013](../decisions/ADR-0013-input-keyboard-mouse-only-v1.md) | Input Keyboard/Mouse Only v1 | Tooling | accepted | docs/decisions/ADR-0013-* | FABLE_DECISOES_RESPOSTAS_v3.0 (4.5) |
| [ADR-0014](../decisions/ADR-0014-single-difficulty-v1.md) | Single Difficulty v1 | Gameplay | accepted | docs/decisions/ADR-0014-* | FABLE_DECISOES_RESPOSTAS_v3.0 (4.6) |
| [ADR-0015](../decisions/ADR-0015-canonical-specs-relocated-to-dotspecs.md) | Canonical Specs Relocated to .specs | Governance | accepted | .claude/rules/docs-governance.md | Owner directive 2026-06-13 |
| [ADR-0016](../decisions/ADR-0016-cave-enemy-density-depth-scaling.md) | Cave Enemy Density and Depth Scaling | Gameplay | accepted | docs/game_rules/cave_rules.md | fable_67 |
| [ADR-0017](../decisions/ADR-0017-reputation-absorbed-by-friendship.md) | Reputation Absorbed by Friendship | Gameplay | proposed | docs/decisions/ADR-0017-* | fable_57 |
| [ADR-0018](../decisions/ADR-0018-cave-conflict-stable-run-carveout.md) | Cave Inter-Monster Conflict Carve-out | Gameplay | accepted | docs/game_rules/cave_rules.md | fable_78 |
| [ADR-0019](../decisions/ADR-0019-cave-biome-mineable-budget-supersedes-resource-node-range.md) | Cave Biome Mineable Budget Supersedes 4-10 | Gameplay | accepted | docs/game_rules/cave_rules.md | fable_78 |

---

## Decision Categories

### Documentation & Governance (ADR-0001, ADR-0002, ADR-0003)

These define how documentation is organized and read:
- Canonical folder structure
- Agent context minimum
- Spec lifecycle locations

**See also:** docs/game_rules/documentation_rules.md, docs/game_rules/agent_execution_rules.md

### Validation & Acceptance (ADR-0004, ADR-0009)

These define validation phases and acceptance criteria:
- Phase 0/1/2/3 gates
- NOT_RUN explicit tracking
- MVP acceptance requires Phase 2-3

**See also:** docs/game_rules/validation_acceptance_rules.md

### Gameplay Architecture (ADR-0005, ADR-0007)

These define core gameplay and communication patterns:
- Cave stable run invariant
- Event bus for gameplay communication

**See also:** docs/game_rules/cave_rules.md, docs/game_rules/event_rules.md

### Data & Persistence (ADR-0006)

Defines save/load and data contracts:
- Simple DTOs, no Unity refs, versionable

**See also:** docs/game_rules/save_rules.md

### Tooling & Safety (ADR-0008)

Defines how tools and code editing work:
- No manual YAML edits (use scripts/tools)

**See also:** .claude/rules/unity-yaml-editing-policy.md

---

## Related Game Rules

All game-specific rules are in `docs/game_rules/`. Key documents:

- [documentation_rules.md](../game_rules/documentation_rules.md) — File organization, canonical sources
- [agent_execution_rules.md](../game_rules/agent_execution_rules.md) — What agents read/don't read
- [validation_acceptance_rules.md](../game_rules/validation_acceptance_rules.md) — Phase definitions, acceptance criteria
- [cave_rules.md](../game_rules/cave_rules.md) — Cave stable run, snapshots, ranges
- [event_rules.md](../game_rules/event_rules.md) — Event bus, communication patterns
- [save_rules.md](../game_rules/save_rules.md) — DTO contracts, versioning, migration
- [combat_rules.md](../game_rules/combat_rules.md) — Enemy roles, AI, status effects
- [inventory_equipment_rules.md](../game_rules/inventory_equipment_rules.md) — Slots, capacity, durability
- [skill_tree_rules.md](../game_rules/skill_tree_rules.md) — Points, slots, respec
- [ui_modal_rules.md](../game_rules/ui_modal_rules.md) — Modal stack, input blocking
- [death_anya_corpse_rules.md](../game_rules/death_anya_corpse_rules.md) — Death flow, recovery
- [player_rules.md](../game_rules/player_rules.md) — Level/XP curve, derived stats, vitals, fatigue
- [economy_rules.md](../game_rules/economy_rules.md) — Pricing, shop stock refresh, money sources/sinks
- [quest_rules.md](../game_rules/quest_rules.md) — Objectives/events, conditions, reward idempotency, flags
- [npc_rules.md](../game_rules/npc_rules.md) — NPC data, schedules, dialogue, friendship, gifts, services
- [time_rules.md](../game_rules/time_rules.md) — Calendar, seasons, day transitions, weather, lunar cycle
- [city_rules.md](../game_rules/city_rules.md) — Cindar's Hope footprint, districts, interiors, landmarks
- [fonte_rules.md](../game_rules/fonte_rules.md) — Fonte state, fragment-driven unlocks, final-choice gating
- [ui_rules.md](../game_rules/ui_rules.md) — Gameplay HUD overlays, floating combat feedback (non-modal)

---

## Decisions Deprecated/Superseded

- ADR-0005 — "Resource Node Range 4-10" partially superseded by ADR-0019 (per-biome budget).
- ADR-0005 — "scene identical on revisit" clause carved out by ADR-0018 (scoped to
  non-player-caused enemy HP/death; composition unchanged).
- ADR-0005 — "Enemies per level range" previously superseded by ADR-0016 (16-32, cap 44).

---

## How to Use This Log

### For Agents

- Spec cites `required_adrs: [ADR-0005]` → Read ADR-0005 only, not others
- Spec cites `required_game_rules: [cave_rules.md, event_rules.md]` → Read only those game rule docs
- Spec is self-contained → Don't read ADRs/game rules unless cited

### For Humans

- Looking for why a decision was made? → Find the ADR
- Looking for current rules/behavior? → Find the game_rules doc
- Looking for historical amendments? → Amendments are archived after migration; ADR explains what changed

### For Architecture Review

- New spec contradicts ADR? → Stop, report, reconcile
- New code contradicts game rule? → Stop, report, reconcile
- New document needs ADR? → Create ADR, cite in DECISION_LOG, update related game_rules

---

*Last Updated: 2026-06-13 (Refinamento v3 — ADR-0010..0014 adicionados; skill_tree_rules.md e inventory_equipment_rules.md reconciliados com o código FABLE pela ADR-0010)*  
*Source of Truth: docs/decisions/ (ADRs) and docs/game_rules/ (rules)*
