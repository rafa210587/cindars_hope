using System;
using CindarsHope.Core.Data;

namespace CindarsHope.Core.Bootstrap.Installers
{
    /// <summary>
    /// SPEC_11 Wave 6: Context DTO for CombatRuntimeInstaller.
    /// Holds all references required for explicit combat domain wiring validation.
    /// Built by GameBootstrap from its own serialized fields — no separate scene wiring needed.
    /// arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager nao eh
    /// mais carregado aqui; CombatRuntimeInstaller resolve via EquipmentManager.Instance (self-registro,
    /// molde Craft/Economy/Skills).
    /// arch: Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) — o campo InventoryManager
    /// foi removido: CombatRuntimeInstaller.Install nunca o lia (campo morto), e mante-lo aqui exigiria
    /// referenciar CindarsHope.Inventory nesta pasta Core, reintroduzindo a aresta Core->Inventory.
    /// arch: Core|Player (spec_arch_core_player_cycle_reduction_v37) — StaminaManager/ManaManager
    /// referenciados por nome totalmente qualificado (sem using CindarsHope.Player) para nao
    /// reintroduzir a aresta Core->Player.
    /// </summary>
    [Serializable]
    public class CombatRuntimeInstallContext
    {
        // arch: quebra do ciclo Core|Inventory (spec_arch_core_inventory_cycle_reduction_v36) — tipo
        // totalmente qualificado (sem using CindarsHope.Inventory) para nao reintroduzir a aresta
        // Core->Inventory; ItemDatabaseSO agora vive em CindarsHope.Inventory.Data.
        public CindarsHope.Inventory.Data.ItemDatabaseSO ItemDatabase;
        public WeaponDatabaseSO WeaponDatabase;
        public SpellDatabaseSO SpellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase;
        public CindarsHope.Player.StaminaManager StaminaManager;
        public CindarsHope.Player.ManaManager ManaManager;
    }
}
