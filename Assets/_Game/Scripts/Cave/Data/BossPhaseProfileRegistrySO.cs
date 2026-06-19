using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    /// <summary>
    /// fable_05 — registry of <see cref="BossPhaseProfileSO"/> keyed by ProfileId
    /// (<c>boss_phase_&lt;bossId&gt;</c>). The CaveBossSpawner looks up the profile for the boss's
    /// EnemyId; bosses without a profile keep the simple (pre-fable_05) behaviour (anti-regression).
    /// Populated by the AttachDefaultBossPhaseProfiles editor generator.
    /// </summary>
    [CreateAssetMenu(fileName = "BossPhaseProfileRegistry", menuName = "CindarsHope/Cave/Boss Phase Profile Registry")]
    public sealed class BossPhaseProfileRegistrySO : DataRegistrySO<BossPhaseProfileSO>
    {
        /// <summary>Convention helper: profile id for a given boss enemy id.</summary>
        public static string BuildProfileId(string bossEnemyId)
        {
            return string.IsNullOrWhiteSpace(bossEnemyId) ? string.Empty : "boss_phase_" + bossEnemyId;
        }

        /// <summary>Find the phase profile authored for a boss enemy id, or null when none exists.</summary>
        public BossPhaseProfileSO GetProfileForBoss(string bossEnemyId)
        {
            if (string.IsNullOrWhiteSpace(bossEnemyId))
            {
                return null;
            }

            // Prefer the id convention; fall back to a linear scan on BossEnemyId for hand-authored assets.
            if (TryGetById(BuildProfileId(bossEnemyId), out var byId) && byId != null)
            {
                return byId;
            }

            foreach (var profile in All)
            {
                if (profile != null && profile.BossEnemyId == bossEnemyId)
                {
                    return profile;
                }
            }

            return null;
        }
    }
}
