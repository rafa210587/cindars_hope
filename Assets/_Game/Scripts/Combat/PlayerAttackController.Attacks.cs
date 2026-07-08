using CindarsHope.Combat.Magic;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using CindarsHope.Interaction;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using CindarsHope.Skills;
using UnityEngine;

namespace CindarsHope.Combat
{
    public partial class PlayerAttackController
    {
        private void AttackWithSlot(EquipmentSlot slot, string equippedItemId, AttackWeight weight = AttackWeight.Light)
        {
            if (_attackCore.IsAttackBlockedByDodge())
            {
                CombatLog.Log($"CombatLog: PlayerAttackBlocked. Reason=Dodging, Slot={slot}", this);
                return;
            }

            // Resolve ItemDataSO first to categorize the equipped item.
            ItemDataSO itemData = null;
            if (!string.IsNullOrEmpty(equippedItemId) && _itemDatabase != null)
                _itemDatabase.TryGetById(equippedItemId, out itemData);

            // Lookup weapon refs needed by the core dispatch (may be null).
            WeaponDataSO bowCheck = null;
            WeaponDataSO wandCheck = null;
            if (itemData != null && itemData.Category == ItemCategory.Weapon)
            {
                if (!string.IsNullOrEmpty(itemData.WeaponId))
                    bowCheck = LookupWeapon(itemData.WeaponId);
                if (!string.IsNullOrEmpty(itemData.SpellId))
                    wandCheck = LookupWeapon(itemData.WeaponId);
            }

            var attackPath = _attackCore.ResolveAttackPath(itemData, bowCheck, wandCheck);

            switch (attackPath)
            {
                case AttackPath.Bow:
                    TryExecuteArrowAttack(slot, itemData);
                    return;
                case AttackPath.Spell:
                case AttackPath.WandSpell:
                    TryExecuteSpellAttack(slot, itemData);
                    return;
                case AttackPath.BlockedBowHand:
                    CombatLog.Log($"CombatLog: PlayerAttackBlocked. Reason=BowHandPressed_UseArrowHand, Slot={slot}", this);
                    return;
                case AttackPath.BlockedNonWeapon:
                    CombatLog.Log($"CombatLog: PlayerAttackBlocked. Reason=EquippedItemIsNotAWeapon, Slot={slot}, EquippedInstanceId={equippedItemId}, Category={itemData?.Category}", this);
                    return;
            }

            // AttackPath.Melee or AttackPath.BlockedWeaponNotResolved â€” proceed to weapon resolution.
            WeaponDataSO weapon = ResolveEquippedWeapon(slot, equippedItemId, out string resolveError);

            // CASE A: Slot is empty (nothing equipped) -> use unarmed fallback.
            // CASE B: Something IS equipped but didn't resolve -> ERROR + abort (do not silently fall to unarmed).
            // CASE C: Resolved weapon -> attack with it.
            if (weapon == null && string.IsNullOrEmpty(equippedItemId))
            {
                if (_unarmedFallback == null)
                {
                    Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=NoWeaponEquippedAndNoUnarmedFallback, Slot={slot}", this);
                    return;
                }
                weapon = ConvertUnarmedToWeapon(_unarmedFallback);
            }
            else if (weapon == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=WeaponEquippedButNotResolved, Slot={slot}, EquippedInstanceId={equippedItemId}, ResolveError={resolveError}", this);
                return;
            }

            // SPEC_07B: Block bow from normal weapon path â€” must use bow+arrow path instead
            if (weapon.Type == WeaponType.Bow)
            {
                CombatLog.Log($"CombatLog: PlayerAttackBlocked. Reason=BowHandPressed_UseArrowHand, Slot={slot}", this);
                return;
            }

            // F02/F03: cooldown final via AttackSpeed derivado Ã— ASPD da arma.
            float cooldown = CooldownHelper.CalculateWeaponCooldown(weapon);
            if (_statsProvider != null)
            {
                cooldown = _statsProvider.FinalCooldown(cooldown, weapon);
            }

            if (!_attackCore.CanAttackSlot(slot, Time.time, cooldown))
            {
                CombatLog.Log($"CombatLog: PlayerAttackBlocked. Reason=Cooldown, Slot={slot}", this);
                return;
            }

            // F03: custos canÃ´nicos POR ARMA quando autorados; senÃ£o razÃµes F02.
            int staminaCost = PlayerCombatStatsProvider.WeaponStaminaCost(weapon, weight);
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                CombatLog.Log($"CombatLog: PlayerAttackBlocked. Reason=InsufficientStamina, Slot={slot}, StaminaCost={staminaCost}", this);
                return;
            }

            CombatLog.Log($"CombatLog: PlayerAttackStarted. Slot={slot}, Weapon={weapon.DisplayName}, BaseDamage={weapon.BaseDamage}, Weight={weight}, Range={weapon.Range:F2}, Type={weapon.Type}", this);
            GameEventBus.Publish(new PlayerChargedAttackEvent((int)weight));
            // Animacao de ataque do player: direcao pelo facing, duracao = cooldown efetivo
            // (atrela a velocidade da anim ao attack speed). Arquetipo derivado do tipo da arma.
            Vector2 swingDirection = _playerController != null ? _playerController.LastFacingDirection : Vector2.right;
            var archetype = WeaponAttackArchetypeMapper.FromWeaponType(weapon.Type);
            GameEventBus.Publish(new PlayerMeleeSwingEvent(swingDirection, cooldown, archetype));
            // fable_22: passa a instÃ¢ncia equipada para o ponto Ãºnico de tags (infusÃ£o de tÃªmpera).
            ExecuteWeaponAttack(weapon, weight, equippedItemId);
            _attackCore.RecordAttack(slot, Time.time);
        }

        // SPEC_07: Delegated to BowArrowAttackService
        private void TryExecuteArrowAttack(EquipmentSlot ammoSlot, ItemDataSO ammoItemData)
        {
            if (_bowArrowService == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=BowArrowServiceNull, Slot={ammoSlot}", this);
                return;
            }
            Vector2 direction = _playerController?.LastFacingDirection ?? Vector2.right;
            float lastTime = _attackCore.GetLastAttackTime(ammoSlot);
            var result = _bowArrowService.TryFire(ammoSlot, ammoItemData, lastTime, direction, transform.position);
            if (result.Success)
                _attackCore.RecordAttack(ammoSlot, Time.time);
        }

        // SPEC_07: Delegated to SpellCastService
        private void TryExecuteSpellAttack(EquipmentSlot slot, ItemDataSO itemData)
        {
            if (_spellCastService == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=SpellCastServiceNull, Slot={slot}", this);
                return;
            }
            Vector2 direction = _playerController?.LastFacingDirection ?? Vector2.right;

            // fable_08: valida/reserva (cooldown, mana, conhecimento). Em sucesso a rotina cuida da
            // janela de cast time + interrupt; cast 0s resolve imediatamente dentro do BeginCast.
            float lastTime = _attackCore.GetLastAttackTime(slot);
            var begin = _spellCastService.TryBeginCast(slot, itemData, lastTime, direction, transform.position, out var plan);
            if (!begin.Success || plan == null)
            {
                return;
            }

            if (_spellCastRoutine != null)
            {
                if (!_spellCastRoutine.BeginCast(_spellCastService, plan))
                {
                    // JÃ¡ conjurando outra magia: reembolsa a mana reservada deste plano.
                    _spellCastService.RefundCast(plan);
                    return;
                }
            }
            else
            {
                // Fallback sem rotina (nÃ£o deveria ocorrer em cena): resolve direto.
                _spellCastService.ResolveCast(plan);
            }

            _attackCore.RecordAttack(slot, Time.time);
        }

        // SPEC_05: Delegated to EquippedItemResolver
        // SPEC_05B: Added null guard for resolver safety.
        private SpellDataSO ResolveEquippedSpell(ItemDataSO itemData)
        {
            if (_itemResolver == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ItemResolverNull", this);
                return null;
            }
            return _itemResolver.ResolveEquippedSpell(itemData);
        }

        private static EquipmentSlot GetOppositeHand(EquipmentSlot slot)
        {
            return slot == EquipmentSlot.LeftHand ? EquipmentSlot.RightHand : EquipmentSlot.LeftHand;
        }

        private void ExecuteWeaponAttack(WeaponDataSO weapon, AttackWeight weight = AttackWeight.Light, string equippedItemId = null)
        {
            Vector2 direction = _playerController?.LastFacingDirection ?? Vector2.right;

            if (weapon.Type == WeaponType.Bow && weapon.ProjectilePrefab != null)
            {
                ExecuteRangedAttack(weapon, direction);
            }
            else
            {
                ExecuteMeleeAttack(weapon, direction, weight, equippedItemId);
            }

            if (_equipmentManager != null)
                _equipmentManager.RegisterEquipmentUsage();
        }

        // fable_22: resolve as tags efetivas da arma para o matching F06 = tags base do WeaponDataSO
        // + (se houver) a tag de gume canÃ´nica da INFUSÃƒO de tÃªmpera da instÃ¢ncia equipada. Ponto
        // ÃšNICO de injeÃ§Ã£o da tag de infusÃ£o; nenhum multiplicador paralelo Ã© criado (a tag entra
        // no matching como qualquer outra). Ã“leo SUPRIME a tÃªmpera: ver ResolveActiveWeaponTags.
        private string[] ResolveWeaponMaterialTags(WeaponDataSO weapon, string equippedItemId)
        {
            var baseTags = weapon != null ? weapon.MaterialTagsApplied : null;
            string edgeTag = ResolveInfusionEdgeTag(equippedItemId);
            if (string.IsNullOrEmpty(edgeTag))
            {
                return baseTags;
            }

            int baseLen = baseTags?.Length ?? 0;
            var combined = new string[baseLen + 1];
            if (baseLen > 0)
            {
                System.Array.Copy(baseTags, combined, baseLen);
            }
            combined[baseLen] = edgeTag;
            return combined;
        }

        // fable_22: tag de gume da infusÃ£o da instÃ¢ncia equipada, ou null se sem tÃªmpera. LÃª do
        // acessor Ãºnico WeaponInfusionRegistry.Active (sem busca global de cena).
        private static string ResolveInfusionEdgeTag(string equippedItemId)
        {
            if (string.IsNullOrEmpty(equippedItemId)) return null;
            var registry = CindarsHope.Economy.WeaponInfusionRegistry.Active;
            return registry != null ? registry.GetEdgeTag(equippedItemId) : null;
        }

        private void ExecuteMeleeAttack(WeaponDataSO weapon, Vector2 direction, AttackWeight weight = AttackWeight.Light, string equippedItemId = null)
        {
            Vector2 attackCenter = (Vector2)transform.position + direction * 0.5f;
            // spec_codex_13: ContactFilter2D (mask "Enemy" com fallback NoFilter) + buffer
            // pre-alocado reutilizavel (OverlapCircle NonAlloc) — sem alocacao por ataque.
            int candidatesTotal = Physics2D.OverlapCircle(attackCenter, weapon.Range, EnemyContactFilter, _combatQueryBuffer);

            int hitEnemies = 0;
            for (int i = 0; i < candidatesTotal; i++)
            {
                var collider = _combatQueryBuffer[i];
                if (collider == null || collider.gameObject == gameObject)
                    continue;

                var enemyHealth = collider.GetComponentInParent<EnemyHealth>() ?? collider.GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                    continue;

                CombatLog.Log($"CombatLog: PlayerAttackHitCandidate. EnemyId={enemyHealth.EnemyId}, EnemyHP={enemyHealth.CurrentHp}/{enemyHealth.MaxHp}, Distance={Vector2.Distance(attackCenter, collider.transform.position):F2}", this);

                // F02: dano final = (base + Attack derivado) Ã— peso Ã— crÃ­tico canÃ´nico.
                // Janela de vulnerabilidade aberta (CoreExposed) GARANTE crÃ­tico (emenda).
                var vulnerability = enemyHealth.GetComponent<CindarsHope.Enemy.EnemyVulnerabilityState>();
                var guaranteedCrit = vulnerability != null && vulnerability.IsVulnerable;
                var finalDamage = weapon.BaseDamage;
                var isCrit = false;
                if (_statsProvider != null)
                {
                    // F03: inclui scaling por atributo da arma.
                    finalDamage = _statsProvider.FinalDamage(weapon, weight, guaranteedCrit, out isCrit);
                }

                var damageRequest = new DamageRequest(enemyHealth.EnemyId, finalDamage)
                {
                    DamageType = weapon.DamageType,
                    SourcePosition = transform.position,
                    KnockbackForce = _knockbackForce,
                    // fable_06: tags de material da arma (ex.: prata) para matching de vulnerabilidade.
                    // fable_22: + tag de gume da tÃªmpera (FireEdge/...) quando a instÃ¢ncia estÃ¡ infundida.
                    WeaponMaterialTags = ResolveWeaponMaterialTags(weapon, equippedItemId)
                };

                int hpBefore = enemyHealth.CurrentHp;
                enemyHealth.TakeDamage(damageRequest);
                hitEnemies++;

                // F02: dano de posture por peso (quebra â†’ stagger + CoreExposed).
                var posture = enemyHealth.GetComponent<EnemyPostureState>();
                if (posture != null)
                {
                    posture.ApplyPostureDamage(weapon.BaseDamage * AttackChargeRules.PostureMultiplier(weight));
                }

                CombatLog.Log($"CombatLog: PlayerAttackDamageApplied. EnemyId={enemyHealth.EnemyId}, BaseDamage={weapon.BaseDamage}, FinalDamage={finalDamage}, Weight={weight}, Crit={isCrit}, HP={hpBefore}->{enemyHealth.CurrentHp}", this);
            }

            if (hitEnemies == 0)
            {
                CombatLog.Log($"CombatLog: PlayerAttackMissed. Reason={(candidatesTotal == 0 ? "NoCollidersInRange" : "NoEnemyHealthInColliders")}, AttackCenter={attackCenter}, Range={weapon.Range:F2}, CollidersSeen={candidatesTotal}, Direction={direction}", this);
            }
        }

        private void ExecuteRangedAttack(WeaponDataSO weapon, Vector2 direction)
        {
            // SPEC_06: Use ProjectileSpawnService to centralize spawn logic
            var spawnRequest = new ProjectileSpawnRequest(
                weapon.ProjectilePrefab,
                (Vector2)transform.position,
                direction,
                weapon.ProjectileSpeed,
                weapon.Range,
                weapon.BaseDamage,
                weapon.DamageType,
                _knockbackForce,
                spawnOffset: 0.5f
            );

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}", this);
            }
        }

        private void ExecuteSpellAttack(SpellDataSO spellData, Vector2 direction)
        {
            if (spellData.ProjectilePrefab == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=SpellHasNoProjectilePrefab, SpellId={spellData.Id}", this);
                return;
            }

            // SPEC_06: Use ProjectileSpawnService to centralize spawn logic
            CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect = null;
            if (!string.IsNullOrEmpty(spellData.StatusEffectId))
                statusEffect = Resources.Load<CindarsHope.Combat.StatusEffect.StatusEffectSO>(spellData.StatusEffectId);

            var spawnRequest = new ProjectileSpawnRequest(
                spellData.ProjectilePrefab,
                (Vector2)transform.position,
                direction,
                spellData.ProjectileSpeed,
                spellData.Range,
                spellData.BaseDamage,
                spellData.DamageType,
                _knockbackForce,
                spawnOffset: 0.5f,
                statusEffect: statusEffect,
                statusApplyChance: spellData.StatusApplyChance
            );

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}", this);
            }
        }

    }
}