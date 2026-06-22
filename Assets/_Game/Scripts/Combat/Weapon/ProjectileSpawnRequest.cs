using CindarsHope.Combat.StatusEffect;
using UnityEngine;

namespace CindarsHope.Combat.Weapon
{
    /// <summary>
    /// SPEC_06: Request object for projectile spawning.
    /// Contains all parameters needed to spawn and initialize a projectile.
    /// </summary>
    public class ProjectileSpawnRequest
    {
        public GameObject Prefab { get; set; }
        public Vector2 SourcePosition { get; set; }
        public Vector2 Direction { get; set; }
        public float SpawnOffset { get; set; } = 0.5f;

        public float Speed { get; set; }
        public float Range { get; set; }
        public int BaseDamage { get; set; }
        public DamageType DamageType { get; set; }
        public float KnockbackForce { get; set; }

        public CindarsHope.Combat.StatusEffect.StatusEffectSO StatusEffect { get; set; }
        public float StatusApplyChance { get; set; }

        /// <summary>
        /// fable_48 (aditivo): tags de material/elemento carregadas pelo projétil (ex.: flecha
        /// "Silver"/"Fire"). Propagadas ao <see cref="DamageRequest.WeaponMaterialTags"/> no impacto,
        /// onde o matching de vulnerabilidade da F06 concede bônus SÓ contra vulnerabilidade
        /// declarada. Default null/vazio => sem tags (melee/magia legados inalterados).
        /// </summary>
        public string[] AppliedTags { get; set; }

        /// <summary>Visual archetype used when Prefab is null (runtime-built projectile).</summary>
        public ProjectileVisualStyle VisualStyle { get; set; } = ProjectileVisualStyle.Auto;

        /// <summary>How many enemies the projectile can hit before despawning (1 = no pierce).</summary>
        public int MaxHits { get; set; } = 1;

        /// <summary>
        /// Fracao da velocidade inicial atingida no fim do alcance (game-feel de desaceleracao).
        /// 1 = velocidade constante (default, magias/legado); 0.3 = chega a 30% da velocidade no
        /// alcance maximo. A flecha usa &lt; 1 para comecar rapida e desacelerar conforme avanca.
        /// </summary>
        public float SpeedDecayToFraction { get; set; } = 1f;

        public ProjectileSpawnRequest()
        {
        }

        public ProjectileSpawnRequest(
            GameObject prefab,
            Vector2 sourcePosition,
            Vector2 direction,
            float speed,
            float range,
            int baseDamage,
            DamageType damageType,
            float knockbackForce,
            float spawnOffset = 0.5f,
            CindarsHope.Combat.StatusEffect.StatusEffectSO statusEffect = null,
            float statusApplyChance = 0f)
        {
            Prefab = prefab;
            SourcePosition = sourcePosition;
            Direction = direction;
            SpawnOffset = spawnOffset;
            Speed = speed;
            Range = range;
            BaseDamage = baseDamage;
            DamageType = damageType;
            KnockbackForce = knockbackForce;
            StatusEffect = statusEffect;
            StatusApplyChance = statusApplyChance;
        }
    }
}
