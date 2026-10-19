using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveEntryController : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private CaveLevelRuntimeController _levelController;

        private int _pendingCheckpointLevel = 0;
        private bool _isAwaitingSelection = false;

        private void OnEnable()
        {
            GameEventBus.Subscribe<CaveCheckpointSelectedEvent>(OnCheckpointSelected);
            GameEventBus.Subscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CaveCheckpointSelectedEvent>(OnCheckpointSelected);
            GameEventBus.Unsubscribe<CaveLevelEnteredEvent>(OnCaveLevelEntered);
        }

        public void RequestCheckpointSelection()
        {
            if (_caveRunManager == null)
            {
                return;
            }

            _isAwaitingSelection = true;
            _pendingCheckpointLevel = 0;

            GameEventBus.Publish(new CaveCheckpointSelectionRequestedEvent());
            Debug.Log("CaveEntryController: Requested checkpoint selection.", this);
        }

        private void OnCheckpointSelected(CaveCheckpointSelectedEvent evt)
        {
            if (!_isAwaitingSelection)
            {
                return;
            }

            _pendingCheckpointLevel = evt.SelectedCheckpointLevel;
            _isAwaitingSelection = false;

            if (_caveRunManager != null && _levelController != null)
            {
                _caveRunManager.EnterLevel(_pendingCheckpointLevel);
                _levelController.SetSpawnAnchorForNextGeneration(CaveSpawnAnchor.Entrance);
                _levelController.GenerateCurrentLevel();

                Debug.Log($"CaveEntryController: Entered checkpoint level {_pendingCheckpointLevel}.", this);
            }
        }

        private void OnCaveLevelEntered(CaveLevelEnteredEvent evt)
        {
            _isAwaitingSelection = false;
        }

        public bool IsAwaitingCheckpointSelection => _isAwaitingSelection;
        public int PendingCheckpointLevel => _pendingCheckpointLevel;
    }
}
