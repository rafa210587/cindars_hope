using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveSnapshotCacheManager : MonoBehaviour
    {
        [SerializeField] private CaveRunManager _runManager;
        private Dictionary<string, Dictionary<int, VisitedLevelSnapshot>> _snapshotCacheByRun = new();
        private string _currentRunSeed;

        private void OnEnable()
        {
            GameEventBus.Subscribe<CaveRunRegeneratedEvent>(OnRunRegenerated);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<CaveRunRegeneratedEvent>(OnRunRegenerated);
        }

        private void Awake()
        {
            if (_runManager == null)
            {
                _runManager = GetComponent<CaveRunManager>();
            }
        }

        private void OnRunRegenerated(CaveRunRegeneratedEvent evt)
        {
            _currentRunSeed = evt.CaveRunSeed;
            ClearCacheForRun(evt.CaveRunSeed);
            Debug.Log($"CaveSnapshotCacheManager: Run regenerated. Cache cleared for run {evt.CaveRunSeed}.", this);
        }

        public bool TryGetSnapshotFromCache(int caveLevel, out VisitedLevelSnapshot snapshot)
        {
            snapshot = null;

            if (_runManager == null)
            {
                return false;
            }

            var runSeed = _runManager.CaveRunSeed;
            if (string.IsNullOrWhiteSpace(runSeed))
            {
                return false;
            }

            if (!_snapshotCacheByRun.TryGetValue(runSeed, out var levelSnapshots))
            {
                return false;
            }

            return levelSnapshots.TryGetValue(caveLevel, out snapshot) && snapshot != null && snapshot.IsValid();
        }

        public void CacheSnapshot(int caveLevel, VisitedLevelSnapshot snapshot)
        {
            if (_runManager == null || string.IsNullOrWhiteSpace(_runManager.CaveRunSeed))
            {
                return;
            }

            if (!snapshot.IsValid())
            {
                Debug.LogWarning($"CaveSnapshotCacheManager: Attempted to cache invalid snapshot for level {caveLevel}.", this);
                return;
            }

            var runSeed = _runManager.CaveRunSeed;
            if (!_snapshotCacheByRun.ContainsKey(runSeed))
            {
                _snapshotCacheByRun[runSeed] = new Dictionary<int, VisitedLevelSnapshot>();
            }

            _snapshotCacheByRun[runSeed][caveLevel] = snapshot;
            Debug.Log($"CaveSnapshotCacheManager: Cached snapshot for level {caveLevel} in run {runSeed}.", this);
        }

        public void ClearCacheForRun(string runSeed)
        {
            if (string.IsNullOrWhiteSpace(runSeed))
            {
                return;
            }

            if (_snapshotCacheByRun.Remove(runSeed))
            {
                Debug.Log($"CaveSnapshotCacheManager: Cleared snapshot cache for run {runSeed}.", this);
            }
        }

        public void ClearAllCaches()
        {
            _snapshotCacheByRun.Clear();
            Debug.Log($"CaveSnapshotCacheManager: Cleared all snapshot caches.", this);
        }

        public int GetCachedSnapshotCount(string runSeed)
        {
            return _snapshotCacheByRun.TryGetValue(runSeed, out var cache) ? cache.Count : 0;
        }
    }
}
