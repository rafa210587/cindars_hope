using System;
using CindarsHope.Core.Data;
using CindarsHope.Inventory;
using CindarsHope.Player;

namespace CindarsHope.Core.Bootstrap.Installers
{
    /// <summary>
    /// SPEC_11 Wave 6: Context DTO for CombatRuntimeInstaller.
    /// Holds all references required for explicit combat domain wiring validation.
    /// Built by GameBootstrap from its own serialized fields — no separate scene wiring needed.
    /// arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager nao eh
    /// mais carregado aqui; CombatRuntimeInstaller resolve via EquipmentManager.Instance (self-registro,
    /// molde Craft/Economy/Skills).
    /// </summary>
    [Serializable]
    public class CombatRuntimeInstallContext
    {
        public ItemDatabaseSO ItemDatabase;
        public WeaponDatabaseSO WeaponDatabase;
        public SpellDatabaseSO SpellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase;
        public InventoryManager InventoryManager;
        public StaminaManager StaminaManager;
        public ManaManager ManaManager;
    }
}
