using CindarsHope.Combat.StatusEffect;
using CindarsHope.Core.Data;
using UnityEngine;
using CindarsHope.Foundation;

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
        public SpellDiscipline Discipline = SpellDiscipline.None;
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

        // fable_08 â€” forma da magia + parÃ¢metros por shape. Defaults NEUTROS: Shape=Bolt e
        // CastTimeSeconds=0 (jÃ¡ existente) garantem que toda spell autorada antes desta spec
        // continue castando como o projÃ©til linear instantÃ¢neo de hoje (caracterizaÃ§Ã£o nos testes).
        [Header("Shape & Targeting (fable_08)")]
        public SpellShape Shape = SpellShape.Bolt;

        /// <summary>Auto-target (EMENDA 6.6-A): mira no inimigo elegÃ­vel mais prÃ³ximo/na mira. Bolt apenas.</summary>
        public bool AutoTarget = false;

        /// <summary>Cone: meia-abertura do leque em graus (e nÂº de projÃ©teis via ConeProjectileCount).</summary>
        [Range(5f, 170f)] public float ConeHalfAngleDegrees = 35f;

        /// <summary>Cone: quantidade de projÃ©teis disparados no leque.</summary>
        [Range(1, 9)] public int ConeProjectileCount = 3;

        /// <summary>Nova: raio da explosÃ£o radial 360Â° (OverlapCircleAll).</summary>
        public float NovaRadius = 3f;

        // SelfRestore â€” quantidades restauradas no prÃ³prio caster (0 = nÃ£o restaura aquele recurso).
        [Header("Self Restore (fable_08)")]
        public int RestoreHp = 0;
        public int RestoreStamina = 0;
        public int RestoreMana = 0;

        // Barrier â€” absorÃ§Ã£o temporÃ¡ria antes da defesa/resistÃªncia (PlayerBarrierState).
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

            // fable_08 â€” clamps neutros (nÃ£o mexem em assets Bolt: campos ficam em 0/defaults).
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
