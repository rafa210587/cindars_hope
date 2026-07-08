using CindarsHope.Combat.Magic;
using CindarsHope.Combat.Weapon;
using CindarsHope.Core.Data;
using CindarsHope.Foundation;
using CindarsHope.Inventory.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// SPEC_05: Extracted resolver for equipment items, weapons, and spells.
    /// Centralizes resolution chain: itemInstanceId -> ItemDataSO -> WeaponDataSO/SpellDataSO.
    /// No logic changes; pure extraction to reduce PlayerAttackController complexity.
    /// </summary>
    public class EquippedItemResolver
    {
        private readonly ItemDatabaseSO _itemDatabase;
        private readonly WeaponDatabaseSO _weaponDatabase;
        private readonly SpellDatabaseSO _spellDatabase;
        private readonly WeaponDataSO[] _knownWeapons;

        public EquippedItemResolver(
            ItemDatabaseSO itemDatabase,
            WeaponDatabaseSO weaponDatabase,
            SpellDatabaseSO spellDatabase,
            WeaponDataSO[] knownWeapons = null)
        {
            _itemDatabase = itemDatabase;
            _weaponDatabase = weaponDatabase;
            _spellDatabase = spellDatabase;
            _knownWeapons = knownWeapons ?? new WeaponDataSO[0];
        }

        public WeaponDataSO ResolveEquippedWeapon(EquipmentSlot slot, string equippedItemId, out string error)
        {
            error = null;

            if (string.IsNullOrEmpty(equippedItemId))
            {
                CombatLog.Log($"CombatLog: PlayerAttackResolveSlot. Slot={slot}, EquippedInstanceId=<empty>");
                return null;
            }

            CombatLog.Log($"CombatLog: PlayerAttackResolveSlot. Slot={slot}, EquippedInstanceId={equippedItemId}");

            ItemDataSO itemData = null;
            string weaponLookupId = equippedItemId;
            bool wentThroughItemDatabase = false;

            if (_itemDatabase != null && _itemDatabase.TryGetById(equippedItemId, out itemData) && itemData != null)
            {
                wentThroughItemDatabase = true;
                CombatLog.Log($"CombatLog: PlayerAttackResolveItemData. ItemInstanceId={equippedItemId}, ItemDataId={itemData.Id}, ItemType={itemData.Category}, WeaponId='{itemData.WeaponId}'");
                if (!string.IsNullOrEmpty(itemData.WeaponId))
                {
                    weaponLookupId = itemData.WeaponId;
                }
                else
                {
                    error = $"Item '{equippedItemId}' (Category={itemData.Category}) is not a weapon — WeaponId is empty.";
                    CombatLog.Log($"CombatLog: PlayerAttackResolveWeapon. WeaponId=<none>, WeaponResolved=False, Reason=ItemNotWeapon");
                    return null;
                }
            }
            else
            {
                CombatLog.Log($"CombatLog: PlayerAttackResolveItemData. ItemInstanceId={equippedItemId}, ItemDataId=<not_in_itemdb>, FallingBackToDirectWeaponLookup=True");
            }

            var weapon = LookupWeapon(weaponLookupId);
            CombatLog.Log($"CombatLog: PlayerAttackResolveWeapon. WeaponId={weaponLookupId}, WeaponResolved={weapon != null}, ViaItemDb={wentThroughItemDatabase}");

            if (weapon == null)
            {
                error = $"Could not resolve WeaponDataSO for WeaponId='{weaponLookupId}' (from EquippedInstanceId='{equippedItemId}'). " +
                        $"_itemDatabase assigned={_itemDatabase != null}, _weaponDatabase assigned={_weaponDatabase != null}, _knownWeapons count={(_knownWeapons?.Length ?? 0)}.";
            }
            return weapon;
        }

        public WeaponDataSO LookupWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return null;
            if (_weaponDatabase != null && _weaponDatabase.TryGetById(weaponId, out var fromDb) && fromDb != null) return fromDb;
            if (_knownWeapons != null)
            {
                foreach (var w in _knownWeapons)
                {
                    if (w != null && w.Id == weaponId) return w;
                }
            }
            return null;
        }

        public SpellDataSO ResolveEquippedSpell(ItemDataSO itemData)
        {
            if (itemData == null || string.IsNullOrEmpty(itemData.SpellId))
                return null;

            if (_spellDatabase != null && _spellDatabase.TryGetById(itemData.SpellId, out var spell) && spell != null)
                return spell;

            return null;
        }
    }
}
