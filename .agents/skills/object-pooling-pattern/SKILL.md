---
name: object-pooling-pattern
description: Add or repair pooling only for measured spawn/despawn churn, preserving spawn-service ownership, reset semantics and object lifetime.
---

# Skill: Object pooling pattern

**Verify the current spawn path and measured cost before adding a pool.** Do not assume the
repository has no pooling. Reuse an existing service or compatible Unity pool before writing
another implementation; a pool is an optimization with extra lifetime/state contracts.

## Required decisions

- The spawn service owns acquisition/release; gameplay callers keep their existing request/
  result contracts. Inject dependencies through current composition, never scene search.
- Define active/inactive ownership, duplicate release handling, capacity/prewarm and exhaustion
  policy. Growth, rejection or recycling must preserve the authored gameplay; no arbitrary cap.
- Initialize all per-use state before activation exposes it to OnEnable/subscribers: target,
  damage, hit count, timers, velocity, colliders and callbacks. Clear trails/particles and old
  references. Cleanup must handle cancelled coroutines and late callbacks from the previous use.
- Disable/unsubscribe on release and register once on reuse. Scene unload disposes owned pool
  resources without retaining destroyed scene objects. One-shot objects need not be pooled.
- Shared ScriptableObjects hold authored config, not mutable per-projectile runtime state.

## Evidence

Exercise acquire→use→release→reuse with different data; assert no stale damage/hits, duplicate
callback, double release corruption or scene reference leak. Use PlayMode for actual Unity
lifecycle/physics/visual state. Profile the same workload before/after, separating cold creation,
warm reuse and unrelated allocations; never infer zero GC or a performance win from pooling.

Reference on demand: [Object Pool, Game Programming Patterns](https://gameprogrammingpatterns.com/object-pool.html).
