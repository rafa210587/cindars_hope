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

        [Header("Rise-Once (spec_enemy_attack_kits_v1 — primitiva P2)")]
        // Habilita o comportamento de reerguer 1x apos "morrer" (cracked_bone, undead_shambler, etc.).
        public bool RiseOnceEnabled = false;
        // Fracao de HP maximo restaurada ao reerguer (0-1).
        [Range(0f, 1f)] public float RiseOnceHpPercent = 0.25f;
        // DamageType (strings, case-insensitive) que bloqueiam o reerguimento (ex.: {"fire","radiant"}).
        public string[] RiseOnceBlockedByDamageTypes = new string[0];
        // Duracao do colapso antes de reerguer (janela de punicao para o player destruir os ossos).
        public float RiseOnceCollapseSeconds = 2f;

        [Header("Ally Heal/Buff (spec_enemy_attack_kits_v1 — primitiva P2)")]
        // Raio de busca por alvo-aliado (mais ferido, senao mais proximo).
        public float AllyTargetRadius = 4f;
        // Fracao de HP curada no aliado-alvo (0 = nao cura, so buffa).
        [Range(0f, 1f)] public float AllyHealPercent = 0f;
        // StatusId de buff aplicado ao aliado-alvo (reusa StatusEffect database; vazio = nenhum).
        public string AllyBuffStatusId = string.Empty;

        [Header("Hazard Zone (spec_enemy_attack_kits_v1 — primitiva P2)")]
        // Raio da zona de hazard deixada no chao.
        public float HazardRadius = 1.5f;
        // Duracao total da zona antes de expirar.
        public float HazardDurationSeconds = 4f;
        // Intervalo entre ticks de dano/status dentro da zona.
        public float HazardTickSeconds = 1f;
        // Dano aplicado por tick a quem estiver dentro da zona.
        public int HazardDamagePerTick = 0;
        // StatusId aplicado por tick (reusa StatusEffect database; vazio = nenhum).
        public string HazardStatusId = string.Empty;
        // Quando true, a TelegraphedAoE desta acao deixa a zona persistir apos o dano inicial.
        public bool LeavesHazard = false;

        [Header("Pull (spec_enemy_attack_kits_v1 — primitiva P2)")]
        // Distancia (em tiles) que o player e deslocado na direcao configurada.
        public float PullDistanceTiles = 1.5f;
        // True = puxa na direcao do atacante (agarrao); false = puxa na direcao do hazard/origem configurada pelo consumidor.
        public bool PullFromAttackerOrigin = true;

        [Header("Salvo Multiplo (spec_enemy_attack_kits_v1 — follow-up salvas)")]
        // Quantidade de projeteis disparados em leque por uso (1 = comportamento atual, sem leque).
        public int ProjectileCount = 1;
        // Angulo total (graus) do leque, distribuido simetricamente em torno da direcao base.
        public float ProjectileSpreadAngleDegrees = 0f;

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

            RiseOnceHpPercent = Mathf.Clamp01(RiseOnceHpPercent);
            RiseOnceCollapseSeconds = Mathf.Max(0f, RiseOnceCollapseSeconds);
            AllyTargetRadius = Mathf.Max(0f, AllyTargetRadius);
            AllyHealPercent = Mathf.Clamp01(AllyHealPercent);
            HazardRadius = Mathf.Max(0f, HazardRadius);
            HazardDurationSeconds = Mathf.Max(0f, HazardDurationSeconds);
            HazardTickSeconds = Mathf.Max(0.01f, HazardTickSeconds);
            HazardDamagePerTick = Mathf.Max(0, HazardDamagePerTick);
            PullDistanceTiles = Mathf.Max(0f, PullDistanceTiles);

            ProjectileCount = Mathf.Clamp(ProjectileCount, 1, 5);
            ProjectileSpreadAngleDegrees = Mathf.Clamp(ProjectileSpreadAngleDegrees, 0f, 90f);

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
