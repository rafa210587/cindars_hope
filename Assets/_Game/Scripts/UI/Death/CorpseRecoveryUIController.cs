using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Player.Death;
using CindarsHope.UI.Modal;
using UnityEngine;

namespace CindarsHope.UI.Death
{
    [DisallowMultipleComponent]
    public class CorpseRecoveryUIController : MonoBehaviour
    {
        [SerializeField] private CorpseRecoveryModal _recoveryModalPrefab;
        private ModalManager _modalManager;
        private CorpseRecoveryManager _recoveryManager;

        private void OnEnable()
        {
            GameEventBus.Subscribe<CorpseCreatedEvent>(OnCorpseCreated);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CorpseCreatedEvent>(OnCorpseCreated);
        }

        public void Initialize()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                Debug.LogError("[CorpseRecoveryUIController] GameBootstrap not found");
                return;
            }

            _modalManager = bootstrap.ModalManager;
            _recoveryManager = bootstrap.CorpseRecoveryManager;

            if (_modalManager == null)
            {
                Debug.LogWarning("[CorpseRecoveryUIController] ModalManager not found");
            }

            if (_recoveryManager == null)
            {
                Debug.LogWarning("[CorpseRecoveryUIController] CorpseRecoveryManager not found");
            }
        }

        private void OnCorpseCreated(CorpseCreatedEvent evt)
        {
            Debug.Log($"[CorpseRecoveryUIController] Corpse created: {evt.CorpseId}. Modal will open on interactable interaction.");
        }

        public void OpenRecoveryModal(Corpse corpse)
        {
            if (_modalManager == null || _recoveryManager == null)
            {
                Debug.LogError("[CorpseRecoveryUIController] Cannot open modal: missing managers");
                return;
            }

            if (_recoveryModalPrefab == null)
            {
                Debug.LogWarning("[CorpseRecoveryUIController] Recovery modal prefab not assigned");
                return;
            }

            var modalInstance = _modalManager.OpenModal(_recoveryModalPrefab);
            if (modalInstance is CorpseRecoveryModal recoveryModal)
            {
                recoveryModal.Initialize(corpse, _recoveryManager);
                Debug.Log($"[CorpseRecoveryUIController] Recovery modal opened for corpse {corpse.CorpseId}");
            }
        }
    }
}
