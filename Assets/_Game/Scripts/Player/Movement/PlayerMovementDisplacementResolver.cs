using System;
using System.Collections;
using UnityEngine;

namespace CindarsHope.Player.Movement
{
    [DisallowMultipleComponent]
    public sealed class PlayerMovementDisplacementResolver : MonoBehaviour
    {
        public const string CollisionDebtNoPlayerCollider = "COLLISION_DETECTION_DEBT_NO_PLAYER_COLLIDER";

        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private PlayerController _playerController;

        private Coroutine _displacementRoutine;

        public bool IsDisplacing => _displacementRoutine != null;
        public bool LastMoveHadNoColliderDebt { get; private set; }

        private void Awake()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (_playerController == null) _playerController = GetComponent<PlayerController>();
        }

        public bool TryDisplace(Vector2 direction, float distance, float duration, Action onComplete)
        {
            if (IsDisplacing || direction.sqrMagnitude < 0.001f || distance <= 0f || duration <= 0f)
            {
                return false;
            }

            _displacementRoutine = StartCoroutine(DisplaceRoutine(direction.normalized, distance, duration, onComplete));
            return true;
        }

        private IEnumerator DisplaceRoutine(Vector2 direction, float distance, float duration, Action onComplete)
        {
            var origin = _rigidbody != null ? _rigidbody.position : (Vector2)transform.position;
            var target = ResolveTarget(origin, direction, distance);
            var previousSpeedMultiplier = _playerController != null ? _playerController.SpeedMultiplier : 1f;

            if (_playerController != null) _playerController.SpeedMultiplier = 0f;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                MoveTo(Vector2.Lerp(origin, target, t));
                yield return null;
            }

            MoveTo(target);

            if (_playerController != null) _playerController.SpeedMultiplier = previousSpeedMultiplier;

            _displacementRoutine = null;
            onComplete?.Invoke();
        }

        private Vector2 ResolveTarget(Vector2 origin, Vector2 direction, float distance)
        {
            LastMoveHadNoColliderDebt = _collider == null;
            if (_collider == null)
            {
                return origin + direction * distance;
            }

            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                useTriggers = false,
                layerMask = Physics2D.DefaultRaycastLayers
            };
            var hits = new RaycastHit2D[12];
            var hitCount = _collider.Cast(direction, filter, hits, distance);
            var nearestDistance = distance;
            for (var i = 0; i < hitCount; i++)
            {
                if (hits[i].collider == null || hits[i].collider == _collider)
                {
                    continue;
                }

                nearestDistance = Mathf.Min(nearestDistance, hits[i].distance);
            }

            var safeDistance = Mathf.Max(0f, nearestDistance - 0.05f);
            return origin + direction * safeDistance;
        }

        private void MoveTo(Vector2 position)
        {
            if (_rigidbody != null) _rigidbody.MovePosition(position);
            else transform.position = position;
        }
    }
}
