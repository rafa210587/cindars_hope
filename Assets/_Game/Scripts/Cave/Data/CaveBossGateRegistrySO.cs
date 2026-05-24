using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Cave.Data
{
    [CreateAssetMenu(fileName = "CaveBossGateRegistry", menuName = "CindarsHope/Cave/Boss Gate Registry")]
    public sealed class CaveBossGateRegistrySO : ScriptableObject
    {
        private const string DefaultGateIdPrefix = "boss_gate_level_";
        private const string DefaultBiomeId = "biome_cave_earth";
        private const string DefaultBossEnemyId = "enemy_meteor_ooze_king";

        private static readonly int[] DefaultBossGateLevels = { 15, 30, 45, 60, 75, 90 };

        [SerializeField] private List<CaveBossGateDataSO> _gates = new List<CaveBossGateDataSO>();

        private readonly Dictionary<int, CaveBossGateDataSO> _runtimeDefaultGatesByLevel = new Dictionary<int, CaveBossGateDataSO>();
        private bool _runtimeFallbackLogged;

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

            return TryParseDefaultGateLevel(id, out var defaultLevel) ? GetRuntimeDefaultGate(defaultLevel) : null;
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

            return IsDefaultBossGateLevel(caveLevel) ? GetRuntimeDefaultGate(caveLevel) : null;
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

        public static bool IsDefaultBossGateLevel(int caveLevel)
        {
            foreach (var level in DefaultBossGateLevels)
            {
                if (level == caveLevel)
                {
                    return true;
                }
            }

            return false;
        }

        public static string BuildDefaultGateId(int caveLevel)
        {
            return $"{DefaultGateIdPrefix}{caveLevel}";
        }

        private CaveBossGateDataSO GetRuntimeDefaultGate(int caveLevel)
        {
            if (_runtimeDefaultGatesByLevel.TryGetValue(caveLevel, out var existing) && existing != null)
            {
                return existing;
            }

            var gate = CreateInstance<CaveBossGateDataSO>();
            gate.name = $"RuntimeDefaultBossGate_Level{caveLevel}";
            gate.Id = BuildDefaultGateId(caveLevel);
            gate.CaveLevel = caveLevel;
            gate.BiomeId = DefaultBiomeId;
            gate.BossEnemyId = DefaultBossEnemyId;
            gate.CheckpointUnlockedOnDefeat = caveLevel;
            _runtimeDefaultGatesByLevel[caveLevel] = gate;

            if (!_runtimeFallbackLogged)
            {
                _runtimeFallbackLogged = true;
                Debug.LogWarning(
                    "CaveBossGateRegistrySO: using runtime fallback boss gate set for levels 15, 30, 45, 60, 75 and 90. Regenerate CaveScene or persist CaveBossGateRegistry.asset to remove this warning.",
                    this);
            }

            return gate;
        }

        private static bool TryParseDefaultGateLevel(string id, out int caveLevel)
        {
            caveLevel = 0;
            if (string.IsNullOrWhiteSpace(id) || !id.StartsWith(DefaultGateIdPrefix))
            {
                return false;
            }

            var suffix = id.Substring(DefaultGateIdPrefix.Length);
            return int.TryParse(suffix, out caveLevel) && IsDefaultBossGateLevel(caveLevel);
        }
    }
}