using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
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
        private readonly float _knockbackForce;

        public BowArrowAttackService(
            EquipmentManager equipmentManager,
            InventoryManager inventoryManager,
            StaminaManager staminaManager,
            ItemDatabaseSO itemDatabase,
            EquippedItemResolver itemResolver,
            float knockbackForce)
        {
            _equipmentManager = equipmentManager;
            _inventoryManager = inventoryManager;
            _staminaManager = staminaManager;
            _itemDatabase = itemDatabase;
            _itemResolver = itemResolver;
            _knockbackForce = knockbackForce;
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

            // F02/F18: dano final via stats derivados + bônus de arco (BowDamageBonus/BowRange).
            var bowDamageBonus = StatsProvider != null ? Mathf.RoundToInt(StatsProvider.Current.BowDamageBonus) : 0;
            var finalDamage = StatsProvider != null
                ? StatsProvider.FinalDamage(bowWeapon.BaseDamage + bowDamageBonus, AttackWeight.Light, false, out _)
                : bowWeapon.BaseDamage;
            var finalRange = bowWeapon.Range + (StatsProvider != null ? Mathf.Max(0f, StatsProvider.Current.BowRange) : 0f);

            var spawnRequest = new ProjectileSpawnRequest(
                bowWeapon.ProjectilePrefab,
                spawnPosition,
                direction,
                bowWeapon.ProjectileSpeed,
                finalRange,
                finalDamage,
                bowWeapon.DamageType,
                _knockbackForce,
                spawnOffset: 0.5f
            );
            spawnRequest.VisualStyle = ProjectileVisualStyle.Arrow;

            var spawnResult = ProjectileSpawnService.SpawnProjectile(spawnRequest);
            if (!spawnResult.Success)
            {
                Debug.LogError($"CombatLog: PlayerAttackBlocked. Reason=ProjectileSpawnFailed, ErrorCode={spawnResult.ErrorCode}");
                return AttackResult.CreateError("ProjectileSpawnFailed", spawnResult.ErrorCode);
            }

            if (_equipmentManager != null)
                _equipmentManager.RegisterEquipmentUsage();

            Debug.Log($"CombatLog: ArrowFired. AmmoSlot={ammoSlot}, BowWeapon={bowWeapon.Id}, Range={bowWeapon.Range}, Speed={bowWeapon.ProjectileSpeed}, AmmoId={ammoItemData.Id}");
            return AttackResult.CreateSuccess();
        }
    }
}
