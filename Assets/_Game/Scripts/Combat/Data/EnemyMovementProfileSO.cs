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
        GroundChase,
        GroundPatrol,
        GuardStationary,
        KiteRanged,
        CasterKeepAway,
        BurrowAmbush,
        SwarmErratic,
        TankSlowPush,
        PhaseShortBlink,
        Leaper
    }
}
