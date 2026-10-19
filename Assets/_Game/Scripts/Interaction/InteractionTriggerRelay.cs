using UnityEngine;

namespace CindarsHope.Interaction
{
    [DisallowMultipleComponent]
    public sealed class InteractionTriggerRelay : MonoBehaviour
    {
        [SerializeField] private InteractionSystem _interactionSystem;

        public void Configure(InteractionSystem interactionSystem)
        {
            _interactionSystem = interactionSystem;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_interactionSystem == null)
            {
                return;
            }

            _interactionSystem.RegisterCandidate(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_interactionSystem == null)
            {
                return;
            }

            _interactionSystem.UnregisterCandidate(other);
        }
    }
}