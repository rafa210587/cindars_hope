using CindarsHope.Combat;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveBossDeathReporter : MonoBehaviour
    {
        private CaveRunManager _caveRunManager;
        private string _bossGateId;
        private string _bossEnemyId;
        private int _caveLevel;
        private int _checkpointUnlockedOnDefeat;
        private Vector3 _spawnPos;
        private bool _reported;

        public void Configure(CaveRunManager caveRunManager, string bossGateId, string bossEnemyId, int caveLevel, int checkpointUnlockedOnDefeat, Vector3 spawnPos)
        {
            _caveRunManager = caveRunManager;
            _bossGateId = bossGateId;
            _bossEnemyId = bossEnemyId;
            _caveLevel = caveLevel;
            _checkpointUnlockedOnDefeat = checkpointUnlockedOnDefeat;
            _spawnPos = spawnPos;
            _reported = false;

            Debug.Log($"CaveBossDeathReporter: Configured for boss {_bossEnemyId} (gate={_bossGateId}) at level {_caveLevel}. CheckpointUnlock={_checkpointUnlockedOnDefeat}.", this);
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            if (_reported || _caveRunManager == null)
            {
                return;
            }

            if (e.EnemyId != _bossEnemyId)
            {
                return;
            }

            var distanceToSpawn = Vector3.Distance(e.DeathPosition, _spawnPos);
            if (distanceToSpawn > 5f)
            {
                Debug.LogWarning($"CaveBossDeathReporter: Enemy {e.EnemyId} killed but distance to boss spawn ({distanceToSpawn:F2}) exceeds threshold. Ignoring.", this);
                return;
            }

            _reported = true;

            Debug.Log($"CaveBossDeathReporter: Boss {_bossEnemyId} defeated (gate={_bossGateId}).", this);

            _caveRunManager.MarkBossAsDefeated(_bossGateId, _caveLevel);

            if (_checkpointUnlockedOnDefeat > 0)
            {
                _caveRunManager.UnlockCheckpoint(_checkpointUnlockedOnDefeat);
                Debug.Log($"CaveBossDeathReporter: Checkpoint {_checkpointUnlockedOnDefeat} unlocked.", this);
            }

            GameEventBus.Publish(new CaveBossDefeatedEvent(_bossGateId, _caveLevel, _checkpointUnlockedOnDefeat));
        }
    }
}
