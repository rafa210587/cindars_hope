using CindarsHope.Core.Data;
using UnityEngine;

namespace CindarsHope.Combat
{
    [CreateAssetMenu(fileName = "EnemyMovement_", menuName = "CindarsHope/Combat/Enemy Movement Profile")]
    public class EnemyMovementProfileSO : ScriptableObject, IIdentifiedData
    {
        public string MovementProfileId;
        public EnemyMovementType MovementType;

        [Header("Speed & Acceleration")]
        public float MoveSpeed = 2f;
        public float Acceleration = 0f;

        [Header("Detection & Pursuit")]
        public float DetectionRange = 10f;
        public float LeashRange = 30f;
        public float AttackRange = 1.5f;
        public float PreferredDistance = 1f;
        public float WanderRadius = 5f;

        [Header("Special Movement")]
        public bool CanBurrow;
        public bool CanPhaseShortBlink;
        public bool CanLeap;
        public bool CanFly;

        [Header("Timing")]
        public float DecisionTickSeconds = 0.3f;

        // ── fable_82: Evasao Reativa (campos aditivos, default OFF para roles que nao evadem) ──

        [Header("Reactive Evasion (fable_82)")]
        /// <summary>Habilita sidestep reativo ao windup do player. Default false (tanks/brutes/swarm/guard nao evadem).</summary>
        public bool CanReactiveEvade = false;
        /// <summary>Probabilidade (0-1) de executar o sidestep quando condicoes forem atendidas.</summary>
        [Range(0f, 1f)] public float EvadeChance = 0.5f;
        /// <summary>Cooldown minimo entre esquivas reativas (segundos).</summary>
        public float EvadeCooldown = 2.5f;
        /// <summary>Raio maximo do player para ativar a esquiva reativa (tiles).</summary>
        public float EvadeReactionRadius = 4f;
        /// <summary>Distancia do sidestep lateral (tiles).</summary>
        public float EvadeSidestepDistance = 1.6f;
        /// <summary>Duracao do sidestep hop (segundos).</summary>
        public float EvadeSidestepDuration = 0.18f;

        [Header("Reposition Dash (fable_82)")]
        /// <summary>Habilita dash de reposicionamento. So para ranged/caster/assassino/duelista (gate por role).</summary>
        public bool RepositionDashEnabled = false;
        /// <summary>Cooldown entre repositions dash (segundos).</summary>
        public float DashCooldown = 3f;
        /// <summary>Distancia do dash de reposicionamento (tiles).</summary>
        public float DashDistance = 2.5f;
        /// <summary>Duracao do dash de reposicionamento (segundos).</summary>
        public float DashDuration = 0.22f;

        [Header("Gap Closer Leap (fable_82)")]
        /// <summary>Habilita uso do Leap existente como gap-closer agressivo. Default false.</summary>
        public bool GapCloserLeap = false;

        string IIdentifiedData.Id => MovementProfileId;

        private void OnValidate()
        {
            MoveSpeed = Mathf.Max(0f, MoveSpeed);
            Acceleration = Mathf.Max(0f, Acceleration);
            DetectionRange = Mathf.Max(0.5f, DetectionRange);
            LeashRange = Mathf.Max(DetectionRange, LeashRange);
            AttackRange = Mathf.Max(0.5f, AttackRange);
            PreferredDistance = Mathf.Max(0f, PreferredDistance);
            WanderRadius = Mathf.Max(0f, WanderRadius);
            DecisionTickSeconds = Mathf.Max(0.1f, DecisionTickSeconds);

            if (string.IsNullOrWhiteSpace(MovementProfileId))
                MovementProfileId = "movement_" + name.ToLower();

            // fable_82: clampar campos de evasao
            EvadeChance = Mathf.Clamp01(EvadeChance);
            EvadeCooldown = Mathf.Max(0.5f, EvadeCooldown);
            EvadeReactionRadius = Mathf.Max(0.5f, EvadeReactionRadius);
            EvadeSidestepDistance = Mathf.Max(0.2f, EvadeSidestepDistance);
            EvadeSidestepDuration = Mathf.Max(0.05f, EvadeSidestepDuration);
            DashCooldown = Mathf.Max(0.5f, DashCooldown);
            DashDistance = Mathf.Max(0.5f, DashDistance);
            DashDuration = Mathf.Max(0.05f, DashDuration);
        }
    }

    public enum EnemyMovementType
    {
        // ── The 10 original moves (intact; do not reorder — EnemyDataSO assets serialize by index) ──
        GroundChase,
        GroundPatrol,
        GuardStationary,
        KiteRanged,
        CasterKeepAway,
        BurrowAmbush,
        SwarmErratic,
        TankSlowPush,
        PhaseShortBlink,
        Leaper,

        // ── fable_24: the 12 canonical moves added at the END (additive, save-safe) ──────────────
        // Pack coordination (builds on fable_04 EnemyPackCoordinator).
        PackFlanker,        // only engages while a living PackLeader is in range; else GroundChase after timeout
        PackLeader,         // leads the pack; its death sends flankers into RetreatAndCall
        RetreatAndCall,     // flees and emits EnemyCallForHelpEvent so allies in radius regroup
        // Floating movers (ignore ground obstacles; alternate Slow↔Orbit via EnemyDataSO.MoveSecondary).
        FloatingSlow,       // hovers slowly toward the player, ignoring floor hazards
        FloatingOrbit,      // orbits the player at firing distance
        // Ranged kiting / charge.
        CircleStrafe,       // strafes around the player at preferred firing distance
        ChargeLine,         // telegraphs a straight line, then charges in a straight investida
        // Idle / anchored.
        TreasureIdleAmbush, // mimic: stays disguised and immobile until the player is < 2 tiles
        HazardLure,         // retreats trying to pull the player across a hazard tile
        ProtectAnchor,      // never strays more than N tiles from its anchor (spawn point / object)
        // Boss primitives (fable_05 orchestrates full phases; here only the 2 primitives).
        BossArenaControl,   // arena leash: cannot leave the arena bounds
        BossPhaseShift      // swaps ActionSet/Move on an external trigger (fable_05 drives it)
    }
}
