---
doc_type: game_rule
status: accepted
domain: player-progression
source_adrs:
  - ADR-0010
source_documents:
  - docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
  - docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
  - docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
  - docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md
  - Assets/_Game/Scripts/Player/Progression/ProgressionCurve.cs
  - Assets/_Game/Scripts/Player/Progression/PlayerProgressionRules.cs
  - Assets/_Game/Scripts/Player/Progression/PlayerProgressionManager.cs
  - Assets/_Game/Scripts/Player/Progression/PlayerProgressionSaveData.cs
  - Assets/_Game/Scripts/Player/DerivedStatsCalculator.cs
  - Assets/_Game/Scripts/Player/PlayerVitalsApplier.cs
  - Assets/_Game/Scripts/Player/Conditions/FatigueThreshold.cs
  - Assets/_Game/Scripts/Player/Conditions/FatigueSystem.cs
last_reviewed: 2026-06-13
---

# Player Rules

> **This document is current operational behavior, not desired future state.**
> **Change this rule only via a new ADR or spec.**
>
> Scope note: this rule owns **player progression and the player's own derived stats**
> (level, XP, attributes, vitals, fatigue/sleep, inferred class). It does **not** own the
> skill-tree economy (points spent, ranks, slots, respec) — that is
> [skill_tree_rules.md](skill_tree_rules.md). Accessory/equipment slots are owned by
> [inventory_equipment_rules.md](inventory_equipment_rules.md). Sibling rules are
> cross-referenced, not duplicated.

## Purpose

Defines how the player character grows and is statted: the level cap and XP curve, XP
sources, the central attributes and the points that raise them, the derived vitals
(HP / MP / Stamina) and how they are computed and applied at runtime, the fatigue / sleep /
collapse condition track, and the inferred-class identity layer. It is the canonical *what*
for FABLE specs `fable_16` (fatigue/sleep), `fable_18` (derived-stats vitals application),
`fable_23`, `fable_29`, `fable_39` (inferred class) and `fable_42` (XP curve).

---

## Definitions and Terms

- **Total XP (source of truth):** the single persisted progression value. Level, current-level
  XP and "XP to next" are **derived** from Total XP, never stored as the authority
  (`ProgressionCurve`, `PlayerProgressionManager`).
- **Level:** the derived band the player has reached from Total XP; capped at 100.
- **Central attribute:** one of the six raw attributes the player allocates points into.
- **Attribute point:** one unspent point that raises one central attribute by +1.
- **Skill point:** the currency spent in the skill tree (owned by `skill_tree_rules.md`);
  earned from leveling and main-quest acts.
- **Derived stat / vital:** a runtime value computed from base + attributes + level +
  equipment + skill passives (`DerivedStatsCalculator`), e.g. Max HP, Max Stamina, Max Mana.
- **Fatigue (Cansaço):** long-term wear that accumulates over time and activity and is
  reduced by sleep; distinct from Stamina (the immediate physical resource).
- **Inferred class:** a display-only title plus a small identity bonus derived from how
  skill points are distributed across trees; never a stored class.

---

## Canonical Rules

### Rule 1 — Level cap and XP curve

- **Rule:** The level cap is **100** (`ProgressionCurve.MaxLevel = 100`;
  `PlayerProgressionRules.MaxLevel = 100`).
- **Curve:** XP required to advance from level `N` to `N+1` is
  **`XpForNext(N) = round(60 × N^1.5)`** (`ProgressionCurve.XpForNext`, line 18).
- **Total XP is the authority:** the player's current level is derived from accumulated
  Total XP (`ProgressionCurve.LevelForTotalXp`, lines 35-57); current-level XP for the HUD bar
  is `XpIntoCurrentLevel` (lines 60-64); `XpToNextLevel` is recomputed from the derived level.
- **At the cap:** once level 100 is reached, **excess XP still accumulates** in Total XP but
  the level no longer rises (the derive loop stops at `MaxLevel`, lines 44-46).
- **Legacy migration:** a legacy save that stored `{level, currentXp}` with `TotalXp == 0` is
  migrated to the equivalent Total XP on load via `ProgressionCurve.MigrateLegacy`
  (lines 67-70); migration does **not** re-grant attribute or skill points
  (`PlayerProgressionManager.NormalizeState`, lines 220-225).
- **Applies to:** All player progression.
- **Source:** `ProgressionCurve.cs`, `PlayerProgressionRules.cs`; design `fable_42` curve
  (BALANCE_CURVES v1.0 as cited in code).

#### XP curve reference (computed from `round(60 × N^1.5)`)

| Level N → N+1 | XP for next | Cumulative Total XP to reach N+1 |
|---|---:|---:|
| 1 → 2 | 60 | 60 |
| 2 → 3 | 170 | 230 |
| 5 → 6 | 671 | ~1,400 |
| 10 → 11 | 1,897 | ~12,300 |
| 25 → 26 | 7,500 | ~118,000 |
| 50 → 51 | 21,213 | ~660,000 |
| 99 → 100 | 59,100 | ~3.9M (to reach 100) |

> Values are computed from the live formula, not hand-authored; the cumulative column is
> approximate (rounded) and exists for pacing reference only.

### Rule 2 — XP sources

- **Rule:** Total XP is increased only through these channels (`PlayerProgressionManager`):
  1. **Enemy kills** — on `EnemyKilledEvent`, awards `evt.XpReward` (lines 160-166). The
     reward is `max(1, enemyLevel) × difficultyMultiplier`, with multipliers
     `VeryEasy 5 / Easy 8 / Normal 10 / Hard 15 / Elite 25 / MiniBoss 50 / Boss 100`
     (`PlayerProgressionRules.CalculateEnemyXpReward`, lines 54-91); an explicit
     `overrideValue > 0` wins.
  2. **Cave-level discovery** — the first time a deeper cave level is entered, awards
     `15 × band`, where `band = ((caveLevel - 1) / 10) + 1` (lines 168-180). Idempotent:
     guarded by `DeepestXpAwardedCaveLevel`, so revisiting an already-rewarded level grants
     nothing (preserves the cave stable-run contract — see `cave_rules.md`).
  3. **First harvest of each crop** — `+10` the first time a given `SeedId` is harvested
     (`CropHarvestedEvent`, lines 182-198). Idempotent per seed via `FirstHarvestXpSeedIds`.
- **Constraint:** XP gains of `amount <= 0` are ignored (`AddXp`, lines 44-49).
- **Applies to:** Combat, cave exploration, farming progression loops.
- **Source:** `PlayerProgressionManager.cs`, `PlayerProgressionRules.cs`.

### Rule 3 — Central attributes and attribute points

- **Rule (design intent):** The player has **6 central attributes** —
  **Força, Constituição, Destreza, Inteligência, Vontade, Carisma** — and there is no fixed
  D&D-style class (`PLAYER_CORE_SYSTEMS_DIRECTION` §4-6). The player starts with **1 point in
  each** central attribute and gains **+1 attribute point per level up** to spend on a central
  attribute (`PLAYER_CORE_SYSTEMS_DIRECTION` §5).
- **Rule (code grant):** `PlayerProgressionRules.CalculateAttributePointsGrantedOnLevelUp`
  grants 1 point for every level above 1 (lines 43-46); total points at the cap is
  `level - 1` = **99 at level 100** (lines 48-52). Points are spent one at a time via
  `PlayerProgressionManager.TrySpendAttributePoint`, which fails when no unspent points remain
  (lines 67-103).
- **Each attribute's responsibility** (design direction; the runtime values that consume them
  are Rules 4-5):

  | Attribute | Primarily affects (design) |
  |---|---|
  | Força (Strength) | physical damage, heavy weapons/tools, stagger/knockback, breaking obstacles; small Stamina |
  | Constituição (Constitution) | moderate HP, moderate Stamina, posture/physical-status resistance, moderate Block stability and fatigue resistance — **not** a universal defense multiplier |
  | Destreza (Dexterity) | movement, Dodge/Dash, attack speed, recovery, conditional crit; small Stamina |
  | Inteligência (Intelligence) | technical magic, crafting, reading monsters/traps/resources |
  | Vontade (Willpower) | Max MP, slow MP regen, mental/spiritual resistance, Fonte de Anya |
  | Carisma (Charisma) | social influence, reputation, future vendor pricing, companions/pets |

- **Known divergence (must not be silently "fixed"):** the live enum
  `PlayerAttributeType` exposes `Strength, Dexterity, Intelligence, Willpower, Constitution,
  **Breath**` (`PlayerAttributeType.cs`), and `PlayerProgressionSaveData` still carries a
  `Breath` field defaulted to 1 (line 23). The binding design direction
  (`PLAYER_CORE_SYSTEMS_DIRECTION` §0, `PLAYER_DERIVED_ATTRIBUTES_DIRECTION` §0.1) **removed
  Breath/Fôlego entirely** and replaces the sixth slot with **Carisma**. Until a spec
  reconciles the enum/save (rename `Breath` → `Carisma`, no behavior change since neither is
  consumed by a derived formula yet), the **legacy `Breath` field is retained for save
  backward-compatibility and must not be surfaced in HUD, derived formulas, or new content**.
- **Applies to:** Character creation defaults, level-up rewards, the attribute screen (F14).
- **Source:** `PLAYER_CORE_SYSTEMS_DIRECTION`, `PlayerProgressionRules.cs`,
  `PlayerAttributeType.cs`, `PlayerProgressionSaveData.cs`.

### Rule 4 — Skill points (cross-reference, not duplicated)

- **Rule:** Skill points are earned from progression at **1 per 2 levels**
  (`PlayerProgressionRules.SkillPointIntervalLevels = 2`; total at cap = `level / 2` = **50
  base** at level 100, lines 37-41) **plus 1 per completed main-quest act** (FABLE v3.0
  Decision 1.9, OVERRIDE), giving an effective ceiling of **~55**. The first skill point is
  granted on reaching level 2 (none at level 1).
- **Ownership:** how those points are *spent* — tree structure, dynamic rank caps, per-node
  prerequisites, 4 active slots, real-time cooldowns, and respec via the Fonte de Anya — is
  defined by **[skill_tree_rules.md](skill_tree_rules.md)** and reconciled by **ADR-0010**.
  This rule does not restate those mechanics.
- **Applies to:** The boundary between leveling (here) and skill spending (skill tree).
- **Source:** `PlayerProgressionRules.cs`, FABLE v3.0 Decision 1.9, ADR-0010.

### Rule 5 — Derived vitals and the layered formula

- **Rule:** The player's max vitals and combat stats are **derived**, never raw-stored as the
  authority. The runtime aggregator is `DerivedStatsCalculator.Calculate`, which starts from
  base values and adds equipment bonuses then skill passive modifiers
  (`DerivedStatsCalculator.cs`):
  - equipment adds `StrengthBonus → Attack`, `BaseDefense → Defense`, and the
    Toxic/Cold/Heat resistances (lines 59-70);
  - skill passive modifiers add flat HP / Stamina / Mana / Attack / Defense / regen /
    resistance / move & attack speed / craft & repair / hunger-drain reductions, by
    `SkillModifierType` (lines 72-96).
- **Design formula (the *intended* layering these flat sums feed into,
  `PLAYER_DERIVED_ATTRIBUTES_DIRECTION` §1):**
  ```text
  DerivedStat = Base + AttributeContribution + LevelContribution
              + EquipmentFlat + SkillFlat + BuffFlat
  FinalStat   = DerivedStat × (1 + SkillPct) × (1 + EquipPct) × (1 + BuffPct)
              × (1 - StatusPenalty) × ContextMultiplier
  ```
  Flat bonuses apply before multipliers; percent bonuses that could break balance need a
  cap/softcap.
- **Recalculate-on-load / on-change:** max vitals are recomputed (never trusted from a stale
  save) by `PlayerVitalsApplier.Reapply` on `EquipmentSlotChangedEvent` and at start
  (`PlayerVitalsApplier.cs`, lines 65-104, 142-149).
- **Applies to:** HP / MP / Stamina maxima, Attack, Defense, resistances, regen.
- **Source:** `DerivedStatsCalculator.cs`, `PlayerVitalsApplier.cs`,
  `PLAYER_DERIVED_ATTRIBUTES_DIRECTION`.

#### Direction-level vital constants (proposta a calibrar)

These are the **recommended** tuning constants from the design direction
(`PLAYER_DERIVED_ATTRIBUTES_DIRECTION` §9-13). They are **not yet hard-coded** in a shipping
formula (the runtime currently sums flat bonuses without per-attribute coefficients), so they
are marked **proposta a calibrar** and must be validated in Play Mode before being treated as
final:

| Vital | Direction formula (recommended) | Constants |
|---|---|---|
| Max HP | `Base + Level×2 + CON×5 + equip + skill + buff` | Base 110, HP/level 2, HP/CON 5 |
| Max MP | `Base + Level×1 + Vontade×6 + INT×2 + equip + skill + buff` | Base 40, MP/level 1, MP/Will 6, MP/Int 2 |
| Max Stamina | `Base + Level×0.6 + CON×2.0 + FOR×1.5 + DES×1.0 + equip + skill + buff` | Base 80, /level 0.6, /CON 2.0, /Str 1.5, /Dex 1.0 |
| Defense | `BaseDefense + Armor + CON×0.75 + skill + buff` | Defense/CON 0.75 (Armor is the main source) |
| Stamina regen (out of combat) | `BaseRegen + CON×0.15 + DES×0.20 + survival + equip + food − armor` | BaseRegen 5 |
| Stamina regen (in combat) | `out-of-combat × CombatMultiplier` | CombatMultiplier 0.35–0.50 |

> Live runtime defaults that differ and are the *current* authority until calibration:
> `PlayerManager` Max HP comes from `PlayerDataSO.BaseHP` (line 40); `StaminaManager` default
> Max Stamina = 100 (line 11) with day-start full recover (lines 123-126); `ManaManager`
> default Max Mana = 100 with 5 MP/s regen (lines 11-12).

#### Stamina action costs (canonical reference, Steel Sword)

From `PLAYER_DERIVED_ATTRIBUTES_DIRECTION` §12 (closed decisions, Part L); costs scale with
weapon/tool tier and action type so high level gives margin, not free spam:

| Action | Reference Stamina cost |
|---|---:|
| Light melee (Steel Sword) | 25 |
| Heavy melee (Steel Sword) | 40 |
| Dash | 40 |
| Dodge | 40 |
| Block (hold) | 18 / s |

Block **impact** cost is proportional to post-armor damage relative to Max HP, not raw damage
(direction §12 "Block impact"); final caps are still **proposta a calibrar**.

### Rule 6 — Vitals application contract (F18)

- **Rule:** `PlayerVitalsApplier` is the single bridge from the derived stats to the vital
  managers (`PlayerVitalsApplier.cs`):
  - it captures base maxima once at start (lines 131-137) and recomputes on demand;
  - it sets `MaxHP`, `MaxStamina`, `MaxMana` **preserving the current proportion** with floor
    1 (`PreserveRatio`, lines 37-46; e.g. raising Max HP while at 50% leaves the player at
    ~50%, never an instant heal) and pushes regen bonuses into the managers;
  - it lowers hunger drain by the derived `HungerDrainReduction`, clamped to `[0.25, 1.0]`
    (line 99);
  - it feeds per-type resistance into combat damage via
    `PlayerDamageReceiver.ResistanceSource` (Toxic→ToxicRes, Ice→ColdRes, Fire→HeatRes;
    lines 49-63, 103).
- **Constraint:** the bridge must never use a forbidden global search at runtime — it resolves
  managers through `GameBootstrap` (lines 117-129) per the Unity architecture invariants.
- **Applies to:** Any spec that changes how derived stats reach HP/MP/Stamina/resistance.
- **Source:** `PlayerVitalsApplier.cs`, ADR-0006 (no Unity refs in save), event-bus invariant.

### Rule 7 — Fatigue, sleep and collapse (F16)

- **Rule:** Fatigue (Cansaço) is a long-term wear track separate from Stamina, owned by the
  pure `FatigueSystem` (it is **not** part of `HungerManager`):
  - value range **0-100**, banded by `FatigueThreshold` (`FatigueThreshold.cs`):
    **Rested 0-24, SlightlyTired 25-49, Tired 50-74, VeryTired 75-89, Exhausted 90-100**;
    penalties begin at **Tired** (`HasPenalty` ≥ Tired); **Exhausted** is the collapse band;
  - gain sources (`FatigueSystem.cs`): **~2 fatigue / hour awake** (`AddTimePassingFatigue`,
    lines 60-69); **10% of Stamina spent** converts to fatigue
    (`AddFatigueFromStaminaSpend`, lines 37-49); low hunger (`< 20%`) multiplies fatigue gain
    by **×1.5**, and cave exploration multiplies by **×1.3** (lines 7-8, 26-30);
  - relief: sleep reduces fatigue via `SleepRecoveryCalculator` and records the sleep quality
    (`ApplySleepRecovery`, lines 51-58).
- **Design intent (`PLAYER_CORE_SYSTEMS_DIRECTION` §11):** high fatigue reduces recovery,
  efficiency, movement and combat safety; it is reduced by sleep, rest, food, the Survival
  tree and some effects. Fatigue resistance reduces pressure but never removes the need to
  sleep. Day transition and sleep timing are owned by **[time_rules.md](time_rules.md)**
  (cross-ref — the calendar/day-advance and "when sleep happens" live there); this rule owns
  the fatigue values themselves.
- **Note:** Stamina fully recovers on `DayStartedEvent` (`StaminaManager`, lines 123-126);
  fatigue does **not** auto-reset on day start — only sleep/rest reduces it.
- **Applies to:** Long runs, the sleep loop, the fatigue HUD indicator.
- **Source:** `FatigueSystem.cs`, `FatigueThreshold.cs`, `PLAYER_CORE_SYSTEMS_DIRECTION`.

### Rule 8 — Inferred player class (F39)

- **Rule:** The player **never chooses a class**; an inferred title plus a tiny identity bonus
  is derived (read-only, deterministic) from how skill points are distributed across the five
  trees (FABLE v1.0 §9 / Decision Q9.1; `fable_39` spec). It is computed by a pure function
  recomputed on skill purchase / respec — **no class state is ever saved** (it always re-derives
  from the skill points that the skill tree already persists).
- **Inference thresholds (`fable_39`):**
  - **dominant** tree = the tree with the most points, **only if** it has **≥ 6 points and
    ≥ 40% of total** spent;
  - **hybrid** if the second tree has **≥ 70% of the first**'s points → composite title and
    **half** of each micro-bonus;
  - otherwise **"Colono"** (no dominant tree, **no bonus**), including ties.
- **Identity bonus cap:** micro-bonuses are **identity, not power — never above 5%**
  (`fable_39`): Warrior +3% posture dealt, Hunter +3% crit chance, Mystic −3% mana cost,
  Farmer +5% stamina outside the cave, Bond +3% friendship gained; hybrids get half of each.
  The bonus enters as **one named entry** in the derived-stats provider (replaced on each
  recompute — never double-counted).
- **Constraint:** no rigid classes, no class respec (it self-derives), no content gating by
  class, no new saved state.
- **Applies to:** The character sheet title (F14) and occasional NPC greetings (F28).
- **Source:** `fable_39_spec_inferred_player_class_runtime.md`, FABLE v1.0 §9 (Q9.1).

---

## Edge Cases

- **XP overflow at cap 100:** Total XP keeps growing but level stays 100; current-level XP
  reported for the HUD is clamped to the level-100 band (`ProgressionCurve`, lines 44-46, 60-64).
- **Legacy save (`TotalXp == 0`, level > 1):** migrated to equivalent Total XP **without**
  re-granting attribute/skill points (`PlayerProgressionManager.NormalizeState`, lines 220-225).
- **Death XP loss:** `ResetCurrentLevelXp` removes only the **current-level** partial XP (not
  whole levels) and re-derives level/partial from the reduced Total XP
  (`PlayerProgressionManager`, lines 145-158). The death/recovery flow that calls it is owned
  by [death_anya_corpse_rules.md](death_anya_corpse_rules.md).
- **Raising Max HP/Stamina/Mana mid-run:** proportion is preserved (no free full heal); floor
  is 1 (`PlayerVitalsApplier.PreserveRatio`).
- **No unspent points:** `TrySpendAttributePoint` / `TrySpendSkillPoints` return `false` and
  change nothing when the player has no points (`PlayerProgressionManager`, lines 67-115).
- **Empty inferred class:** with no dominant tree (or a tie), the profile is "Colono" with no
  bonus — identical stats to a fresh character.
- **Legacy `Breath` attribute:** retained in save only; not consumed by any derived formula
  and not shown in HUD until reconciled to Carisma (Rule 3 divergence).

---

## What Persists in the Save

Persisted (`PlayerProgressionSaveData`, `PlayerSaveData`, `ManaManagerSaveData` — simple types
and IDs only, no Unity refs, per ADR-0006):

- **Total XP** (authority), plus derived `Level`, `CurrentXp`, `XpToNextLevel` (recomputed on
  load — the persisted copies are convenience/cache, not the source of truth);
- XP idempotency guards: `DeepestXpAwardedCaveLevel`, `FirstHarvestXpSeedIds`;
- `UnspentAttributePoints`, `UnspentSkillPoints`;
- the six attributes `Strength, Dexterity, Intelligence, Willpower, Constitution, Breath`
  (Breath is legacy — see Rule 3 divergence);
- current/max HP and current/max Mana; current/max Hunger and position (`PlayerSaveData`).

Recalculated on load (never trusted from the save):

- Max HP / Max Stamina / Max Mana, Attack, Defense, resistances, regen — recomputed by
  `PlayerVitalsApplier` from attributes + equipment + skill passives;
- the inferred-class title and bonus — re-derived from skill points (no class state saved).

Not persisted: the inferred class profile; any derived stat; the removed Breath/Fôlego as a
*resource* (the field is a legacy attribute slot only).

---

## What Tests Should Cover

Per [.claude/rules/testing-quality-gate.md](../../.claude/rules/testing-quality-gate.md),
the deterministic progression logic here is **mandatory EditMode** territory; vitals
application and fatigue under live scene/day events are **Play Mode / human scenario**:

- **XP curve (EditMode):** `XpForNext` matches `round(60×N^1.5)` at sample levels; level cap
  holds at 100 with overflow accumulating; `LevelForTotalXp` / `XpIntoCurrentLevel` round-trip;
  legacy migration produces equivalent Total XP and does **not** re-grant points.
- **XP sources (EditMode):** enemy reward = `max(1, level) × multiplier` per difficulty;
  cave-discovery `15×band` is idempotent per depth; first-harvest `+10` is idempotent per
  `SeedId`.
- **Attribute/skill grants (EditMode):** +1 attribute point per level (99 at cap); 1 skill
  point per 2 levels (50 base at cap); spend fails with zero unspent points.
- **Derived vitals (EditMode):** `DerivedStatsCalculator` sums equipment + passive modifiers
  correctly per `SkillModifierType`; `PreserveRatio` keeps proportion with floor 1.
- **Fatigue (EditMode):** threshold banding (Rested→Exhausted boundaries 25/50/75/90); gain
  from stamina spend (10%), time (2/h), hunger ×1.5, cave ×1.3; sleep reduces fatigue.
- **Inferred class (EditMode, from `fable_39` CA-1…CA-4):** dominance (≥6 pts & ≥40%), hybrid
  (2nd ≥70% of 1st → composite title + half bonus), Colono (no dominant / ties), recompute on
  skill purchase/respec, and the ≤5% magnitude guard.
- **Play Mode / human scenario:** HP/Stamina/Mana maxima reapply on equip change without a free
  heal; day-start stamina full-recover; fatigue accumulation and sleep relief across a day; the
  inferred-class title shows on the character sheet (F14).

---

## Quick Reference (canonical values)

| Field | Canonical value | Source |
|---|---|---|
| Level cap | 100 | `ProgressionCurve.MaxLevel` / `PlayerProgressionRules.MaxLevel` |
| XP to next | `round(60 × N^1.5)` | `ProgressionCurve.XpForNext` (line 18) |
| XP authority | Total XP (level derived) | `PlayerProgressionManager` / `PlayerProgressionSaveData.TotalXp` |
| Attribute points | +1 per level (99 at cap) | `PlayerProgressionRules` (lines 43-52) |
| Skill points | 1 / 2 levels (50 base) + 1/act (~55) | `SkillPointIntervalLevels=2`; FABLE v3.0 1.9; see skill_tree_rules.md |
| Central attributes | Força, Constituição, Destreza, Inteligência, Vontade, Carisma | `PLAYER_CORE_SYSTEMS_DIRECTION` §4 |
| Starting attributes | 1 in each | `PLAYER_CORE_SYSTEMS_DIRECTION` §5 |
| Enemy XP reward | `max(1, level) × {VE5/E8/N10/H15/Elite25/Mini50/Boss100}` | `PlayerProgressionRules` (lines 54-91) |
| Cave-discovery XP | `15 × band` (idempotent) | `PlayerProgressionManager` (lines 168-180) |
| First-harvest XP | +10 per seed (idempotent) | `PlayerProgressionManager` (lines 182-198) |
| Vitals applier | proportion-preserving, recompute on equip/start | `PlayerVitalsApplier` |
| Fatigue bands | Rested<25, SlightlyTired<50, Tired<75, VeryTired<90, Exhausted≤100 | `FatigueThreshold.cs` |
| Fatigue gain | 10% stamina spent; ~2/h; hunger ×1.5; cave ×1.3 | `FatigueSystem.cs` |
| Inferred class | derived (≥6 & ≥40% dominant; 2nd ≥70% hybrid; else Colono), bonus ≤5% | `fable_39` |
| HP/MP/Stamina design constants | proposta a calibrar (Base HP 110, etc.) | `PLAYER_DERIVED_ATTRIBUTES_DIRECTION` §9-13 |

---

## Open Questions

- Reconcile the live `PlayerAttributeType.Breath` enum value and `PlayerProgressionSaveData.Breath`
  field to **Carisma** (design-removed Breath/Fôlego), with a migration that does not change
  behavior (neither value feeds a derived formula today).
- Wire the direction-level per-attribute vital coefficients (Base HP 110, HP/CON 5, Stamina
  per-attribute, etc.) into a calibrated runtime formula and validate in Play Mode — currently
  **proposta a calibrar**, as the runtime sums flat bonuses without those coefficients.
- Final caps for Block Power / Block Stability / Stamina regen and the min/max
  `BlockImpactStaminaCost` (deferred by `PLAYER_DERIVED_ATTRIBUTES_DIRECTION` Part M).
- Exact main-quest-act → skill-point scaling once the F42 ceiling rebalance (50 + 1/act) lands
  (tracked in skill_tree_rules.md, not here).

---

## Cross-References

### Related ADRs

- [ADR-0010: FABLE Skill and Inventory Rules Reconciliation](../decisions/ADR-0010-fable-skill-and-inventory-rules-reconciliation.md)
  (level cap 100, 50-base skill points, +1/act ceiling — shared progression facts)
- [ADR-0006: Save Data Contracts](../decisions/ADR-0006-save-data-contracts-simple-dtos.md)
  (progression/vitals persist as simple types + IDs; vitals recomputed on load)
- [ADR-0007: Event Bus Gameplay Communication](../decisions/ADR-0007-event-bus-gameplay-communication.md)
  (XP/level/attribute/vitals events; `PlayerVitalsApplier` resolves via bootstrap, no global search)

### Sibling Game Rules

- [skill_tree_rules.md](skill_tree_rules.md) — **owns** skill-point spending, trees, dynamic
  rank caps, prerequisites, 4 active slots, real-time cooldowns, and respec via the Fonte de
  Anya. This rule owns only how points are *earned*.
- [inventory_equipment_rules.md](inventory_equipment_rules.md) — **owns** the 9 equipment slots
  (incl. the 3 accessory-type slots) and durability/upgrade; the derived-stats formula here
  consumes equipment bonuses but does not define slots.
- [combat_rules.md](combat_rules.md) — damage, status effects and enemy difficulty (the
  difficulty tiers that feed enemy XP rewards in Rule 2).
- [death_anya_corpse_rules.md](death_anya_corpse_rules.md) — death/recovery flow that triggers
  current-level XP loss (Rule edge case) and Fonte-de-Anya respec.
- [time_rules.md](time_rules.md) — day transitions and sleep timing that drive fatigue
  accumulation/relief and day-start stamina recovery (Rule 7).
- [save_rules.md](save_rules.md) — the DTO/versioning contract these progression DTOs follow.

---

*Last Reviewed: 2026-06-13 (new rule, grounded in live code + player design directions + ADR-0010)*
*Source: PLAYER_CORE_SYSTEMS_DIRECTION, PLAYER_DERIVED_ATTRIBUTES_DIRECTION, PLAYER_SKILL_TREES_DIRECTION, FABLE v3.0, ProgressionCurve.cs, PlayerProgressionRules.cs, PlayerProgressionManager.cs, DerivedStatsCalculator.cs, PlayerVitalsApplier.cs, FatigueSystem.cs, FatigueThreshold.cs, fable_39 spec*
