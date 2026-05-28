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
        public string CombatResistanceProfileId;
        [Tooltip("Primary damage type hint for this enemy (e.g. 'physical', 'fire'). Full per-action typing lives in EnemyActionSO.")]
        public string PrimaryDamageTypeId;

        [Header("Movement (Legacy fallback)")]
        public float moveSpeed = 1.2f;
        public float detectionRadius = 5f;
        public float stopDistance = 0.55f;

        [Header("Behavior")]
        public string aiBehaviorId;

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

        private void OnValidate()
        {
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
}