using CindarsHope.Core.Data;
using CindarsHope.Equipment;
using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    [CreateAssetMenu(fileName = "Weapon_", menuName = "CindarsHope/Combat/Weapon")]
    public class WeaponDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;
        public WeaponType Type;
        public DamageType DamageType = DamageType.Physical;
        public int BaseDamage;
        public float BaseCooldownSeconds = 0.5f;
        public float StaminaCost = 10f;
        public float Range = 1f;
        public float ArcDegrees = 120f;
        public float AttackSpeedMultiplier = 1f;
        public int CriticalChance;
        public int CooldownMs;
        public int RequiredStrength;
        public int RequiredDexterity;
        public int BaseValue;
        public int Weight;
        public int DurabilityMax = 100;

        [Header("Projectile (for Bow/Ranged)")]
        public GameObject ProjectilePrefab;
        public float ProjectileSpeed = 10f;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            BaseDamage = Mathf.Max(1, BaseDamage);
            CriticalChance = Mathf.Clamp(CriticalChance, 0, 100);
            CooldownMs = Mathf.Max(100, CooldownMs);
            StaminaCost = Mathf.Max(0, StaminaCost);
            Weight = Mathf.Max(0, Weight);
        }
    }

    public enum WeaponType
    {
        None,
        Sword,
        Spear,
        Axe,
        Bow,
        Staff,
        Dagger
    }
}
