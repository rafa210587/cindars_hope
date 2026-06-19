using System;
using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    /// <summary>
    /// fable_05 — data-driven boss phase profile consumed by <c>BossBrainController</c>. Each profile
    /// is keyed by the boss enemy id (<c>boss_phase_&lt;bossId&gt;</c>) and lists the phases ordered by
    /// HP threshold descending (100% first). The controller resolves the current phase from the live
    /// HP fraction and swaps the brain's ActionSet/Move + applies multipliers on each transition.
    ///
    /// This SO does NOT decide phase transitions itself (the controller does, from EnemyHealth);
    /// it is pure authored data. Reused by WAVE 19 for the final boss (level 100/101).
    /// Boss/phase state is derived from current HP (ADR-0005 / cave_rules.md): a revisited level with a
    /// boss left mid-fight resolves to the same phase for the same HP — HP persistence itself is F13.
    /// </summary>
    [CreateAssetMenu(fileName = "BossPhase_", menuName = "CindarsHope/Cave/Boss Phase Profile")]
    public sealed class BossPhaseProfileSO : ScriptableObject, IIdentifiedData
    {
        [Tooltip("Stable id: boss_phase_<bossId>. Used as the registry / lookup key.")]
        public string ProfileId;

        [Tooltip("EnemyId of the boss this profile drives (matches EnemyDataSO.enemyId).")]
        public string BossEnemyId;

        [Tooltip("Phases ordered by HpThresholdPercent DESC (the 100% phase is index 0). " +
                 "The first phase whose threshold is >= current HP% is the active phase.")]
        public BossPhase[] Phases = Array.Empty<BossPhase>();

        [Multiline(2)] public string Notes;

        string IIdentifiedData.Id => ProfileId;

        public bool HasPhases => Phases != null && Phases.Length > 0;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(ProfileId))
            {
                ProfileId = string.IsNullOrWhiteSpace(BossEnemyId)
                    ? "boss_phase_" + name.ToLowerInvariant()
                    : "boss_phase_" + BossEnemyId;
            }

            if (Phases == null)
            {
                return;
            }

            // Keep thresholds clamped to [0,100]; the controller assumes descending order but is
            // defensive (BossPhaseLogic sorts a copy) so authoring mistakes degrade gracefully.
            for (int i = 0; i < Phases.Length; i++)
            {
                if (Phases[i] == null)
                {
                    continue;
                }

                Phases[i].HpThresholdPercent = Mathf.Clamp(Phases[i].HpThresholdPercent, 0f, 100f);
                Phases[i].MoveSpeedMultiplier = Mathf.Max(0f, Phases[i].MoveSpeedMultiplier);
                Phases[i].DamageMultiplier = Mathf.Max(0f, Phases[i].DamageMultiplier);
                Phases[i].AddsCount = Mathf.Max(0, Phases[i].AddsCount);
                Phases[i].VulnerabilityWindowSeconds = Mathf.Max(0f, Phases[i].VulnerabilityWindowSeconds);
            }
        }
    }

    /// <summary>
    /// A single boss phase. <see cref="HpThresholdPercent"/> is the UPPER HP bound of this phase
    /// expressed as a percent of MaxHp: the phase is active while current HP% is at or below this
    /// threshold and above the next (lower) phase's threshold. Phases are ordered descending so the
    /// 100% phase comes first.
    /// </summary>
    [Serializable]
    public sealed class BossPhase
    {
        [Range(0f, 100f)]
        [Tooltip("Upper HP% bound for this phase (e.g. 100, 66, 33). Active while HP% <= this and > the next phase's threshold.")]
        public float HpThresholdPercent = 100f;

        [Tooltip("ActionSetId swapped into the brain when this phase becomes active (EnemyActionSetSO id).")]
        public string ActionSetId;

        [Tooltip("Optional MovementProfileId for this phase. Empty = keep current movement. " +
                 "Used for the EMENDA flight phase (e.g. a FloatingOrbit profile for Cindershard Wyrm / Draconic Elder).")]
        public string MovementProfileId;

        [Tooltip("Move speed multiplier applied while in this phase (1 = unchanged).")]
        public float MoveSpeedMultiplier = 1f;

        [Tooltip("Damage multiplier applied while in this phase (1 = unchanged). Telemetry/event only in this spec; balance is F06.")]
        public float DamageMultiplier = 1f;

        [Tooltip("EnemyId of the adds summoned ONCE when this phase is entered. Empty = no adds.")]
        public string AddsEnemyId;

        [Tooltip("How many adds to summon once on phase entry (0 = none).")]
        public int AddsCount;

        [Tooltip("Vulnerability window (seconds) opened on the transition INTO this phase (0 = none).")]
        public float VulnerabilityWindowSeconds = 1.5f;
    }
}
