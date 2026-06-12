---
name: object-pooling-pattern
description: Introduce object pooling for high-churn spawns (projectiles, floating damage text, drops, enemy waves) without breaking existing spawn service contracts. Use when touching ProjectileSpawnService, spawners, or any code flagged by performance-auditor for Instantiate/Destroy churn.
---

# Skill: Object Pooling Pattern

**Project baseline:** there is NO pooling anywhere yet. `ProjectileSpawnService.SpawnProjectile()` does `Object.Instantiate(prefab)` / `Object.Destroy()` per shot; `CaveEnemySpawner` instantiates waves. Bow/spell combat makes this per-attack GC churn.

## Pool contract

```csharp
namespace CindarsHope.Core.Pooling
{
    /// Plain C# pool keyed by prefab (or visual-style for runtime-built projectiles).
    public class GameObjectPool
    {
        private readonly Stack<GameObject> _inactive = new Stack<GameObject>();
        private readonly GameObject _prefab;
        private readonly Transform _parent;   // inactive container, set once by bootstrap

        public GameObject Get(Vector3 position)
        {
            GameObject go = _inactive.Count > 0 ? _inactive.Pop() : Object.Instantiate(_prefab, _parent);
            go.transform.position = position;
            go.SetActive(true);
            return go;
        }

        public void Release(GameObject go)
        {
            go.SetActive(false);
            _inactive.Push(go);
        }
    }
}
```

## Non-negotiable rules when pooling in this project

1. **Full state reset on Get or Release.** Pooled `ProjectileBehaviour` must re-run the equivalent of `Initialize(...)`: direction, speed, range traveled, damage, status effect, **hit counter (`SetMaxHits`) and any per-flight accumulators**. A pooled projectile that remembers old hits is a gameplay bug, not a perf bug.
2. **Replace `Destroy(this.gameObject)` with `Release` via callback/event** — the projectile must not know the pool; the spawn service owns it. Keep `ProjectileSpawnResult`/`ProjectileSpawnRequest` contracts unchanged so callers (BowArrowAttackService, SpellCastService) are untouched.
3. **No global search to find the pool** (rule: unity-architecture). The pool lives in/under GameBootstrap and is injected into the spawn service.
4. **Trail/particle hygiene:** call `TrailRenderer.Clear()` / `ParticleSystem.Clear()` on reuse, or the projectile teleport-streaks across the screen.
5. **Physics hygiene:** zero `Rigidbody2D.velocity`/`angularVelocity` on Release; re-enable colliders if disabled on impact.
6. **Don't pool one-shots** (boss intro FX, one-per-day objects). Pool only per-attack/per-wave churn.
7. **Cap + prewarm:** prewarm typical burst size (e.g., 8 projectiles); allow growth; never hard-fail on empty pool.

## Validation

- EditMode: pool Get/Release/reuse state-reset tests (pure C# part).
- Play Mode human scenario: fire 50+ arrows/spells, verify no stale trails, no double-hit from recycled projectiles, profiler shows no per-shot GC alloc (skill: gameplay-test-scenario).
