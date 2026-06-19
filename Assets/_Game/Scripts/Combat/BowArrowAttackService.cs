using System.Collections.Generic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Core.Events;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Combat
{
    public class BowArrowAttackService
    {
        // F02: provider de stats derivados (setado pelo PlayerAttackController; null-safe).
        public PlayerCombatStatsProvider StatsProvider { get; set; }

        private readonly EquipmentManager _equipmentManager;
        private readonly InventoryManager _inventoryManager;
        private readonly StaminaManager _staminaManager;
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly EquippedItemResolver _itemResolver;
        // fable_48: resolve o StatusEffectSO da flecha (Burn/Chill) pela id canônica — MESMO caminho
        // do SpellCastService (DB + fallback Resources). Null-safe: sem DB, flecha elemental sai sem status.
        private readonly StatusEffectDatabaseSO _statusEffectDatabase;
        private readonly float _knockbackForce;

        public BowArrowAttackService(
            EquipmentManager equipmentManager,
            InventoryManager inventoryManager,
            StaminaManager staminaManager,
            ItemDatabaseSO itemDatabase,
            EquippedItemResolver itemResolver,
            float knockbackForce,
            StatusEffectDatabaseSO statusEffectDatabase = null)
        {
            _equipmentManager = equipmentManager;
            _inventoryManager = inventoryManager;
            _staminaManager = staminaManager;
            _itemDatabase = itemDatabase;
            _itemResolver = itemResolver;
            _knockbackForce = knockbackForce;
            _statusEffectDatabase = statusEffectDatabase;
        }

        public AttackResult TryFire(
            EquipmentSlot ammoSlot,
            ItemDataSO ammoItemData,
            float lastAttackTime,
            Vector2 direction,
            Vector2 spawnPosition)
        {
            var bowSlot = ammoSlot == EquipmentSlot.LeftHand ? EquipmentSlot.RightHand : EquipmentSlot.LeftHand;
            var bowItemId = _equipmentManager != null ? _equipmentManager.GetEquippedItem(bowSlot) : null;

            ItemDataSO bowItemData = null;
            if (!string.IsNullOrEmpty(bowItemId) && _itemDatabase != null)
                _itemDatabase.TryGetById(bowItemId, out bowItemData);

            var bowWeapon = bowItemData != null && !string.IsNullOrEmpty(bowItemData.WeaponId)
                ? _itemResolver?.LookupWeapon(bowItemData.WeaponId) : null;

            if (bowWeapon == null || bowWeapon.Type != WeaponType.Bow)
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=ArrowRequiresBowInOtherHand, AmmoSlot={ammoSlot}, BowSlot={bowSlot}");
                return AttackResult.CreateError("ArrowRequiresBowInOtherHand");
            }

            // Null prefab is allowed: ProjectileSpawnService falls back to RuntimeProjectileFactory
            // (procedural arrow visual) so archery works before art prefabs are authored.
            float cooldown = CooldownHelper.CalculateWeaponCooldown(bowWeapon);
            if (!CooldownHelper.IsCooldownExpired(lastAttackTime, cooldown))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Cooldown, Slot={ammoSlot}, RemainingSeconds={CooldownHelper.GetRemainingCooldown(lastAttackTime, cooldown):F2}");
                return AttackResult.CreateError("Cooldown");
            }

            if (_inventoryManager == null || !_inventoryManager.HasItem(ammoItemData.Id, 1))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=NoArrowsInInventory, AmmoItemId={ammoItemData.Id}");
                return AttackResult.CreateError("NoArrowsInInventory");
            }

            int staminaCost = Mathf.RoundToInt(bowWeapon.StaminaCost);
            if (_staminaManager != null && !_staminaManager.TrySpendStamina(staminaCost))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=InsufficientStamina, Slot={ammoSlot}, StaminaCost={staminaCost}");
                return AttackResult.CreateError("InsufficientStamina");
            }

            _inventoryManager.RemoveItem(ammoItemData.Id, 1);

            // fable_48: balística da flecha (§19 × §8) — ponto único. Dano somado ANTES dos derived
            // stats; tags/status/damageType vêm daqui (fallback WoodenArrow se a id for desconhecida).
            var ballistics = ArrowBallisticsResolver.Resolve(ammoItemData);

            // F02/F18 + fable_48: fórmula §18 — finalDamage = derive(bowWeaponDamage + bowBonus + arrowDamage).
            // SkillBonus/MaterialModifier já entram via derived stats (F02); arrowDamage é a parte da munição.
            var bowDamageBonus = StatsProvider != null ? Mathf.RoundToInt(StatsProvider.Current.BowDamageBonus) : 0;
            var preDeriveDamage = bowWeapon.BaseDamage + bowDamageBonus + ballistics.ArrowDamage;
            var finalDamage = StatsProvider != null
                ? StatsProvider.FinalDamage(preDeriveDamage, AttackWeight.Light, false, out _)
                : preDeriveDamage;
            var finalRange = bowWeapon.Range + (StatsProvider != null ? Mathf.Max(0f, StatsProvider.Current.BowRange) : 0f);

            // fable_48: a flecha define o DamageType (fire→Fire, frost→Ice, físicas→Physical). Físicas
            // herdam o do arco quando o resolver devolve Physical (preserva arcos elementais futuros).
            var damageType = ballistics.DamageType != DamageType.Physical ? ballistics.DamageType : bowWeapon.DamageType;

            // fable_48: tags da flecha + tags de material do arco (vocabulário único do matching F06).
            var appliedTags = CombineTags(bowWeapon.MaterialTagsApplied, ballistics.Tags);

            // fable_48: status elemental on-hit (fire→Burn, frost→Chill) com a chance canônica única.
            var statusEffect = ballistics.HasStatus ? ResolveStatusEffect(ballistics.StatusEffectId) : null;
            var statusChance = statusEffect != null ? ballistics.StatusChance : 0f;

            var spawnRequest = new ProjectileSpawnRequest(
                bowWeapon.ProjectilePrefab,
                spawnPosition,
                direction,
                bowWeapon.ProjectileSpeed,
                finalRange,
                finalDamage,
                damageType,
                _knockbackForce,
                spawnOffset: 0.5f,
                statusEffect: statusEffect,
                statusApplyChance: statusChance
            );
            spawnRequest.VisualStyle = ProjectileVisualStyle.Arrow;
            spawnRequest.AppliedTags = appliedTags;

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}");
                return AttackResult.CreateError("ProjectileSpawnFailed", spawnResult.ErrorCode);
            }

            if (_equipmentManager != null)
                _equipmentManager.RegisterEquipmentUsage();

            Debug.Log($"CombatLog: ArrowFired. AmmoSlot={ammoSlot}, BowWeapon={bowWeapon.Id}, Range={bowWeapon.Range}, Speed={bowWeapon.ProjectileSpeed}, AmmoId={ammoItemData.Id}, ArrowDamage={ballistics.ArrowDamage}, DamageType={damageType}, Tags=[{string.Join(",", appliedTags)}]");

            // fable_48: pilha equipada zerou? auto-equipa a próxima munição compatível (ordem canônica).
            if (!_inventoryManager.HasItem(ammoItemData.Id, 1))
            {
                AutoSelectNextAmmo(ammoSlot, ammoItemData, bowWeapon);
            }

            return AttackResult.CreateSuccess();
        }

        // fable_48: troca determinística pós-consumo. Sem munição compatível => mantém o slot atual
        // (a próxima TryFire bloqueia em NoArrowsInInventory). Publica AmmoAutoSelectedEvent (bus).
        private void AutoSelectNextAmmo(EquipmentSlot ammoSlot, ItemDataSO depletedAmmo, WeaponDataSO bowWeapon)
        {
            if (_equipmentManager == null || _inventoryManager == null)
            {
                return;
            }

            var requiredAmmoType = bowWeapon != null ? bowWeapon.AllowedAmmoType : depletedAmmo.AmmoType;
            var candidates = BuildAmmoCandidates();
            var nextAmmoId = ArrowAmmoSelector.SelectNextCompatible(depletedAmmo.Id, requiredAmmoType, candidates);

            if (string.IsNullOrEmpty(nextAmmoId))
            {
                Debug.Log($"CombatLog: AmmoAutoSelect. PreviousAmmoId={depletedAmmo.Id}, NewAmmoId=<none>, Reason=NoCompatibleAmmo");
                GameEventBus.Publish(new AmmoAutoSelectedEvent(depletedAmmo.Id, null));
                return;
            }

            _equipmentManager.EquipItem(ammoSlot, nextAmmoId);
            Debug.Log($"CombatLog: AmmoAutoSelect. PreviousAmmoId={depletedAmmo.Id}, NewAmmoId={nextAmmoId}, Slot={ammoSlot}");
            GameEventBus.Publish(new AmmoAutoSelectedEvent(depletedAmmo.Id, nextAmmoId));
        }

        // Snapshot do inventário como candidatos de munição (id + AmmoType + estoque). Sem GameObject.Find:
        // lê os slots do InventoryManager injetado e resolve o AmmoType via ItemDatabase.
        private List<AmmoCandidate> BuildAmmoCandidates()
        {
            var candidates = new List<AmmoCandidate>();
            var slots = _inventoryManager.Slots;
            if (slots == null)
            {
                return candidates;
            }

            foreach (var slot in slots)
            {
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                if (!_inventoryManager.TryGetItemData(slot.ItemId, out var itemData) || itemData == null)
                {
                    continue;
                }

                if (itemData.Category != ItemCategory.Ammo || string.IsNullOrEmpty(itemData.AmmoType))
                {
                    continue;
                }

                candidates.Add(new AmmoCandidate(itemData.Id, itemData.AmmoType, slot.Amount));
            }

            return candidates;
        }

        // fable_48: tags do arco (material) + tags da flecha, sem duplicar. Vocabulário único F06.
        private static string[] CombineTags(string[] weaponTags, string[] arrowTags)
        {
            var combined = new List<string>();
            void AddRange(string[] source)
            {
                if (source == null)
                {
                    return;
                }
                foreach (var tag in source)
                {
                    if (!string.IsNullOrEmpty(tag) && !combined.Contains(tag))
                    {
                        combined.Add(tag);
                    }
                }
            }

            AddRange(weaponTags);
            AddRange(arrowTags);
            return combined.ToArray();
        }

        // fable_48: MESMO caminho de resolução do SpellCastService (DB canônico + fallback Resources).
        private CindarsHope.Combat.StatusEffect.StatusEffectSO ResolveStatusEffect(string statusEffectId)
        {
            if (string.IsNullOrEmpty(statusEffectId))
            {
                return null;
            }

            if (_statusEffectDatabase != null && _statusEffectDatabase.TryGetById(statusEffectId, out var dbEffect))
            {
                return dbEffect;
            }

            return Resources.Load<CindarsHope.Combat.StatusEffect.StatusEffectSO>(statusEffectId);
        }
    }
}
