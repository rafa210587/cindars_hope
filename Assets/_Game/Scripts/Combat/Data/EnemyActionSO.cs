using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemyAction_", menuName = "CindarsHope/Combat/Enemy Action")]
    public class EnemyActionSO : ScriptableObject, IIdentifiedData
    {
        public string ActionId;
        public string DisplayName;
        public EnemyActionType ActionType;

        [Header("Damage")]
        public string DamageType = "physical";
        public int BaseDamage = 5;

        [Header("Range & Area")]
        public float Range = 1.5f;
        public float AreaRadius = 0f;

        [Header("Timing")]
        public float CooldownSeconds = 2f;
        public float WindupSeconds = 0.5f;
        public float RecoverSeconds = 0.5f;

        [Header("Projectile")]
        public float ProjectileSpeed = 0f;

        [Header("Status")]
        public string[] StatusApplicationIds = new string[0];
        [Range(0f, 1f)] public float StatusApplyChance = 1.0f;

        [Header("Telegraph")]
        public string TelegraphProfileId;

        [Header("Vulnerability")]
        public bool TriggersVulnerabilityWindow;
        public VulnerabilityTriggerMode VulnerabilityWindowTrigger = VulnerabilityTriggerMode.AfterAttackRecover;

        [Header("Targeting")]
        public float MinRange = 0f;
        public int MaxTargets = 1;
        public bool RequiresLineOfSight = false;
        public bool IsInterruptible = true;

        string IIdentifiedData.Id => ActionId;

        private void OnValidate()
        {
            BaseDamage = Mathf.Max(0, BaseDamage);
            Range = Mathf.Max(0f, Range);
            MinRange = Mathf.Clamp(MinRange, 0f, Range);
            AreaRadius = Mathf.Max(0f, AreaRadius);
            CooldownSeconds = Mathf.Max(0.1f, CooldownSeconds);
            WindupSeconds = Mathf.Max(0f, WindupSeconds);
            RecoverSeconds = Mathf.Max(0f, RecoverSeconds);
            ProjectileSpeed = Mathf.Max(0f, ProjectileSpeed);

            if (string.IsNullOrWhiteSpace(ActionId))
                ActionId = "action_" + name.ToLower();
        }
    }

    public enum EnemyActionType
    {
        MeleeAttack,
        RangedProjectile,
        CastProjectile,
        AreaPulse,
        SelfBuff,
        BurrowStrike,
        LeapStrike
    }
}
