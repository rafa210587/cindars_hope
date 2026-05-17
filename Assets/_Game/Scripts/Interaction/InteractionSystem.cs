using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private Collider2D _interactionTrigger;
        [SerializeField] private KeyCode _interactKey = KeyCode.E;

        private readonly List<IInteractable> _nearbyInteractables = new List<IInteractable>();
        private bool _missingTriggerWarningLogged;

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
            if (interactable == null || _nearbyInteractables.Contains(interactable))
            {
                return;
            }

            _nearbyInteractables.Add(interactable);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = GetInteractable(other);
            if (interactable == null)
            {
                return;
            }

            _nearbyInteractables.Remove(interactable);
        }

        private IInteractable GetBestCandidate()
        {
            for (var i = _nearbyInteractables.Count - 1; i >= 0; i--)
            {
                if (_nearbyInteractables[i] == null)
                {
                    _nearbyInteractables.RemoveAt(i);
                }
            }

            for (var i = 0; i < _nearbyInteractables.Count; i++)
            {
                var interactable = _nearbyInteractables[i];

                if (interactable.CanInteract(gameObject))
                {
                    return interactable;
                }
            }

            return null;
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
