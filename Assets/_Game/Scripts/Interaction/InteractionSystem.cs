using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private Collider2D _interactionTrigger;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        private readonly List<InteractionCandidate> _candidates = new List<InteractionCandidate>();
        private bool _missingTriggerWarningLogged;

        private readonly struct InteractionCandidate
        {
            public readonly IInteractable Interactable;
            public readonly Collider2D Collider;

            public InteractionCandidate(IInteractable interactable, Collider2D collider)
            {
                Interactable = interactable;
                Collider = collider;
            }
        }

        private void Awake()
        {
            EnsureInteractionTrigger();
        }

        private void Reset()
        {
            _interactionTrigger = GetComponent<Collider2D>();
        }

        private void OnValidate()
        {
            if (_interactionTrigger == null)
            {
                _interactionTrigger = GetComponent<Collider2D>();
            }
        }

        private void Update()
        {
            if (!Input.GetKeyDown(_interactKey))
            {
                return;
            }

            var interactable = GetBestCandidate();
            if (interactable == null)
            {
                return;
            }

            interactable.Interact(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = GetInteractable(other);
            if (interactable == null || ContainsCandidate(interactable))
            {
                return;
            }

            _candidates.Add(new InteractionCandidate(interactable, other));
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = GetInteractable(other);
            if (interactable == null && other == null)
            {
                return;
            }

            RemoveCandidate(interactable, other);
        }

        private IInteractable GetBestCandidate()
        {
            CleanInvalidCandidates();

            var origin = _interactionTrigger != null
                ? (Vector2)_interactionTrigger.bounds.center
                : (Vector2)transform.position;

            IInteractable bestInteractable = null;
            var bestSqrDistance = float.PositiveInfinity;

            for (var i = 0; i < _candidates.Count; i++)
            {
                var candidate = _candidates[i];
                var interactable = candidate.Interactable;
                if (interactable == null || !interactable.CanInteract(gameObject))
                {
                    continue;
                }

                var targetPosition = GetCandidatePosition(candidate, origin);
                var sqrDistance = (targetPosition - origin).sqrMagnitude;

                if (sqrDistance < bestSqrDistance)
                {
                    bestInteractable = interactable;
                    bestSqrDistance = sqrDistance;
                }
            }

            return bestInteractable;
        }

        private static IInteractable GetInteractable(Collider2D source)
        {
            if (source == null)
            {
                return null;
            }

            var interactable = source.GetComponent<IInteractable>();
            return interactable ?? source.GetComponentInParent<IInteractable>();
        }

        private static Vector2 GetCandidatePosition(InteractionCandidate candidate, Vector2 origin)
        {
            if (candidate.Collider != null)
            {
                return candidate.Collider.ClosestPoint(origin);
            }

            if (candidate.Interactable is Component component)
            {
                return component.transform.position;
            }

            return origin;
        }

        private bool ContainsCandidate(IInteractable interactable)
        {
            for (var i = 0; i < _candidates.Count; i++)
            {
                if (_candidates[i].Interactable == interactable)
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveCandidate(IInteractable interactable, Collider2D source)
        {
            for (var i = _candidates.Count - 1; i >= 0; i--)
            {
                var candidate = _candidates[i];
                if (candidate.Interactable == interactable || candidate.Collider == source)
                {
                    _candidates.RemoveAt(i);
                }
            }
        }

        private void CleanInvalidCandidates()
        {
            for (var i = _candidates.Count - 1; i >= 0; i--)
            {
                var candidate = _candidates[i];
                if (candidate.Interactable == null || candidate.Collider == null)
                {
                    _candidates.RemoveAt(i);
                }
            }
        }

        private void EnsureInteractionTrigger()
        {
            if (_interactionTrigger == null)
            {
                _interactionTrigger = GetComponent<Collider2D>();
            }

            if (_interactionTrigger != null)
            {
                return;
            }

            if (!_missingTriggerWarningLogged)
            {
                Debug.LogWarning($"{nameof(InteractionSystem)} on '{name}' has no interaction trigger assigned.");
                _missingTriggerWarningLogged = true;
            }
        }
    }
}
