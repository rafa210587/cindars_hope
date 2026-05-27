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
        private bool _isInitialized;

        private void Start()
        {
            TryInitialize("Start");
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CorpseCreatedEvent>(OnCorpseCreated);
            TryInitialize("OnEnable");
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CorpseCreatedEvent>(OnCorpseCreated);
        }

        public void Initialize()
        {
            TryInitialize("Initialize");
        }

        private bool TryInitialize(string reason)
        {
            if (_isInitialized)
            {
                return true;
            }

            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return false;
            }

            _modalManager = bootstrap.ModalManager;
            _recoveryManager = bootstrap.CorpseRecoveryManager;

            if (_modalManager == null || _recoveryManager == null)
            {
                Debug.LogWarning($"[CorpseRecoveryUIController] Initialization incomplete from {reason}. modalManager={_modalManager != null}, recoveryManager={_recoveryManager != null}", this);
                return false;
            }

            _isInitialized = true;
            return true;
        }

        private void OnCorpseCreated(CorpseCreatedEvent evt)
        {
            Debug.Log($"[CorpseRecoveryUIController] Corpse created: {evt.CorpseId}. Modal will open on interactable interaction.");
        }

        public void OpenRecoveryModal(Corpse corpse)
        {
            if (!TryInitialize("OpenRecoveryModal"))
            {
                Debug.LogError("[CorpseRecoveryUIController] Cannot open modal: managers not initialized", this);
                return;
            }

            if (_recoveryModalPrefab == null)
            {
                Debug.LogWarning("[CorpseRecoveryUIController] Recovery modal prefab not assigned", this);
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
