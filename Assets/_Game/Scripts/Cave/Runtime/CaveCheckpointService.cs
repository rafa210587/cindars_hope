using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveCheckpointService : MonoBehaviour
    {
        private static readonly int[] OfficialCheckpoints = { 1, 15, 30, 45, 60, 75, 90 };

        [SerializeField] private CaveRunManager _runManager;

        private void Awake()
        {
            if (_runManager == null)
            {
                _runManager = GetComponent<CaveRunManager>();
            }

            _runManager?.InitializeIfNeeded();
        }

        public bool IsCheckpointLevel(int level)
        {
            foreach (var checkpoint in OfficialCheckpoints)
            {
                if (checkpoint == level)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCheckpointUnlocked(int level)
        {
            if (level == 1)
            {
                return true;
            }

            return _runManager != null
                && _runManager.State.UnlockedCheckpoints.Contains(level);
        }

        public bool TryUnlockCheckpoint(int level)
        {
            if (!IsCheckpointLevel(level))
            {
                return false;
            }

            if (_runManager == null)
            {
                Debug.LogWarning("CaveCheckpointService cannot unlock checkpoint without CaveRunManager.", this);
                return false;
            }

            _runManager.InitializeIfNeeded();
            if (!_runManager.State.UnlockedCheckpoints.Add(level))
            {
                return false;
            }

            GameEventBus.Publish(new CaveCheckpointUnlockedEvent(level));
            Debug.Log($"CaveCheckpointService: checkpoint {level} unlocked.", this);
            return true;
        }

        public IReadOnlyCollection<int> GetUnlockedCheckpoints()
        {
            if (_runManager == null)
            {
                return new[] { 1 };
            }

            _runManager.InitializeIfNeeded();
            return _runManager.State.UnlockedCheckpoints;
        }
    }
}
