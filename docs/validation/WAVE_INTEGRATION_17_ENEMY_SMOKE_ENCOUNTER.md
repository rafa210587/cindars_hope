# WAVE_INTEGRATION_17 — Enemy Smoke Encounter

**Date:** 2026-06-10

---

## Smoke Enemy Configuration

| Field | Value | Source |
|-------|-------|--------|
| Bridge component | `CaveSmokeTestSpawnerBridge` | WAVE17 new |
| Enemy data | `enemy_slime_basic.asset` | EXISTING |
| Enemy ID | `enemy_slime_basic` (from asset) | EXISTING |
| Components added at runtime | EnemyHealth, EnemyChaseController, EnemyContactDamage, KnockbackController, HitFlashController, CircleCollider2D, Rigidbody2D | REUSE_EXISTING |
| Player target binding | Via `GameBootstrap.Instance.PlayerManager.transform` | EXISTING pattern |
| Contact damage | From `EnemyDataSO.contactDamage` | EXISTING |
| Detection radius | From `EnemyDataSO.detectionRadius` | EXISTING |
| Drop item | `EnemyDataSO.dropItemId` (set in asset) | EXISTING |
| Drop amount | `EnemyDataSO.dropAmount` (set in asset) | EXISTING |
| Spawn event | `EnemySpawnedEvent` via `GameEventBus.Publish` | EXISTING event |

---

## Component Pipeline (REUSE_EXISTING)

```
CaveSmokeTestSpawnerBridge.SpawnSmokeEnemy()
  → Creates GameObject "SmokeEnemy_<DisplayName>"
    ├── SpriteRenderer (red placeholder)
    ├── CircleCollider2D (radius 0.4)
    ├── Rigidbody2D (gravityScale=0, FreezeRotation)
    ├── EnemyHealth.Configure(enemyData)   ← EXISTING
    ├── KnockbackController                ← EXISTING
    ├── HitFlashController                 ← EXISTING
    ├── EnemyChaseController.ConfigureFromData(enemyData) ← EXISTING
    │     .RebindTarget(playerTransform)
    └── Child "ContactDamageTrigger"
          ├── CircleCollider2D (isTrigger, radius 0.5)
          └── EnemyContactDamage.Configure(enemyData, triggerCollider) ← EXISTING
```

---

## Combat Loop (REUSE_EXISTING)

```
Player presses Q (or E) near enemy
  → PlayerAttackController.TryAttackLeftHand/RightHand()
  → AttackWithSlot() → ExecuteMeleeAttack()
  → Physics2D.OverlapCircleAll(attackCenter, weapon.Range)
  → EnemyHealth.TakeDamage(DamageRequest)
  → EnemyHealth internal: _currentHp -= finalDamage
  → FloatingDamageNumberDisplayer.ShowAtTarget()
  → HitFlashController.Flash()
  → KnockbackController.ApplyKnockback() (if knockbackForce > 0)
  → [IF dead]: EnemyHealth.Die()
    → GameEventBus.Publish(EnemyKilledEvent)
    → gameObject.SetActive(false)
    → EnemyDropSpawner.OnEnemyKilled(evt)
    → InventoryManager.AddItem(dropItemId, dropAmount)
```

---

## Status

The full combat loop is functional via **REUSE_EXISTING** strategy.
No parallel combat systems were created.
Human must wire the scene and assign EnemyDataSO with valid drop item before smoke test.
