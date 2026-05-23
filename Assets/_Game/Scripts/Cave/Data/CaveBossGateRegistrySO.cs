using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBossGateRegistry", menuName = "CindarsHope/Cave/Boss Gate Registry")]
    public sealed class CaveBossGateRegistrySO : ScriptableObject
    {
        private const string DefaultGateId = "boss_gate_level_15";
        private const int DefaultGateLevel = 15;
        private const int DefaultCheckpointUnlockedOnDefeat = 15;
        private const string DefaultBiomeId = "biome_cave_earth";
        private const string DefaultBossEnemyId = "enemy_meteor_ooze_king";

        [SerializeField] private List<CaveBossGateDataSO> _gates = new List<CaveBossGateDataSO>();

        private CaveBossGateDataSO _runtimeDefaultGate;

        public IReadOnlyList<CaveBossGateDataSO> Gates => _gates.AsReadOnly();

        public CaveBossGateDataSO GetGateById(string id)
        {
            foreach (var gate in _gates)
            {
                if (gate != null && gate.Id == id)
                {
                    return gate;
                }
            }

            return id == DefaultGateId ? GetRuntimeDefaultGate() : null;
        }

        public CaveBossGateDataSO GetGateByLevel(int caveLevel)
        {
            foreach (var gate in _gates)
            {
                if (gate != null && gate.CaveLevel == caveLevel)
                {
                    return gate;
                }
            }

            return caveLevel == DefaultGateLevel ? GetRuntimeDefaultGate() : null;
        }

        public bool IsBossGateLevel(int caveLevel)
        {
            return GetGateByLevel(caveLevel) != null;
        }

        public int GetNextCheckpointAfterDefeat(int caveLevel)
        {
            var gate = GetGateByLevel(caveLevel);
            return gate != null ? gate.CheckpointUnlockedOnDefeat : caveLevel;
        }

        private CaveBossGateDataSO GetRuntimeDefaultGate()
        {
            if (_runtimeDefaultGate != null)
            {
                return _runtimeDefaultGate;
            }

            _runtimeDefaultGate = CreateInstance<CaveBossGateDataSO>();
            _runtimeDefaultGate.name = "RuntimeDefaultBossGate_Level15";
            _runtimeDefaultGate.Id = DefaultGateId;
            _runtimeDefaultGate.CaveLevel = DefaultGateLevel;
            _runtimeDefaultGate.BiomeId = DefaultBiomeId;
            _runtimeDefaultGate.BossEnemyId = DefaultBossEnemyId;
            _runtimeDefaultGate.CheckpointUnlockedOnDefeat = DefaultCheckpointUnlockedOnDefeat;

            Debug.LogWarning(
                "CaveBossGateRegistrySO: registry has no matching persisted gate, using runtime fallback boss_gate_level_15. Regenerate CaveScene or fix CaveBossGateRegistry.asset to persist this gate.",
                this);

            return _runtimeDefaultGate;
        }
    }
}