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
