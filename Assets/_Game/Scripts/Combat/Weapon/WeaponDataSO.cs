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

        // F03 (emenda): campos canônicos EQUIPMENT_MECHANICAL_BASELINES §2.
        // Defaults NEUTROS — assets antigos se comportam igual até o gerador rodar.
        [Header("Canonical Baselines (F03)")]
        public CindarsHope.Player.Progression.PlayerAttributeType PrimaryAttribute = CindarsHope.Player.Progression.PlayerAttributeType.Strength;
        [Range(0f, 2f)] public float PrimaryAttributeWeight = 0f;
        public CindarsHope.Player.Progression.PlayerAttributeType SecondaryAttribute = CindarsHope.Player.Progression.PlayerAttributeType.Dexterity;
        [Range(0f, 2f)] public float SecondaryAttributeWeight = 0f;
        public float BaseLightStaminaCost = 0f;   // 0 = usar StaminaCost × razões globais (F02)
        public float BaseHeavyStaminaCost = 0f;
        public float BaseChargedStaminaCost = 0f;
        [Range(0f, 3f)] public float PostureDamageModifier = 1f;
        [Range(1f, 3f)] public float CritDamageModifier = 1.5f;
        public WeaponWeightClass WeightClass = WeaponWeightClass.Medium;
        public string[] MaterialTagsApplied = new string[0];
        public string[] StatusTagsApplied = new string[0];
        public string AllowedAmmoType = string.Empty;
        public DamageType[] AllowedDamageTypes = new DamageType[0];
        public string DefaultActionSet = string.Empty;
        public string ChargedEffectProfileId = string.Empty;

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
        // Legacy ordinal values are pinned explicitly so they can NEVER drift in saves/assets
        // serialized by integer (these were implicit None=0..Dagger=6 before fable_32).
        None = 0,
        Sword = 1,
        Spear = 2,
        Axe = 3,
        Bow = 4,
        Staff = 5,
        Dagger = 6,

        // fable_32 (EMENDA V3.3, decisao 2.7): new weapon TYPES for the canonical catalog
        // (hammer / wand / tool-as-weapon). Appended with explicit HIGH values so they never
        // collide with or shift Sword..Dagger. The combat-side baselines for these types are
        // owned by fable_03 (EQUIPMENT_MECHANICAL_BASELINES); here they are taxonomy only.
        Hammer = 100,
        Wand = 101,
        Tool = 102
    }

    // F03: classe de peso canônica (afeta feel/knockback futuro).
    public enum WeaponWeightClass
    {
        Light = 0,
        Medium = 1,
        Heavy = 2
    }
}
