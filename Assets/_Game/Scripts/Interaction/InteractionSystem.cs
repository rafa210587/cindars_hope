using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Farm;
using UnityEngine;

namespace CindarsHope.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private Collider2D _interactionTrigger;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        [SerializeField] private float _maxInteractionDistance = 0.45f;
        private readonly List<InteractionCandidate> _candidates = new List<InteractionCandidate>();
        private bool _missingTriggerWarningLogged;
        private bool _lastPublishedHasCandidate;
        private string _lastPublishedPrompt = string.Empty;

        public string CurrentPrompt
        {
            get
            {
                var candidate = GetBestCandidate();
                
                return candidate != null ? candidate.InteractionPrompt : string.Empty;
            }
        }

        public bool HasCandidate => GetBestCandidate() != null;

        // WAVE_INTEGRATION_11: Exposes the current best interactable for skill effect targeting.
        // Returns null if no valid candidate is in range.
        public IInteractable GetCurrentInteractable() => GetBestCandidate();

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

        private void OnEnable()
        {
            PublishPrompt(false, string.Empty, true);
        }

        private void OnDisable()
        {
            PublishPrompt(false, string.Empty, true);
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
            if (FarmPlot.IsAnyActionMenuOpen)
            {
                PublishPromptIfChanged();
                return;
            }

            var modalManager = GameBootstrap.Instance?.ModalManager;
            if (modalManager != null && modalManager.HasActiveModal)
            {
                PublishPromptIfChanged();
                return;
            }

            PublishPromptIfChanged();

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
            PublishPromptIfChanged();
        }

        public void RegisterCandidate(Collider2D other)
        {
            var interactable = GetInteractable(other);
            if (interactable == null || ContainsCandidate(interactable))
            {
                return;
            }

            _candidates.Add(new InteractionCandidate(interactable, other));
            PublishPromptIfChanged();
        }

        public void UnregisterCandidate(Collider2D other)
        {
            var interactable = GetInteractable(other);
            if (interactable == null && other == null)
            {
                return;
            }

            RemoveCandidate(interactable, other);
            PublishPromptIfChanged();
        }
        private void PublishPromptIfChanged()
        {
            var candidate = GetBestCandidate();
            var hasCandidate = candidate != null;
            var prompt = hasCandidate ? candidate.InteractionPrompt : string.Empty;
            PublishPrompt(hasCandidate, prompt, false);
        }

        private void PublishPrompt(bool hasCandidate, string prompt, bool force)
        {
            if (prompt == null)
            {
                prompt = string.Empty;
            }

            if (!force && _lastPublishedHasCandidate == hasCandidate && _lastPublishedPrompt == prompt)
            {
                return;
            }

            _lastPublishedHasCandidate = hasCandidate;
            _lastPublishedPrompt = prompt;
            GameEventBus.Publish(new InteractionPromptChangedEvent(hasCandidate, prompt));
        }

        private IInteractable GetBestCandidate()
        {
            CleanInvalidCandidates();

            var origin = _interactionTrigger != null
                ? (Vector2)_interactionTrigger.bounds.center
                : (Vector2)transform.position;

            IInteractable bestInteractable = null;
            var bestSqrDistance = float.PositiveInfinity;
            var maxSqrDistance = _maxInteractionDistance * _maxInteractionDistance;

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

                if (sqrDistance > maxSqrDistance)
                {
                    continue;
                }

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
