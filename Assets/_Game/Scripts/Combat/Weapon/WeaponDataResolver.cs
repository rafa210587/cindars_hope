using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    public static class WeaponDataResolver
    {
        public static WeaponDataSO ResolveWeapon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                return null;
            }

            var asset = Resources.Load<WeaponDataSO>($"Combat/Weapons/{weaponId}");
            if (asset != null)
            {
                return asset;
            }

            Debug.LogWarning($"WeaponDataResolver: Could not find weapon '{weaponId}' in Resources/Combat/Weapons/", null);
            return null;
        }

        public static UnarmedAttackDataSO ResolveUnarmed(string unarmedId = "unarmed_default")
        {
            if (string.IsNullOrEmpty(unarmedId))
            {
                return null;
            }

            var asset = Resources.Load<UnarmedAttackDataSO>($"Combat/Weapons/{unarmedId}");
            if (asset != null)
            {
                return asset;
            }

            Debug.LogWarning($"WeaponDataResolver: Could not find unarmed attack '{unarmedId}' in Resources/Combat/Weapons/", null);
            return null;
        }
    }
}
