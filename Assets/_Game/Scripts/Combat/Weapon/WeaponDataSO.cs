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
        public int BaseDamage;
        public int CriticalChance;
        public int CooldownMs;
        public float StaminaCost;
        public int RequiredStrength;
        public int RequiredDexterity;
        public int BaseValue;
        public int Weight;
        public int DurabilityMax = 100;

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
