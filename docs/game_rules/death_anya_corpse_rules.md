---
doc_type: game_rule
status: accepted
domain: gameplay-life-death
source_adrs: []
source_documents:
  - docs/specs/implementados/SPEC_25_DEATH_ANYA_CORPSE_CLOSEOUT.md
last_reviewed: 2026-06-01
---

# Death, Anya, and Corpse Rules

## Purpose

Defines player death flow, corpse recovery mechanics, and Anya's role in respawning and skill respec.

---

## Canonical Rules

### Rule: Player Death Flow

- **Rule:** When player health reaches 0:
  1. Player character plays death animation (1-2 seconds)
  2. Death modal opens (showing "You are defeated" message)
  3. Player has two options:
     a. Respawn at last safe location (farm/village start)
     b. Reload last save (if save exists)
  4. Corpse is left at death location in cave (if in cave)
  5. Player respawns with full health/mana

- **Applies to:** Combat and cave exploration
- **Respawn location:** Farm/village (safe zone); not cave
- **Corpse location:** Death location (cave level) persists until recovered or run ends

### Rule: Corpse Recovery (Anya Mechanic)

- **Rule:** After death, player can interact with Anya NPC to recover corpse:
  - Anya is located in safe zone (farm/village)
  - Interaction opens "Recover Corpse" dialogue
  - Player can pay gold to have Anya retrieve corpse items
  - Cost: 50 gold per corpse recovery
  - Result: Items from corpse returned to player inventory (if space)

- **Corpse contents:** All items on death (equipment + carried items)
- **Corpse despawn:** Corpse remains until recovered OR run ends (new cave run clears old corpses)
- **Failed recovery:** If inventory full, items drop at feet (player must manage inventory)

- **Applies to:** Recovery mechanics

### Rule: Respawn and Run Reset

- **Rule:** On respawn:
  - Player health/mana restored to full
  - All active status effects cleared
  - Equipment remains equipped (not dropped)
  - Inventory remains (items not lost, except those that drop on death)
  - Active cave run continues (CaveRunSeed not reset)
  - Revisit to cave level uses snapshot (cave_rules.md)

- **Constraint:** Respawn does not reset the cave run; player can continue exploration
- **Applies to:** Persistence and cave stability

### Rule: Anya NPC Role

- **Rule:** Anya has multiple functions:
  1. **Corpse Recovery:** Retrieves dropped items for gold
  2. **Skill Respec:** Resets skill tree for gold cost (100 gold per respec)
  3. **Tutorial/Dialogue:** Explains death mechanics, respec, recovery

- **Location:** Safe zone (farm or village start)
- **Interaction:** Talk to Anya to open dialogue menu
- **Gold costs:**
  - Corpse recovery: 50 gold
  - Skill respec: 100 gold
  - Both can be done in same session

- **Constraint:** Both services available immediately (no cooldown)

### Rule: Death Penalties

- **Rule:** Death penalty is minimal:
  - No permanent item loss (can recover via Anya)
  - No level/XP loss
  - No skill reset (unless player chooses respec)
  - Only gold cost for corpse recovery

- **Why:** Encourages exploration without harsh punishment
- **Applies to:** Player retention and difficulty balance

### Rule: Corpse Despawn

- **Rule:** Corpse remains at death location until:
  1. Player recovers via Anya (items gone)
  2. Cave run ends (player goes to farm, ends run)
  3. New cave run starts (new CaveRunSeed clears old corpses)

- **Despawn on exit:** When player exits cave permanently, all corpses in that run despawn
- **Applies to:** Clean run management

---

## Death State Management

- **Rule:** On death, create DeathSaveData:
  - Timestamp of death
  - Location (cave level, coordinates)
  - Inventory snapshot (items in corpse)
  - Equipment snapshot (what was equipped)
  - Death cause (enemy, hazard, etc.)

- **Persistence:** Death data saved in game state (not player save; reset on new run)
- **Applies to:** Recovery system and debugging

---

## Open Questions

- Can multiple corpses exist at once? (Current: yes; one per death location; last death overwrites)
- Does corpse recovery give all items or partial? (Current: gives all items from corpse)
- What if player is in cave and Anya is in farm? (Current: must exit cave and go to farm for Anya)
- Can corpse recovery fail? (Current: no; always succeeds if gold available)
- Are there ghost penalties (reduced damage/defense)? (Current: no penalties; full strength on respawn)

---

## Related ADRs

- [ADR-0005: Cave Stable Run and Replay](../decisions/ADR-0005-cave-stable-run-and-replay.md) (run continuation after death)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md) (death/respawn events)
- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md) (death state persistence)

---

*Last Reviewed: 2026-06-01 (SPEC_DOCS_38)*  
*Source: SPEC_25*
