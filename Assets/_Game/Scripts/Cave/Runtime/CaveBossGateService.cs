using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveBossGateService : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _runManager;
        [SerializeField] private CaveBossGateRegistrySO _bossGateRegistry;

        private Dictionary<string, bool> _gateCompletionStates = new Dictionary<string, bool>();
        private Dictionary<string, HashSet<string>> _uniqueRewardsClaimed = new Dictionary<string, HashSet<string>>();

        private void Awake()
        {
            if (_runManager == null)
            {
                _runManager = GetComponent<CaveRunManager>();
            }

            _runManager?.InitializeIfNeeded();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<CaveBossDefeatedEvent>(OnBossDefeated);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CaveBossDefeatedEvent>(OnBossDefeated);
        }

        public bool IsBossGateLevel(int caveLevel)
        {
            return _bossGateRegistry != null && _bossGateRegistry.IsBossGateLevel(caveLevel);
        }

        public bool IsGateCompleted(string gateId)
        {
            if (string.IsNullOrWhiteSpace(gateId) || _runManager == null)
            {
                return false;
            }

            var defeatState = _runManager.State.BossDefeatStates.TryGetValue(gateId, out var state) ? state : null;
            return defeatState != null && defeatState.IsDefeated;
        }

        public bool IsGateCompletedByLevel(int caveLevel)
        {
            var gate = _bossGateRegistry?.GetGateByLevel(caveLevel);
            return gate != null && IsGateCompleted(gate.Id);
        }

        public bool CanProgressBeyondGate(int caveLevel)
        {
            if (!IsBossGateLevel(caveLevel))
            {
                return true;
            }

            return IsGateCompletedByLevel(caveLevel);
        }

        public void CompleteGate(string gateId)
        {
            if (string.IsNullOrWhiteSpace(gateId) || _runManager == null)
            {
                Debug.LogWarning($"CaveBossGateService: Cannot complete gate '{gateId}' - invalid gate ID or missing CaveRunManager");
                return;
            }

            var gate = _bossGateRegistry?.GetGateById(gateId);
            if (gate == null)
            {
                Debug.LogWarning($"CaveBossGateService: Gate '{gateId}' not found in registry");
                return;
            }

            if (!_runManager.State.BossDefeatStates.ContainsKey(gateId))
            {
                _runManager.State.BossDefeatStates[gateId] = new CaveBossDefeatState { IsDefeated = true };
            }
            else
            {
                _runManager.State.BossDefeatStates[gateId].IsDefeated = true;
            }

            GameEventBus.Publish(new CaveBossGateCompletedEvent(gateId, gate.CaveLevel));
            Debug.Log($"CaveBossGateService: Gate '{gateId}' (level {gate.CaveLevel}) completed.");
        }

        public bool HasClaimedUniqueReward(string gateId, string rewardId)
        {
            if (string.IsNullOrWhiteSpace(gateId) || string.IsNullOrWhiteSpace(rewardId))
            {
                return false;
            }

            var key = $"{gateId}:{rewardId}";
            return _runManager?.State.BossDefeatStates.TryGetValue(gateId, out var state) ?? false
                && state.UniqueRewardsClaimed.Contains(rewardId);
        }

        public void MarkUniqueRewardClaimed(string gateId, string rewardId)
        {
            if (string.IsNullOrWhiteSpace(gateId) || string.IsNullOrWhiteSpace(rewardId) || _runManager == null)
            {
                return;
            }

            if (!_runManager.State.BossDefeatStates.TryGetValue(gateId, out var state))
            {
                state = new CaveBossDefeatState();
                _runManager.State.BossDefeatStates[gateId] = state;
            }

            state.UniqueRewardsClaimed.Add(rewardId);
            GameEventBus.Publish(new CaveBossUniqueRewardClaimedEvent(gateId, rewardId));
            Debug.Log($"CaveBossGateService: Reward '{rewardId}' claimed for gate '{gateId}'");
        }

        private void OnBossDefeated(CaveBossDefeatedEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.BossGateId))
            {
                return;
            }

            CompleteGate(evt.BossGateId);
        }
    }
}
