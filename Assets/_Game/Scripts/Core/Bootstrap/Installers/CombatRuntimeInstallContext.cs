using System;
using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using CindarsHope.Inventory;
using CindarsHope.Player;

namespace CindarsHope.Core.Bootstrap.Installers
{
    /// <summary>
    /// SPEC_11 Wave 6: Context DTO for CombatRuntimeInstaller.
    /// Holds all references required for explicit combat domain wiring validation.
    /// Built by GameBootstrap from its own serialized fields — no separate scene wiring needed.
    /// </summary>
    [Serializable]
    public class CombatRuntimeInstallContext
    {
        public ItemDatabaseSO ItemDatabase;
        public WeaponDatabaseSO WeaponDatabase;
        public SpellDatabaseSO SpellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase;
        public EquipmentManager EquipmentManager;
        public InventoryManager InventoryManager;
        public StaminaManager StaminaManager;
        public ManaManager ManaManager;
    }
}
