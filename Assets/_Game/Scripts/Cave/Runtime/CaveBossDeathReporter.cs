using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveBossDeathReporter : MonoBehaviour, ICaveBossReporter
    {
        private CaveRunManager _caveRunManager;
        private string _bossGateId;
        private string _bossEnemyId;
        private int _caveLevel;
        private int _checkpointUnlockedOnDefeat;
        private Vector3 _spawnPos;
        private bool _reported;

        public string BossGateId => _bossGateId ?? string.Empty;
        public string BossEnemyId => _bossEnemyId ?? string.Empty;
        public int CaveLevel => _caveLevel;
        public int CheckpointUnlockedOnDefeat => _checkpointUnlockedOnDefeat;
        public bool HasReportedDefeat => _reported;

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

        public void ReportDefeatedFromOwner(Vector3 deathPosition)
        {
            ReportDefeated(deathPosition, requireSpawnDistanceCheck: false, source: "owner");
        }

        // arch: implementação da porta ICaveBossReporter (Foundation) para que EnemyHealth (Combat)
        // resolva a morte do boss via GetComponent<ICaveBossReporter> sem nomear
        // CindarsHope.Cave.Runtime.CaveBossDeathReporter (corte do par mútuo Cave|Combat).
        void ICaveBossReporter.ReportDefeatedFromOwner(float deathPositionX, float deathPositionY, float deathPositionZ)
        {
            ReportDefeatedFromOwner(new Vector3(deathPositionX, deathPositionY, deathPositionZ));
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

            ReportDefeated(e.DeathPosition, requireSpawnDistanceCheck: true, source: "event-fallback");
        }

        private void ReportDefeated(Vector3 deathPosition, bool requireSpawnDistanceCheck, string source)
        {
            if (_reported || _caveRunManager == null)
            {
                return;
            }

            var distanceToSpawn = Vector3.Distance(deathPosition, _spawnPos);
            if (requireSpawnDistanceCheck && distanceToSpawn > 5f)
            {
                Debug.LogWarning($"CaveBossDeathReporter: Enemy {_bossEnemyId} killed but distance to boss spawn ({distanceToSpawn:F2}) exceeds threshold. Ignoring fallback event. Direct owner reporting should handle the real boss death.", this);
                return;
            }

            _reported = true;

            Debug.Log($"CaveBossDeathReporter: Boss {_bossEnemyId} defeated (gate={_bossGateId}, level={_caveLevel}, source={source}, distanceToSpawn={distanceToSpawn:F2}).", this);

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