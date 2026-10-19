using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public sealed class SellAllPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _sourceId = "shop_town_sell_box";
        [SerializeField] private string _interactionPrompt = "Vender itens";
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        public string InteractionPrompt => string.IsNullOrWhiteSpace(_interactionPrompt) ? "Vender itens" : _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrWhiteSpace(_sourceId);
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"{nameof(SellAllPoint)} on '{name}' has no source id configured.", this);
                return;
            }

            GameEventBus.Publish(new SellAllRequestedEvent(_sourceId));
        }

        /// <summary>Runtime configuration for procedurally spawned sell points (cave wandering merchant).</summary>
        public void ConfigureSource(string sourceId, string interactionPrompt)
        {
            _sourceId = sourceId;
            _interactionPrompt = interactionPrompt;
            EnsureComponents();
        }

        private void Reset()
        {
            EnsureComponents();
        }

        private void OnValidate()
        {
            EnsureComponents();
        }

        private void EnsureComponents()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }
    }
}
