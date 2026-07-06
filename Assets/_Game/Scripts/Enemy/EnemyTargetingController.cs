using CindarsHope.Core.Bootstrap;
using UnityEngine;

namespace CindarsHope.Enemy
{
    /// <summary>Owns target refresh and spatial queries while EnemyBrain coordinates decisions.</summary>
    internal sealed class EnemyTargetingController
    {
        public GameObject CurrentTarget { get; private set; }

        public void RefreshPlayerTarget()
        {
            var visiblePlayer = Player.PlayerController.ActiveInstance;
            if (visiblePlayer != null)
            {
                CurrentTarget = visiblePlayer.gameObject;
            }
            else if (CurrentTarget == null)
            {
                CurrentTarget = GameBootstrap.Instance?.PlayerManager?.gameObject;
            }
        }

        public void RefreshConflictTarget(EnemyConflictHandler conflict)
        {
            var visiblePlayer = Player.PlayerController.ActiveInstance;
            var playerGo = visiblePlayer != null ? visiblePlayer.gameObject : CurrentTarget;
            var newTarget = conflict.RefreshConflictTarget(playerGo, CurrentTarget, out _);
            if (newTarget != null)
                CurrentTarget = newTarget;
        }

        public float DistanceFrom(Transform origin)
        {
            return CurrentTarget != null
                ? Vector2.Distance(origin.position, CurrentTarget.transform.position)
                : float.MaxValue;
        }

        public Vector2 DirectionFrom(Transform origin)
        {
            return CurrentTarget != null
                ? ((Vector2)(CurrentTarget.transform.position - origin.position)).normalized
                : Vector2.zero;
        }
    }
}
