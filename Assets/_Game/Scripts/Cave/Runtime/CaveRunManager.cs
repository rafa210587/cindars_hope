using System;
using System.Collections.Generic;
using CindarsHope.Cave.Data;
using CindarsHope.Core;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core.Events;
using CindarsHope.Save;
using CindarsHope.SceneManagement;
using UnityEngine;

using CavePlayerDefeatedEvent = CindarsHope.Core.Events.CavePlayerDefeatedEvent;

namespace CindarsHope.Cave.Runtime
{
    [DisallowMultipleComponent]
    public sealed class CaveRunManager : MonoBehaviour
    {
        [SerializeField] private string _defaultWorldSeed = "cindars_world_seed_001";
        [SerializeField] private int _currentCaveLevel = 1;
        [SerializeField] private int _deepestLayerReached = 1;
        [SerializeField] private string _caveWorldSeed;
        [SerializeField] private string _caveRunSeed;
        [SerializeField] private CaveBossGateRegistrySO _bossGateRegistry;

        private readonly CaveRuntimeState _state = new CaveRuntimeState();

        public int CurrentCaveLevel => _state.CurrentCaveLevel;
        public int DeepestLayerReached => _state.DeepestLayerReached;
        public string CaveWorldSeed => _state.CaveWorldSeed;
        public string CaveRunSeed => _state.CaveRunSeed;
        public CaveRuntimeState State => _state;

        private void Awake()
        {
            RestoreCachedStateIfNeeded();
            InitializeIfNeeded();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<SceneTransitionStartedEvent>(OnSceneTransitionStarted);
        }

        private void OnSceneTransitionStarted(SceneTransitionStartedEvent evt)
        {
            if (evt.SourceSceneName == "CaveScene")
            {
                CacheCurrentStateInBootstrap();
                Debug.Log($"CaveRunManager: saved state to bootstrap cache before leaving CaveScene. RunSeed={_state.CaveRunSeed}", this);
            }
        }

        private void RestoreCachedStateIfNeeded()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            var cachedState = bootstrap.TakeCachedCaveRunState();
            if (cachedState != null)
            {
                _state.CurrentCaveLevel = cachedState.CurrentCaveLevel;
                _state.DeepestLayerReached = cachedState.DeepestLayerReached;
                _state.CaveWorldSeed = cachedState.CaveWorldSeed;
                _state.CaveRunSeed = cachedState.CaveRunSeed;
                _state.UnlockedCheckpoints.Clear();
                foreach (var cp in cachedState.UnlockedCheckpoints)
                {
                    _state.UnlockedCheckpoints.Add(cp);
                }
                _state.DepletedNodeIds.Clear();
                foreach (var nodeId in cachedState.DepletedNodeIds)
                {
                    _state.DepletedNodeIds.Add(nodeId);
                }
                _state.VisitedLevelSnapshots.Clear();
                foreach (var kvp in cachedState.VisitedLevelSnapshots)
                {
                    _state.VisitedLevelSnapshots[kvp.Key] = kvp.Value;
                }
                _state.BossDefeatStates.Clear();
                foreach (var kvp in cachedState.BossDefeatStates)
                {
                    _state.BossDefeatStates[kvp.Key] = kvp.Value;
                }

                _currentCaveLevel = _state.CurrentCaveLevel;
                _deepestLayerReached = _state.DeepestLayerReached;
                _caveWorldSeed = _state.CaveWorldSeed;
                _caveRunSeed = _state.CaveRunSeed;

                Debug.Log($"CaveRunManager: restored state from bootstrap cache. RunSeed={_state.CaveRunSeed}, Level={_state.CurrentCaveLevel}", this);
            }
        }

        private void CacheCurrentStateInBootstrap()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap == null)
            {
                return;
            }

            bootstrap.SetCachedCaveRunState(_state);
        }

        public void InitializeIfNeeded()
        {
            if (string.IsNullOrWhiteSpace(_caveWorldSeed))
            {
                _caveWorldSeed = string.IsNullOrWhiteSpace(_defaultWorldSeed)
                    ? $"world_{Guid.NewGuid():N}"
                    : _defaultWorldSeed;
            }

            if (string.IsNullOrWhiteSpace(_caveRunSeed))
            {
                _caveRunSeed = CreateRunSeed("initial");
            }

            _currentCaveLevel = Mathf.Max(1, _currentCaveLevel);
            _deepestLayerReached = Mathf.Max(_currentCaveLevel, _deepestLayerReached);
            SyncSerializedToState();
            EnsureCheckpointOne();
        }

        public void EnterLevel(int caveLevel)
        {
            InitializeIfNeeded();
            _currentCaveLevel = Mathf.Max(1, caveLevel);
            _deepestLayerReached = Mathf.Max(_deepestLayerReached, _currentCaveLevel);
            SyncSerializedToState();
        }

        public void GenerateNewRunSeed(string reason)
        {
            InitializeIfNeeded();
            _caveRunSeed = CreateRunSeed(reason);
            SyncSerializedToState();
            GameEventBus.Publish(new CaveRunRegeneratedEvent(_state.CaveRunSeed, reason));
            Debug.Log($"CaveRunManager: run regenerated. WorldSeed={_state.CaveWorldSeed}, RunSeed={_state.CaveRunSeed}, Reason={reason}.", this);
        }

        public CaveSaveData CaptureSaveData()
        {
            InitializeIfNeeded();
            var saveData = new CaveSaveData
            {
                CurrentCaveLevel = _state.CurrentCaveLevel,
                DeepestLayerReached = _state.DeepestLayerReached,
                CaveWorldSeed = _state.CaveWorldSeed ?? string.Empty,
                CaveRunSeed = _state.CaveRunSeed ?? string.Empty,
                UnlockedCheckpoints = new List<int>(_state.UnlockedCheckpoints),
                DepletedNodeIds = new List<string>(_state.DepletedNodeIds)
            };
            saveData.PopulateSnapshots(_state.VisitedLevelSnapshots);
            saveData.PopulateBossDefeatStates(_state.BossDefeatStates);
            return saveData;
        }

        public void RestoreFromSaveData(CaveSaveData saveData)
        {
            if (saveData == null)
            {
                InitializeIfNeeded();
                return;
            }

            _currentCaveLevel = Mathf.Max(1, saveData.CurrentCaveLevel);
            _deepestLayerReached = Mathf.Max(_currentCaveLevel, saveData.DeepestLayerReached);
            _caveWorldSeed = string.IsNullOrWhiteSpace(saveData.CaveWorldSeed) ? _defaultWorldSeed : saveData.CaveWorldSeed;
            _caveRunSeed = string.IsNullOrWhiteSpace(saveData.CaveRunSeed) ? CreateRunSeed("restore_missing") : saveData.CaveRunSeed;

            _state.UnlockedCheckpoints.Clear();
            if (saveData.UnlockedCheckpoints != null)
            {
                foreach (var checkpoint in saveData.UnlockedCheckpoints)
                {
                    if (checkpoint > 0)
                    {
                        _state.UnlockedCheckpoints.Add(checkpoint);
                    }
                }
            }

            _state.DepletedNodeIds.Clear();
            if (saveData.DepletedNodeIds != null)
            {
                foreach (var nodeId in saveData.DepletedNodeIds)
                {
                    if (!string.IsNullOrWhiteSpace(nodeId))
                    {
                        _state.DepletedNodeIds.Add(nodeId);
                    }
                }
            }

            _state.VisitedLevelSnapshots.Clear();
            var restoredSnapshots = saveData.RestoreSnapshots();
            foreach (var kvp in restoredSnapshots)
            {
                _state.VisitedLevelSnapshots[kvp.Key] = kvp.Value;
            }

            _state.BossDefeatStates.Clear();
            var restoredBossStates = saveData.RestoreBossDefeatStates();
            foreach (var kvp in restoredBossStates)
            {
                _state.BossDefeatStates[kvp.Key] = kvp.Value;
            }

            SyncSerializedToState();
            EnsureCheckpointOne();
        }

        public bool RegisterDepletedNode(string nodeInstanceId)
        {
            InitializeIfNeeded();
            return !string.IsNullOrWhiteSpace(nodeInstanceId) && _state.DepletedNodeIds.Add(nodeInstanceId);
        }

        public bool IsNodeDepleted(string nodeInstanceId)
        {
            InitializeIfNeeded();
            return !string.IsNullOrWhiteSpace(nodeInstanceId) && _state.DepletedNodeIds.Contains(nodeInstanceId);
        }

        public void HandlePlayerDefeated()
        {
            InitializeIfNeeded();
            GenerateNewRunSeed("PlayerDefeated");
            _state.VisitedLevelSnapshots.Clear();
            _state.DepletedNodeIds.Clear();
            Debug.Log($"CaveRunManager: player defeated. Snapshots cleared, new run seed generated. Checkpoints remain: {string.Join(",", _state.UnlockedCheckpoints)}.", this);
            GameEventBus.Publish(new CavePlayerDefeatedEvent(_state.CurrentCaveLevel));
        }

        public bool CheckBossGate(int targetLevel)
        {
            InitializeIfNeeded();

            if (_bossGateRegistry == null)
            {
                return true;
            }

            var gate = _bossGateRegistry.GetGateByLevel(_state.CurrentCaveLevel);
            if (gate == null)
            {
                return true;
            }

            if (targetLevel <= gate.CaveLevel)
            {
                return true;
            }

            var isDefeated = IsBossDefeated(gate.Id);
            if (!isDefeated)
            {
                Debug.LogWarning($"CaveRunManager: cannot advance from level {_state.CurrentCaveLevel} to {targetLevel}. Boss gate '{gate.Id}' at level {gate.CaveLevel} not defeated.", this);
                return false;
            }

            return true;
        }

        public bool IsBossDefeated(string bossGateId)
        {
            InitializeIfNeeded();
            if (string.IsNullOrWhiteSpace(bossGateId))
            {
                return false;
            }

            if (_state.BossDefeatStates.TryGetValue(bossGateId, out var state))
            {
                return state.IsDefeated;
            }

            return false;
        }

        public void MarkBossAsDefeated(string bossGateId, int caveLevel)
        {
            InitializeIfNeeded();
            if (string.IsNullOrWhiteSpace(bossGateId))
            {
                return;
            }

            if (!_state.BossDefeatStates.ContainsKey(bossGateId))
            {
                _state.BossDefeatStates[bossGateId] = new CaveBossDefeatState(bossGateId, caveLevel, false);
            }

            _state.BossDefeatStates[bossGateId].MarkAsDefeated();
            Debug.Log($"CaveRunManager: boss '{bossGateId}' at level {caveLevel} marked as defeated.", this);
        }

        private void SyncSerializedToState()
        {
            _state.CurrentCaveLevel = Mathf.Max(1, _currentCaveLevel);
            _state.DeepestLayerReached = Mathf.Max(_state.CurrentCaveLevel, _deepestLayerReached);
            _state.CaveWorldSeed = _caveWorldSeed ?? string.Empty;
            _state.CaveRunSeed = _caveRunSeed ?? string.Empty;
        }

        private void EnsureCheckpointOne()
        {
            _state.UnlockedCheckpoints.Add(1);
        }

        private static string CreateRunSeed(string reason)
        {
            return $"run_{DateTime.UtcNow:yyyyMMddHHmmssfff}_{reason}_{Guid.NewGuid():N}";
        }
    }
}
