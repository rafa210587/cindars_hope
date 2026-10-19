# WAVE_INTEGRATION_17 — Player Combat Bridge

**Date:** 2026-06-10

---

## Strategy: USE_EXISTING_PLAYER_ATTACK

`PlayerAttackController` (`Assets/_Game/Scripts/Combat/PlayerAttackController.cs`) is fully functional for the cave combat smoke test.

---

## PlayerAttackController Audit

| Feature | Status | Detail |
|---------|--------|--------|
| Input keys | Q = left hand, E = right hand | EXISTING |
| Melee attack | Physics2D.OverlapCircleAll on facing direction | EXISTING |
| Enemy detection | GetComponentInParent/GetComponent EnemyHealth | EXISTING |
| Damage delivery | EnemyHealth.TakeDamage(DamageRequest) | EXISTING |
| Unarmed fallback | Runtime ScriptableObject created if null | EXISTING |
| Modal guard | GameBootstrap.Instance?.ModalManager?.HasActiveModal | EXISTING |
| Stamina guard | StaminaManager.TrySpendStamina(staminaCost) | EXISTING |
| Cooldown | CooldownHelper.IsCooldownExpired() | EXISTING |
| Facing direction | _playerController?.LastFacingDirection ?? Vector2.right | EXISTING |
| Ranged (bow) | ProjectileSpawnService | EXISTING |
| Spell | SpellCastService | EXISTING |
| Bootstrap wiring | Resolves EquipmentManager, StaminaManager, etc. from GameBootstrap | EXISTING |

---

## Attack Keys for Smoke Test

- **Q key** — left hand attack (default empty slot → unarmed fallback, range 0.6, damage 3)
- **E key** — right hand attack (same, but blocked if InteractionSystem has a candidate)

For the smoke test, player does not need any weapon equipped — the unarmed fallback (3 damage) will hit the enemy's ~10 HP slime and defeat it in ~4 hits.

---

## No Bridge Created

`PlayerBasicAttackBridge.cs` (proposed in spec) was **not created** because `PlayerAttackController` already provides this functionality completely. Creating it would violate the "NÃO criar combat system paralelo" rule.
