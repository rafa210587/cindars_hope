---
name: rng-and-determinism
description: Use seeded, per-system RNG for any randomness that touches saves, loot, weather, spawns or generation. Generalizes the FASE9F cave stable-run pattern to all systems. Use whenever adding or changing randomness anywhere in gameplay.
---

# Skill: RNG and Determinism

## The project precedent (cave stable-run, FASE9F)

Cave generation already follows the right pattern: deterministic seeds derived from `CaveWorldSeed + CaveRunSeed + CaveLevel + stable salt`; revisits never reroll; seed changes only on new game / KO / explicit debug command (rule: cave-stable-run). **Apply the same discipline everywhere else.**

## Rules for any new randomness

1. **Never `UnityEngine.Random` (global state) in systems whose outcome is saved or revisited** — loot rolls, weather generation, spawn composition, quality rolls, NPC schedule variation. Global state means any other system's call reorders your results.
2. **One `System.Random` per system per scope**, seeded from stable components:
   ```csharp
   int seed = StableHash(worldSeed, "loot", enemyId, dayNumber);   // order-insensitive system salt
   var rng = new System.Random(seed);
   ```
   Use a deterministic string hash (e.g., FNV-1a over the composed key) — **not** `string.GetHashCode()` (varies per runtime/process) and never GUIDs/timestamps for stable IDs (rule: cave-stable-run).
3. **Same trigger, same result:** reopening the same chest, re-entering the same cave level, re-rolling the same day's weather after reload must produce identical outcomes. If a reroll IS the design (daily shop stock), the day number belongs in the seed.
4. **Roll at decision time, persist the OUTCOME** in save data — don't persist the RNG state and don't re-roll on load.
5. **Visual-only randomness is exempt** (particle jitter, idle animation offsets): `UnityEngine.Random` is fine there — outcomes are never saved or gameplay-relevant.
6. **Telegraph high-stakes RNG:** rare-drop or critical chances that gate progression should be inspectable in design docs (drop table percentages in the loot SO, validated by the catalog validator), never buried as magic numbers in code.

## Tests (skill: editmode-test-authoring)

- Same seed inputs → identical sequence/outcome (twice in the same test).
- Different salt per system → different sequences (no cross-system correlation).
- Outcome persisted: save→load→same result without re-rolling.

## Applies directly to upcoming fable specs

fable_09 (cave biome layout variety — MUST stay inside stable-run contract), fable_06/24 (loot tables, affixes), fable_31 (unidentified magic items), fable_37 (festivals/lunar events).
