using CindarsHope.Core;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Inventory.Data;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// SPEC_05: Simple context object carrying all dependencies needed for a combat action.
    /// Extracted to reduce parameter passing and improve testability.
    /// </summary>
    public class CombatActionContext
    {
        public EquipmentSlot Slot { get; set; }
        public string EquippedItemId { get; set; }
        public Vector2 Direction { get; set; }

        public ItemDatabaseSO ItemDatabase { get; set; }
        public WeaponDatabaseSO WeaponDatabase { get; set; }
        public SpellDatabaseSO SpellDatabase { get; set; }
        public EquipmentManager EquipmentManager { get; set; }
        public StaminaManager StaminaManager { get; set; }
        public ManaManager ManaManager { get; set; }
        public InventoryManager InventoryManager { get; set; }

        public CombatActionContext()
        {
        }

        public CombatActionContext(EquipmentSlot slot, string equippedItemId, Vector2 direction)
        {
            Slot = slot;
            EquippedItemId = equippedItemId;
            Direction = direction;
        }
    }
}
