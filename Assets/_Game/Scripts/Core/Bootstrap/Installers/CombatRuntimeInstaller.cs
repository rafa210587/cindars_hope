using UnityEngine;

namespace CindarsHope.Core.Bootstrap.Installers
{
    /// <summary>
    /// SPEC_11 Wave 6: Pilot installer for the combat domain.
    /// Called by GameBootstrap.InitializeManagers() to validate combat wiring explicitly.
    /// Logs errors for required missing references (FR-004: no silent fallback).
    /// Runtime components (PlayerAttackController, EnemyStatusRuntimeTicker) continue to
    /// self-wire via GameBootstrap.Instance — this installer validates at bootstrap time.
    /// </summary>
    public static class CombatRuntimeInstaller
    {
        /// <summary>
        /// Validates combat domain wiring. Must be called after all managers are initialized.
        /// </summary>
        /// <param name="context">Context built from GameBootstrap fields.</param>
        /// <param name="diagnosticOwner">Logging owner (the GameBootstrap MonoBehaviour).</param>
        public static void Install(CombatRuntimeInstallContext context, Object diagnosticOwner = null)
        {
            if (context == null)
            {
                Debug.LogError("CombatRuntimeInstaller: context is null. Combat domain wiring cannot be validated.", diagnosticOwner);
                return;
            }

            // Required — item resolution fails without ItemDatabase
            if (context.ItemDatabase == null)
                Debug.LogError("CombatRuntimeInstaller: ItemDatabase is null. Combat item resolution will fail at runtime.", diagnosticOwner);

            // Required — weapon resolution fails without WeaponDatabase
            if (context.WeaponDatabase == null)
                Debug.LogError("CombatRuntimeInstaller: WeaponDatabase is null. Weapon resolution will fail at runtime.", diagnosticOwner);

            // Required — spell/fireball fails without SpellDatabase
            if (context.SpellDatabase == null)
                Debug.LogError("CombatRuntimeInstaller: SpellDatabase is null. Spell resolution will fail at runtime.", diagnosticOwner);

            // Required — equipment combat wiring fails without EquipmentManager.
            // arch: Core|Equipment (spec_arch_core_equipment_cycle_reduction_v35) — resolvido via
            // EquipmentManager.Instance (self-registro, molde Craft/Economy/Skills).
            var equipmentManager = CindarsHope.Equipment.EquipmentManager.Instance;
            if (equipmentManager == null)
                Debug.LogError("CombatRuntimeInstaller: EquipmentManager is null. Equipment combat wiring will fail at runtime.", diagnosticOwner);

            // Optional — may be absent in non-combat scenes
            if (context.StaminaManager == null)
                Debug.LogWarning("CombatRuntimeInstaller: StaminaManager is null. Stamina cost checks will be skipped.", diagnosticOwner);

            // Optional — may be absent in non-spell scenes
            if (context.ManaManager == null)
                Debug.LogWarning("CombatRuntimeInstaller: ManaManager is null. Mana cost checks will be skipped.", diagnosticOwner);

            Debug.Log(
                $"CombatRuntimeInstaller: Install completed. " +
                $"ItemDb={context.ItemDatabase?.name ?? "null"}, " +
                $"WeaponDb={context.WeaponDatabase?.name ?? "null"}, " +
                $"SpellDb={context.SpellDatabase?.name ?? "null"}, " +
                $"StatusEffectDb={context.StatusEffectDatabase?.name ?? "null"}, " +
                $"EquipmentMgr={equipmentManager?.name ?? "null"}",
                diagnosticOwner);
        }
    }
}
