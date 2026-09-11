using CindarsHope.Combat;
using CindarsHope.Combat.Weapon;
using CindarsHope.Equipment;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public readonly struct MeleeEquipmentReadiness
    {
        public bool CanExecute { get; }
        public string FailureKey { get; }

        private MeleeEquipmentReadiness(bool canExecute, string failureKey)
        {
            CanExecute = canExecute;
            FailureKey = failureKey ?? string.Empty;
        }

        public static MeleeEquipmentReadiness Ready()
            => new MeleeEquipmentReadiness(true, string.Empty);

        public static MeleeEquipmentReadiness Failed(string failureKey)
            => new MeleeEquipmentReadiness(false, failureKey);
    }

    /// <summary>Read-only readiness gate for melee skills with an equipment prerequisite.</summary>
    public sealed class MeleeEquipmentGate
    {
        public const string OffhandCutActionId = "skill_melee_offhand_cut";
        public const string RequiresOffhandFailureKey = "requires_offhand";

        private readonly EquipmentManager _equipmentManager;
        private readonly EquippedItemResolver _itemResolver;

        public MeleeEquipmentGate(EquipmentManager equipmentManager, EquippedItemResolver itemResolver)
        {
            _equipmentManager = equipmentManager;
            _itemResolver = itemResolver;
        }

        public MeleeEquipmentReadiness Evaluate(GameObject caster, string actionId)
        {
            if (!string.Equals(actionId, OffhandCutActionId, System.StringComparison.Ordinal))
                return MeleeEquipmentReadiness.Ready();

            if (caster == null || !caster.activeInHierarchy)
                return MeleeEquipmentReadiness.Failed(RequiresOffhandFailureKey);

            var equipment = _equipmentManager;
            if (equipment == null)
                return MeleeEquipmentReadiness.Failed(RequiresOffhandFailureKey);

            string itemInstanceId = equipment.GetEquippedItem(EquipmentSlot.LeftHand);
            if (string.IsNullOrWhiteSpace(itemInstanceId))
                return MeleeEquipmentReadiness.Failed(RequiresOffhandFailureKey);

            var weapon = _itemResolver?.ResolveEquippedWeapon(EquipmentSlot.LeftHand, itemInstanceId, out _);
            if (!IsLightMeleeWeapon(weapon))
                return MeleeEquipmentReadiness.Failed(RequiresOffhandFailureKey);

            var durability = equipment.GetItemDurability(itemInstanceId);
            if (durability == null || durability.IsBroken)
                return MeleeEquipmentReadiness.Failed(RequiresOffhandFailureKey);

            return MeleeEquipmentReadiness.Ready();
        }

        public static bool IsLightMeleeWeapon(WeaponDataSO weapon)
        {
            if (weapon == null || weapon.WeightClass != WeaponWeightClass.Light)
                return false;

            return weapon.Type != WeaponType.None
                && weapon.Type != WeaponType.Bow
                && weapon.Type != WeaponType.Staff
                && weapon.Type != WeaponType.Wand
                && weapon.Type != WeaponType.Tool;
        }
    }
}
