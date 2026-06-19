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
        public float StatusApplyChance = 0f;
        public GameObject ProjectilePrefab;
        public int CastRangeMeters = 10;

        // fable_08 — forma da magia + parâmetros por shape. Defaults NEUTROS: Shape=Bolt e
        // CastTimeSeconds=0 (já existente) garantem que toda spell autorada antes desta spec
        // continue castando como o projétil linear instantâneo de hoje (caracterização nos testes).
        [Header("Shape & Targeting (fable_08)")]
        public SpellShape Shape = SpellShape.Bolt;

        /// <summary>Auto-target (EMENDA 6.6-A): mira no inimigo elegível mais próximo/na mira. Bolt apenas.</summary>
        public bool AutoTarget = false;

        /// <summary>Cone: meia-abertura do leque em graus (e nº de projéteis via ConeProjectileCount).</summary>
        [Range(5f, 170f)] public float ConeHalfAngleDegrees = 35f;

        /// <summary>Cone: quantidade de projéteis disparados no leque.</summary>
        [Range(1, 9)] public int ConeProjectileCount = 3;

        /// <summary>Nova: raio da explosão radial 360° (OverlapCircleAll).</summary>
        public float NovaRadius = 3f;

        // SelfRestore — quantidades restauradas no próprio caster (0 = não restaura aquele recurso).
        [Header("Self Restore (fable_08)")]
        public int RestoreHp = 0;
        public int RestoreStamina = 0;
        public int RestoreMana = 0;

        // Barrier — absorção temporária antes da defesa/resistência (PlayerBarrierState).
        [Header("Barrier (fable_08)")]
        public int BarrierAbsorb = 0;
        public float BarrierSeconds = 0f;

        string IIdentifiedData.Id => Id;

        private void OnValidate()
        {
            BaseDamage = Mathf.Max(0, BaseDamage);
            ManaCost = Mathf.Max(1, ManaCost);
            CooldownMs = Mathf.Max(100, CooldownMs);
            RequiredIntelligence = Mathf.Max(1, RequiredIntelligence);
            RequiredWillpower = Mathf.Max(1, RequiredWillpower);
            CastRangeMeters = Mathf.Max(1, CastRangeMeters);

            // fable_08 — clamps neutros (não mexem em assets Bolt: campos ficam em 0/defaults).
            CastTimeSeconds = Mathf.Max(0f, CastTimeSeconds);
            ConeProjectileCount = Mathf.Max(1, ConeProjectileCount);
            NovaRadius = Mathf.Max(0f, NovaRadius);
            RestoreHp = Mathf.Max(0, RestoreHp);
            RestoreStamina = Mathf.Max(0, RestoreStamina);
            RestoreMana = Mathf.Max(0, RestoreMana);
            BarrierAbsorb = Mathf.Max(0, BarrierAbsorb);
            BarrierSeconds = Mathf.Max(0f, BarrierSeconds);
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
