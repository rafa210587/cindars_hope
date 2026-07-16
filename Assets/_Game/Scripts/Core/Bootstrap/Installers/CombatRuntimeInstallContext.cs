using System;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Core.Bootstrap.Installers
{
    /// <summary>
    /// SPEC_11 Wave 6: Context DTO for CombatRuntimeInstaller.
    /// Holds all references required for explicit combat domain wiring validation.
    /// Built by GameBootstrap from its own serialized fields — no separate scene wiring needed.
    /// arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — EquipmentManager nao eh
    /// mais carregado aqui; CombatRuntimeInstaller resolve via EquipmentManager.Instance (self-registro,
    /// molde Craft/Economy/Skills).
    /// arch: quebra do par mutuo Core|Inventory (2026-07-15) — ItemDatabase tipado como
    /// ScriptableObject (nao mais CindarsHope.Inventory.Data.ItemDatabaseSO) para que Core pare de
    /// nomear CindarsHope.Inventory; CombatRuntimeInstaller so faz null-check/.name (nao precisa do
    /// tipo concreto). Consumidores fora de Core (ex.: PlayerAttackController) castam localmente.
    /// arch: Core|Player (spec_arch_core_player_cycle_reduction_v37) — StaminaManager/ManaManager
    /// referenciados por nome totalmente qualificado (sem using CindarsHope.Player) para nao
    /// reintroduzir a aresta Core->Player.
    /// </summary>
    [Serializable]
    public class CombatRuntimeInstallContext
    {
        public ScriptableObject ItemDatabase;
        public WeaponDatabaseSO WeaponDatabase;
        public SpellDatabaseSO SpellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase;
        public CindarsHope.Player.StaminaManager StaminaManager;
        public CindarsHope.Player.ManaManager ManaManager;
    }
}
