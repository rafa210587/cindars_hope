using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Interaction;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public sealed class CaveCheckpointPortal : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _checkpointLevel = 1;
        [SerializeField] private bool _isFarmEntrance;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Color _originalColor;
        private const float HoverAlphaMultiplier = 1.2f;

        public int CheckpointLevel => _checkpointLevel;
        public bool IsFarmEntrance => _isFarmEntrance;
        public string InteractionPrompt => "Checkpoint [E]";

        private void Start()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_spriteRenderer != null)
            {
                _originalColor = _spriteRenderer.color;
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            if (interactor == null)
                return false;

            var hasActiveModal = GameBootstrap.Instance?.ModalManager?.HasActiveModal ?? false;
            return !hasActiveModal;
        }

        public void Interact(GameObject interactor)
        {
            OpenCheckpointMenu();
        }

        private void OpenCheckpointMenu()
        {
            GameEventBus.Publish(new CaveCheckpointSelectionRequestedEvent());
            GameEventBus.Publish(new CaveCheckpointPortalOpenedEvent(_checkpointLevel, _isFarmEntrance));

            Debug.Log(
                $"CaveCheckpointPortal: Checkpoint selection requested. Level: {_checkpointLevel}, IsFarmEntrance: {_isFarmEntrance}",
                this);
        }

        public void OnHoverEnter()
        {
            if (_spriteRenderer != null)
            {
                var hoveredColor = _originalColor;
                hoveredColor.a = Mathf.Min(1f, _originalColor.a * HoverAlphaMultiplier);
                _spriteRenderer.color = hoveredColor;
            }
        }

        public void OnHoverExit()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _originalColor;
            }
        }
    }
}
