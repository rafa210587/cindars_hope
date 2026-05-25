using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using CindarsHope.UI.Death;
using UnityEngine;

namespace CindarsHope.World
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider2D))]
    public class CorpseInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _corpseId;
        private Corpse _corpse;
        private CorpseRecoveryManager _recoveryManager;
        private CorpseRecoveryUIController _uiController;

        public string InteractableName => $"Corpse ({_corpse?.GetTotalRecoverableItems() ?? 0} items)";
        public string InteractionPrompt => "Press E to recover items";

        public void Initialize(Corpse corpse, CorpseRecoveryManager recoveryManager)
        {
            _corpse = corpse;
            _corpseId = corpse.CorpseId;
            _recoveryManager = recoveryManager;

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null)
            {
                _uiController = FindObjectOfType<CorpseRecoveryUIController>();
            }
        }

        public void Interact(GameObject interactor)
        {
            if (_corpse == null || _corpse.Status != CorpseStatus.Active)
            {
                return;
            }

            // Open recovery modal
            if (_uiController != null)
            {
                _uiController.OpenRecoveryModal(_corpse);
            }
            else
            {
                // Fallback: recover directly without modal
                _recoveryManager?.RecoverCorpse();
            }
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CorpseRecoveredEvent>(OnCorpseRecovered);
            GameEventBus.Subscribe<CorpseReplacedEvent>(OnCorpseReplaced);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CorpseRecoveredEvent>(OnCorpseRecovered);
            GameEventBus.Unsubscribe<CorpseReplacedEvent>(OnCorpseReplaced);
        }

        private void OnCorpseRecovered(CorpseRecoveredEvent evt)
        {
            if (evt.CorpseId != _corpseId)
            {
                return;
            }

            Destroy(gameObject);
        }

        private void OnCorpseReplaced(CorpseReplacedEvent evt)
        {
            if (evt.OldCorpseId != _corpseId)
            {
                return;
            }

            Destroy(gameObject);
        }
    }
}
