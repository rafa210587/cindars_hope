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

        [Header("Blink / Death-Trigger (SPEC 13D)")]
        public float BlinkRange = 1.5f;
        public bool IsDeathtrigger = false;

        [Header("Signature Attacks (fable_83)")]
        // ComboStrike: numero de hits encadeados (>= 1)
        public int ComboHits = 3;
        // TelegraphedAoE: atraso em segundos entre marcar a zona e resolver o dano
        public float AoeDelay = 0.8f;
        // TelegraphedAoE: raio da zona de dano
        public float AoeRadius = 2.5f;
        // SummonAdds: quantidade de adds invocados por uso
        public int SummonCount = 2;
        // SummonAdds: ID da criatura a ser invocada (referenciado no EnemyDatabase)
        public string SummonEnemyId = string.Empty;
        // DebuffStrike: ID do status a aplicar no hit (reusa StatusEffect database)
        public string DebuffStatusId = string.Empty;

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
        LeapStrike,
        BlinkStrike,
        // fable_83 — ataques-assinatura por arquetipo (aditivos no fim; save-safe)
        ComboStrike,
        TelegraphedAoE,
        SummonAdds,
        MultiHitCharge,
        DebuffStrike
    }
}
