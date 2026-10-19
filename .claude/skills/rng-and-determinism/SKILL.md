---
name: rng-and-determinism
description: Preserve reproducible gameplay randomness and saved outcomes for loot, weather, spawns and generation. Use when changing RNG, seed derivation or replay-sensitive ordering.
---

# Skill: RNG and determinism

**A seed alone does not establish determinism.** Control algorithm/version, stream ownership,
call order, stable input ordering and time. Reuse the existing RNG/clock ports; no parallel RNG service.

## Contract

1. Separate Gameplay/World/Visual purposes. Never consume global `UnityEngine.Random` in a
   saved or revisited gameplay decision. Visual jitter may use it only if it cannot affect gameplay.
2. Reuse the current stable seed derivation with world/run/entity/day and a domain salt as
   appropriate. Never use `string.GetHashCode`, timestamps or new GUIDs as stable seeds/IDs.
   Preserve existing algorithms unless the spec authorizes a versioned migration.
3. Canonicalize unordered input collections before rolls when order affects selection.
   Define who advances each stream; adding a cosmetic roll must not change gameplay results.
4. Persist the decided outcome/snapshot and load it without rerolling. Cave revisits within
   the same run retain their snapshots; read `cave-stable-run-guard` before changing that flow.
5. `System.Random(seed)` is not a cross-runtime/version compatibility guarantee. If a spec
   requires that guarantee, use a versioned existing algorithm and golden vectors; do not
   introduce a new PRNG solely to make a same-runtime unit test pass.
6. Supply time/ticks through the existing clock or explicit delta. Do not infer deterministic
   physics or cross-platform bitwise equality from a fixed seed or a passing pure test.

## Tests selected by changed risk

- Same inputs with independent instances → expected sequence/outcomes; use known vectors
  when algorithm compatibility is part of the contract, not the production function as oracle.
- Advance an unrelated stream → target stream unchanged. Different salts need not produce
  different individual outcomes; collisions are possible and do not prove correlation.
- Save/load/revisit → persisted outcome unchanged, no extra roll or duplicate reward.
- Reordered input → same outcome only where order independence is the intended contract.
- State the runtime/version and tolerance for numeric comparisons. Failing seeds are replay
  inputs; repeated random runs are not a substitute for a reproducible regression.

Reference when compatibility matters: [Microsoft System.Random remarks](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-random).
