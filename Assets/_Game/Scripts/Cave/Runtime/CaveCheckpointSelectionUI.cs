using System.Collections.Generic;
using System.Linq;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveCheckpointSelectionUI : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _caveRunManager;

        private List<int> _availableCheckpoints = new List<int>();
        private int _selectedIndex = 0;
        private bool _isSelectionActive = false;

        private void OnEnable()
        {
            GameEventBus.Subscribe<CaveCheckpointSelectionRequestedEvent>(OnCheckpointSelectionRequested);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CaveCheckpointSelectionRequestedEvent>(OnCheckpointSelectionRequested);
        }

        private void OnCheckpointSelectionRequested(CaveCheckpointSelectionRequestedEvent evt)
        {
            if (_caveRunManager == null)
            {
                return;
            }

            _availableCheckpoints = _caveRunManager.State.UnlockedCheckpoints.OrderBy(cp => cp).ToList();
            if (_availableCheckpoints.Count == 0)
            {
                _availableCheckpoints.Add(1);
            }

            _selectedIndex = 0;
            _isSelectionActive = true;

            Debug.Log($"CaveCheckpointSelectionUI: Selection activated. Available checkpoints: {string.Join(", ", _availableCheckpoints)}", this);
        }

        private void Update()
        {
            if (!_isSelectionActive)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                _selectedIndex = (_selectedIndex - 1 + _availableCheckpoints.Count) % _availableCheckpoints.Count;
                Debug.Log($"CaveCheckpointSelectionUI: Selected checkpoint {_availableCheckpoints[_selectedIndex]}.", this);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                _selectedIndex = (_selectedIndex + 1) % _availableCheckpoints.Count;
                Debug.Log($"CaveCheckpointSelectionUI: Selected checkpoint {_availableCheckpoints[_selectedIndex]}.", this);
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
            {
                ConfirmSelection();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSelection();
            }
        }

        private void ConfirmSelection()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _availableCheckpoints.Count)
            {
                var selectedCheckpoint = _availableCheckpoints[_selectedIndex];
                GameEventBus.Publish(new CaveCheckpointSelectedEvent(selectedCheckpoint));
                Debug.Log($"CaveCheckpointSelectionUI: Confirmed checkpoint {selectedCheckpoint}.", this);
                _isSelectionActive = false;
            }
        }

        private void CancelSelection()
        {
            Debug.Log("CaveCheckpointSelectionUI: Selection cancelled.", this);
            _isSelectionActive = false;
            _availableCheckpoints.Clear();
        }

        public bool IsSelectionActive => _isSelectionActive;
        public int SelectedCheckpoint => _selectedIndex >= 0 && _selectedIndex < _availableCheckpoints.Count ? _availableCheckpoints[_selectedIndex] : 1;
    }
}
