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
        private bool _isDisplacing;

        public bool IsDisplacing => _isDisplacing;
        public bool LastMoveHadNoColliderDebt { get; private set; }

        private void Awake()
        {
            if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody2D>();
            if (_collider == null) _collider = GetComponent<Collider2D>();
            if (_playerController == null) _playerController = GetComponent<PlayerController>();
        }

        public bool TryDisplace(Vector2 direction, float distance, float duration, Action onComplete)
            => TryDisplace(direction, distance, duration, null, onComplete);

        public bool TryDisplace(Vector2 direction, float distance, float duration,
            Func<Collider2D, bool> shouldIgnoreCollider, Action onComplete)
        {
            if (!isActiveAndEnabled || IsDisplacing || (_playerController != null && _playerController.IsBeingDisplaced)
                || direction.sqrMagnitude < 0.001f || distance <= 0f || duration <= 0f)
                return false;
            _isDisplacing = true;
            var routine = StartCoroutine(DisplaceRoutine(
                direction.normalized, distance, duration, shouldIgnoreCollider, onComplete));
            if (_isDisplacing) _displacementRoutine = routine;
            return true;
        }

        private void OnDisable()
        {
            if (!_isDisplacing) return;
            if (_displacementRoutine != null) StopCoroutine(_displacementRoutine);
            ReleaseDisplacement();
        }

        private void ReleaseDisplacement()
        {
            if (_playerController != null && _isDisplacing)
            {
                _playerController.SpeedComposer.ClearFactor(SpeedFactorKind.Displacement);
                _playerController.IsBeingDisplaced = false;
            }
            _isDisplacing = false;
            _displacementRoutine = null;
        }

        private IEnumerator DisplaceRoutine(Vector2 direction, float distance, float duration,
            Func<Collider2D, bool> shouldIgnoreCollider, Action onComplete)
        {
            var origin = _rigidbody != null ? _rigidbody.position : (Vector2)transform.position;
            var target = ResolveTarget(origin, direction, distance, shouldIgnoreCollider);

            if ((target - origin).sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning($"[PlayerMovementDisplacementResolver] Displacement blocked at origin — target equals origin. direction={direction}", gameObject);
                ReleaseDisplacement();
                onComplete?.Invoke();
                yield break;
            }

            // fable_47: fator nomeado em vez de snapshot/restore do mutável compartilhado.
            if (_playerController != null)
            {
                _playerController.SpeedComposer.SetFactor(SpeedFactorKind.Displacement, 0f);
                _playerController.IsBeingDisplaced = true;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Sync with physics: MovePosition called after WaitForFixedUpdate so it is
                // the last call before the physics step — PlayerController.FixedUpdate is
                // already skipped via IsBeingDisplaced, but this ordering is belt-and-suspenders.
                yield return new WaitForFixedUpdate();
                elapsed += Time.fixedDeltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                MoveTo(Vector2.Lerp(origin, target, t));
            }

            MoveTo(target);
            // Let the physics step commit the final MovePosition before impact queries.
            yield return new WaitForFixedUpdate();
            ReleaseDisplacement();
            onComplete?.Invoke();
        }

        private Vector2 ResolveTarget(Vector2 origin, Vector2 direction, float distance,
            Func<Collider2D, bool> shouldIgnoreCollider)
        {
            LastMoveHadNoColliderDebt = _collider == null;
            if (_collider == null)
                return origin + direction * distance;

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
                var hit = hits[i];
                if (hit.collider == null) continue;
                if (ShouldIgnoreHit(hit)
                    || (shouldIgnoreCollider != null && shouldIgnoreCollider(hit.collider))) continue;

                if (hit.distance < nearestDistance)
                    nearestDistance = hit.distance;
            }

            var safeDistance = Mathf.Max(0f, nearestDistance - 0.05f);
            return origin + direction * safeDistance;
        }

        // Ignore self, own children, triggers, and anything sharing the same Rigidbody2D.
        private bool ShouldIgnoreHit(RaycastHit2D hit)
        {
            if (hit.collider == _collider) return true;
            if (hit.collider.isTrigger) return true;
            if (_rigidbody != null && hit.collider.attachedRigidbody == _rigidbody) return true;
            if (hit.collider.transform.IsChildOf(transform)) return true;
            if (transform.IsChildOf(hit.collider.transform)) return true;
            return false;
        }

        private void MoveTo(Vector2 position)
        {
            if (_rigidbody != null) _rigidbody.MovePosition(position);
            else transform.position = position;
        }
    }
}
