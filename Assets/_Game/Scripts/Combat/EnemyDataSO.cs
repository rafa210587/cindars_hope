using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "Enemy_", menuName = "CindarsHope/Combat/Enemy Data")]
    public class EnemyDataSO : ScriptableObject, IIdentifiedData
    {
        public string enemyId;
        public string DisplayName;
        [TextArea(1, 2)] public string LoreTagline;
        [TextArea] public string Description;
        public Sprite Icon;

        string IIdentifiedData.Id => enemyId;

        [Header("Identity")]
        public string FactionId;
        public EnemyRole PrimaryRole = EnemyRole.Chaser;
        public EnemyRole[] SecondaryRoles = new EnemyRole[0];
        public int CaveBand = 1;
        public string[] BiomeTags = new string[0];
        public string[] EnvironmentTags = new string[0];

        [Header("Health & Combat")]
        public int maxHp = 10;
        public int contactDamage = 1;
        public float contactDamageCooldownSeconds = 1f;
        public float contactKnockbackForce = 0f;
        public int defense = 0;
        public float receivedKnockbackResistance = 0f;
        public float receivedKnockbackMultiplier = 1f;

        [Header("Profile References (SPEC 13)")]
        public string SizeProfileId;
        public string MovementProfileId;
        public string ActionSetId;
        public string VulnerabilityProfileId;
        // fable_06 (aditivo): perfil de MATRIZ de vulnerabilidade por família (Element/Material/Status).
        // Separado de VulnerabilityProfileId (que carrega a JANELA temporal por papel/role). Vazio =>
        // sem matriz de família (neutro); o materializer cai no VulnerabilityProfileId como fallback.
        public string VulnerabilityMatrixProfileId;
        public string CombatResistanceProfileId;
        [Tooltip("Primary damage type hint for this enemy (e.g. 'physical', 'fire'). Full per-action typing lives in EnemyActionSO.")]
        public string PrimaryDamageTypeId;

        [Header("Movement Secondary (fable_24)")]
        [Tooltip("Optional secondary move. Floaters alternate primary FloatingSlow ↔ this (e.g. FloatingOrbit) in combat. " +
                 "Default GroundChase = no secondary behaviour (treated as 'unset'). Additive field — does not change existing assets.")]
        public EnemyMovementType MoveSecondary = EnemyMovementType.GroundChase;

        [Header("Movement (Legacy fallback)")]
        public float moveSpeed = 1.2f;
        public float detectionRadius = 5f;
        public float stopDistance = 0.55f;

        [Header("Feedback")]
        public Color hitFlashColor = Color.red;
        public float hitFlashDuration = 0.12f;

        [Header("Drops")]
        public string dropItemId = "item_wood";
        public int dropAmount = 1;
        public string lootTableId;

        [Header("Visual")]
        [Tooltip("Uniform visual scale multiplier applied to this enemy's sprite. 1 = default, 2.5 = boss-sized.")]
        public float VisualScale = 1f;

        [Header("Progression")]
        public int enemyLevel = 1;
        public EnemyDifficulty baseDifficulty = EnemyDifficulty.Easy;
        public int xpReward = 0;
        public int xpRewardOverride;
        public bool IsElite;
        public bool IsMiniBoss;
        public bool IsBoss;

        [Header("Bestiary")]
        public string BestiaryEntryId;

        // fable_33 (aditivo, save-safe): campos da ficha canônica do CAVE_BESTIARY_CATALOG.
        // Defaults preservam assets existentes (SpoilerTier=0, BestiarySize=Medium, Notes vazio).
        [Tooltip("Bestiary spoiler tier (CAVE_BESTIARY_CATALOG §2.5): commons 0-1, minibosses 2, " +
                 "gate bosses 3, the Four of level 101 = 4. Drives BESTIARY_KNOWLEDGE reveal gating (F21).")]
        [Range(0, 4)] public int SpoilerTier = 0;

        [Tooltip("Natural fantasy size class (D&D-style) from the catalog ficha. Drives the placeholder " +
                 "transform scale. Colliders still follow the SPEC 13 SizeProfileId, never this visual class.")]
        public BestiarySizeClass BestiarySize = BestiarySizeClass.Medium;

        [TextArea(1, 3)]
        [Tooltip("Dormant behaviour documented per fable_33: special ficha behaviours not covered by the " +
                 "22 canonical Moves are recorded here (INFO-level), never a new Move nor a parallel brain.")]
        public string Notes;

        private void OnValidate()
        {
            SpoilerTier = Mathf.Clamp(SpoilerTier, 0, 4);
            VisualScale = Mathf.Max(0.1f, VisualScale);
            maxHp = Mathf.Max(1, maxHp);
            contactDamage = Mathf.Max(0, contactDamage);
            contactDamageCooldownSeconds = Mathf.Max(0.01f, contactDamageCooldownSeconds);
            defense = Mathf.Max(0, defense);
            receivedKnockbackMultiplier = Mathf.Max(0f, receivedKnockbackMultiplier);
            moveSpeed = Mathf.Max(0f, moveSpeed);
            detectionRadius = Mathf.Max(0f, detectionRadius);
            stopDistance = Mathf.Max(0f, stopDistance);
            hitFlashDuration = Mathf.Max(0.01f, hitFlashDuration);
            dropAmount = Mathf.Max(0, dropAmount);
            enemyLevel = Mathf.Max(1, enemyLevel);
            xpReward = Mathf.Max(0, xpReward);
            xpRewardOverride = Mathf.Max(0, xpRewardOverride);
        }

        public int GetXPReward()
        {
            return xpRewardOverride > 0 ? xpRewardOverride : xpReward;
        }
    }

    public enum EnemyRole
    {
        Chaser,
        Guard,
        Ranged,
        Caster,
        Burrower,
        Swarm,
        Tank,
        Elite,
        MiniBoss,
        Boss
    }

    public enum EnemyDifficulty
    {
        VeryEasy,
        Easy,
        Normal,
        Hard,
        Elite,
        MiniBoss,
        Boss
    }

    /// <summary>
    /// fable_33 — natural fantasy size classes from CAVE_BESTIARY_CATALOG §2.1, used as the
    /// placeholder visual scale hint on the bestiary ficha. Additive enum (append only, save-safe):
    /// new values MUST be added at the end so serialized indices never shift.
    /// Tile footprints (catalog): Tiny 0.5×0.5, Small 0.75×0.75, Medium 1×1, Large 2×2,
    /// Huge 3×3, Gargantuan 4×4. Minibosses get ×1.5 visual, bosses ×2.5 (applied by the generator).
    /// </summary>
    public enum BestiarySizeClass
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        Gargantuan
    }
}