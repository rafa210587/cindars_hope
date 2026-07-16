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
    /// arch: quebra do par mutuo Core|Player (2026-07-15) — StaminaManager/ManaManager tipados como
    /// MonoBehaviour (nao mais CindarsHope.Player.StaminaManager/ManaManager) para que Core pare de
    /// nomear CindarsHope.Player; CombatRuntimeInstaller so faz null-check (nao precisa do tipo
    /// concreto). Consumidores fora de Core castam localmente se precisarem da API completa.
    /// </summary>
    [Serializable]
    public class CombatRuntimeInstallContext
    {
        public ScriptableObject ItemDatabase;
        public WeaponDatabaseSO WeaponDatabase;
        public SpellDatabaseSO SpellDatabase;
        public StatusEffectDatabaseSO StatusEffectDatabase;
        public MonoBehaviour StaminaManager;
        public MonoBehaviour ManaManager;
    }
}
