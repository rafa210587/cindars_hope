using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat.Magic
{
    [CreateAssetMenu(fileName = "Spell_", menuName = "CindarsHope/Combat/Spell")]
    public class SpellDataSO : ScriptableObject, IIdentifiedData
    {
        public string Id;
        public string SpellName;
        [TextArea] public string Description;
        public Sprite Icon;
        public SpellType Type;
        public DamageType DamageType = DamageType.Arcane;
        public int BaseDamage;
        public int ManaCost;
        public float CooldownSeconds = 1f;
        public float CastTimeSeconds = 0f;
        public float Range = 7f;
        public float ProjectileSpeed = 8f;
        public int CooldownMs;
        public int RequiredIntelligence;
        public int RequiredWillpower;
        public int BaseValue;
        public string StatusEffectId;
        public int CastRangeMeters = 10;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            BaseDamage = Mathf.Max(0, BaseDamage);
            ManaCost = Mathf.Max(1, ManaCost);
            CooldownMs = Mathf.Max(100, CooldownMs);
            RequiredIntelligence = Mathf.Max(1, RequiredIntelligence);
            RequiredWillpower = Mathf.Max(1, RequiredWillpower);
            CastRangeMeters = Mathf.Max(1, CastRangeMeters);
        }
    }

    public enum SpellType
    {
        None,
        Fireball,
        IceSpike,
        Lightning,
        Heal,
        Buff,
        Debuff
    }
}
