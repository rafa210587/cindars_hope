using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Economy
{
    [DisallowMultipleComponent]
    public sealed class BuyItemPoint : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _sourceId = "shop_town_seed";
        [SerializeField] private string _itemId = "item_seed_wheat";
        [SerializeField] private int _amount = 3;
        [SerializeField] private int _totalCost = 5;
        [SerializeField] private string _interactionPrompt = "Comprar";
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Collider2D _collider;

        public string InteractionPrompt => string.IsNullOrWhiteSpace(_interactionPrompt) ? "Comprar" : _interactionPrompt;

        public bool CanInteract(GameObject interactor)
        {
            return !string.IsNullOrWhiteSpace(_itemId) && _amount > 0;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                Debug.LogWarning($"{nameof(BuyItemPoint)} on '{name}' is not configured for purchase.", this);
                return;
            }

            GameEventBus.Publish(new ItemPurchaseRequestedEvent(_itemId, _amount, _totalCost, _sourceId));
        }

        private void Reset()
        {
            EnsureComponents();
        }

        private void OnValidate()
        {
            _amount = Mathf.Max(1, _amount);
            _totalCost = Mathf.Max(0, _totalCost);
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
