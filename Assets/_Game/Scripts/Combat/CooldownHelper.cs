using CindarsHope.Combat.Weapon;
using UnityEngine;

namespace CindarsHope.Combat
{
    /// <summary>
    /// SPEC_05: Helper for cooldown calculations.
    /// Extracted to centralize cooldown logic without behavioral changes.
    /// </summary>
    public static class CooldownHelper
    {
        /// <summary>
        /// Calculate effective cooldown for a weapon, accounting for attack speed multiplier.
        /// </summary>
        public static float CalculateWeaponCooldown(WeaponDataSO weapon)
        {
            if (weapon == null) return 0f;
            float cooldown = weapon.BaseCooldownSeconds;
            float attackSpeed = weapon.AttackSpeedMultiplier;
            return cooldown / Mathf.Max(0.1f, attackSpeed);
        }

        /// <summary>
        /// Check if enough time has passed since last attack for cooldown to expire.
        /// </summary>
        public static bool IsCooldownExpired(float lastAttackTime, float cooldownSeconds)
        {
            return Time.time >= lastAttackTime + cooldownSeconds;
        }

        /// <summary>
        /// Get remaining cooldown time in seconds. Returns 0 if already expired.
        /// </summary>
        public static float GetRemainingCooldown(float lastAttackTime, float cooldownSeconds)
        {
            float remaining = lastAttackTime + cooldownSeconds - Time.time;
            return Mathf.Max(0f, remaining);
        }
    }
}
