using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Combat
{
    public class SpellCastService
    {
        private readonly ManaManager _manaManager;
        private readonly EquipmentManager _equipmentManager;
        private readonly EquippedItemResolver _itemResolver;
        private readonly float _knockbackForce;
        private readonly StatusEffectDatabaseSO _statusEffectDatabase;

        public SpellCastService(
            ManaManager manaManager,
            EquipmentManager equipmentManager,
            EquippedItemResolver itemResolver,
            float knockbackForce,
            StatusEffectDatabaseSO statusEffectDatabase = null)
        {
            _manaManager = manaManager;
            _equipmentManager = equipmentManager;
            _itemResolver = itemResolver;
            _knockbackForce = knockbackForce;
            _statusEffectDatabase = statusEffectDatabase;
        }

        public AttackResult TryCast(
            EquipmentSlot slot,
            ItemDataSO itemData,
            float lastAttackTime,
            Vector2 direction,
            Vector2 spawnPosition)
        {
            var spellData = _itemResolver?.ResolveEquippedSpell(itemData);
            if (spellData == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=SpellNotResolved, ItemId={itemData.Id}, SpellId='{itemData.SpellId}'");
                return AttackResult.CreateError("SpellNotResolved");
            }

            float cooldown = Mathf.Max(0.1f, spellData.CooldownSeconds);
            if (!CooldownHelper.IsCooldownExpired(lastAttackTime, cooldown))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=Cooldown, Slot={slot}, RemainingSeconds={CooldownHelper.GetRemainingCooldown(lastAttackTime, cooldown):F2}");
                return AttackResult.CreateError("Cooldown");
            }

            if (_manaManager != null && !_manaManager.TrySpendMana(spellData.ManaCost))
            {
                Debug.Log($"CombatLog: PlayerAttackBlocked. Reason=InsufficientMana, Slot={slot}, ManaCost={spellData.ManaCost}");
                return AttackResult.CreateError("InsufficientMana");
            }

            if (spellData.ProjectilePrefab == null)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=SpellHasNoProjectilePrefab, SpellId={spellData.Id}");
                return AttackResult.CreateError("SpellHasNoProjectilePrefab");
            }

            CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect = null;
            if (!string.IsNullOrEmpty(spellData.StatusEffectId))
            {
                if (_statusEffectDatabase != null && _statusEffectDatabase.TryGetById(spellData.StatusEffectId, out var dbEffect))
                    statusEffect = dbEffect;
                else
                    statusEffect = Resources.Load<CindarsHope.Combat.StatusEffect.StatusEffectSO>(spellData.StatusEffectId);
            }

            var spawnRequest = new ProjectileSpawnRequest(
                spellData.ProjectilePrefab,
                spawnPosition,
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
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}");
                return AttackResult.CreateError("ProjectileSpawnFailed", spawnResult.ErrorCode);
            }

            if (_equipmentManager != null)
                _equipmentManager.RegisterEquipmentUsage();

            Debug.Log($"CombatLog: SpellFired. Slot={slot}, Spell={spellData.Id}, Range={spellData.Range}, Speed={spellData.ProjectileSpeed}, ManaCost={spellData.ManaCost}");
            return AttackResult.CreateSuccess();
        }
    }
}
