using CindarsHope.Cave.Data;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveBossDefeatMonitor : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _caveRunManager;
        [SerializeField] private CaveBossGateRegistrySO _bossGateRegistry;

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (_caveRunManager == null || _bossGateRegistry == null)
            {
                return;
            }

            var currentLevel = _caveRunManager.CurrentCaveLevel;
            var bossGate = _bossGateRegistry.GetGateByLevel(currentLevel);

            if (bossGate == null)
            {
                return;
            }

            if (evt.EnemyId != bossGate.BossEnemyId)
            {
                return;
            }

            _caveRunManager.MarkBossAsDefeated(bossGate.Id, currentLevel);

            if (!_caveRunManager.State.UnlockedCheckpoints.Contains(bossGate.CheckpointUnlockedOnDefeat))
            {
                _caveRunManager.State.UnlockedCheckpoints.Add(bossGate.CheckpointUnlockedOnDefeat);
                GameEventBus.Publish(new CaveCheckpointUnlockedEvent(bossGate.CheckpointUnlockedOnDefeat));
                Debug.Log($"CaveBossDefeatMonitor: Checkpoint {bossGate.CheckpointUnlockedOnDefeat} unlocked after boss defeat.", this);
            }

            GameEventBus.Publish(new CaveBossDefeatedEvent(bossGate.Id, currentLevel, bossGate.CheckpointUnlockedOnDefeat));
            Debug.Log($"CaveBossDefeatMonitor: Boss '{bossGate.Id}' at level {currentLevel} defeated. Checkpoint {bossGate.CheckpointUnlockedOnDefeat} unlocked.", this);
        }
    }
}
