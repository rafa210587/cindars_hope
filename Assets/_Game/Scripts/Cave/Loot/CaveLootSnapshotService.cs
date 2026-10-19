using System;
using System.Collections.Generic;

namespace CindarsHope.Cave.Loot
{
    public class CaveSnapshotKey : IEquatable<CaveSnapshotKey>
    {
        public int CaveRunSeed { get; }
        public int CaveLevel { get; }
        public string SourceInstanceId { get; }

        public CaveSnapshotKey(int caveRunSeed, int caveLevel, string sourceInstanceId)
        {
            CaveRunSeed = caveRunSeed;
            CaveLevel = caveLevel;
            SourceInstanceId = sourceInstanceId ?? throw new ArgumentNullException(nameof(sourceInstanceId));
        }

        public bool Equals(CaveSnapshotKey other) =>
            other != null && CaveRunSeed == other.CaveRunSeed && CaveLevel == other.CaveLevel && SourceInstanceId == other.SourceInstanceId;

        public override bool Equals(object obj) => Equals(obj as CaveSnapshotKey);
        public override int GetHashCode() => HashCode.Combine(CaveRunSeed, CaveLevel, SourceInstanceId);
    }

    public class CaveLootSnapshotService
    {
        private readonly Dictionary<CaveSnapshotKey, CaveLootSnapshotEntry> _snapshots =
            new Dictionary<CaveSnapshotKey, CaveLootSnapshotEntry>();

        // Derive loot seed deterministically — same inputs always produce same seed
        public static int DeriveLootSeed(int caveRunSeed, int caveLevel, string sourceInstanceId)
        {
            int hash = 17;
            hash = hash * 31 + caveRunSeed;
            hash = hash * 31 + caveLevel;
            hash = hash * 31 + (sourceInstanceId?.GetHashCode() ?? 0);
            return hash;
        }

        // Get or create snapshot — revisiting same key returns existing entry (stable run contract)
        public CaveLootSnapshotEntry GetOrCreate(int caveRunSeed, int caveLevel, string sourceInstanceId,
            CaveLootSourceType sourceType, int gridX = 0, int gridY = 0)
        {
            var key = new CaveSnapshotKey(caveRunSeed, caveLevel, sourceInstanceId);
            if (_snapshots.TryGetValue(key, out var existing))
                return existing;

            var entry = new CaveLootSnapshotEntry
            {
                SnapshotId = $"snap_{caveRunSeed}_{caveLevel}_{sourceInstanceId}",
                CaveRunSeed = caveRunSeed,
                CaveLevel = caveLevel,
                SourceInstanceId = sourceInstanceId,
                SourceType = sourceType,
                GridX = gridX,
                GridY = gridY,
                LootRollSeed = DeriveLootSeed(caveRunSeed, caveLevel, sourceInstanceId)
            };
            _snapshots[key] = entry;
            return entry;
        }

        // Open a chest — idempotent: second open always fails
        public bool TryOpen(CaveLootSnapshotEntry entry, int currentDay)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (entry.IsOpened) return false;

            entry.IsOpened = true;
            entry.OpenedDay = currentDay;
            return true;
        }

        // Deplete a mining node — state persists per policy
        public bool TryDeplete(CaveLootSnapshotEntry entry, int currentDay)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (entry.IsDepleted) return false;

            entry.IsDepleted = true;
            entry.DepletedDay = currentDay;
            return true;
        }

        // Mark a reward consumed (idempotent)
        public void MarkRewardConsumed(CaveLootSnapshotEntry entry, string rewardFlag)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            if (!entry.RewardConsumedFlags.Contains(rewardFlag))
                entry.RewardConsumedFlags.Add(rewardFlag);
        }

        public bool IsRewardConsumed(CaveLootSnapshotEntry entry, string rewardFlag) =>
            entry?.RewardConsumedFlags.Contains(rewardFlag) ?? false;

        // Reset snapshots for a new run — called only on new game, death, or explicit debug command
        // ForwardExit/BackExit must NOT call this
        public void ResetForNewRun()
        {
            _snapshots.Clear();
        }

        // Debug regeneration — only via explicit debug command, never via exit navigation
        public void DebugForceRegenerateLevel(int caveRunSeed, int caveLevel)
        {
            var toRemove = new List<CaveSnapshotKey>();
            foreach (var key in _snapshots.Keys)
                if (key.CaveRunSeed == caveRunSeed && key.CaveLevel == caveLevel)
                    toRemove.Add(key);
            foreach (var key in toRemove)
                _snapshots.Remove(key);
        }

        public IReadOnlyDictionary<CaveSnapshotKey, CaveLootSnapshotEntry> AllSnapshots => _snapshots;
    }
}
