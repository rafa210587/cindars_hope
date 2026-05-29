using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [DisallowMultipleComponent]
    public class BestiaryManager : MonoBehaviour
    {
        private readonly Dictionary<string, BestiaryEntry> _entries = new Dictionary<string, BestiaryEntry>();

        public int EntryCount => _entries.Count;

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            GameEventBus.Subscribe<EnemySeenEvent>(OnEnemySeen);
            GameEventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Subscribe<EnemyLootRolledEvent>(OnEnemyLootRolled);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            GameEventBus.Unsubscribe<EnemySeenEvent>(OnEnemySeen);
            GameEventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Unsubscribe<EnemyLootRolledEvent>(OnEnemyLootRolled);
        }

        public BestiaryEntry GetEntry(string enemyId)
        {
            return !string.IsNullOrWhiteSpace(enemyId) && _entries.TryGetValue(enemyId, out var entry)
                ? entry
                : null;
        }

        public IReadOnlyCollection<BestiaryEntry> GetAllEntries()
        {
            return _entries.Values;
        }

        public BestiarySaveData CaptureSaveData()
        {
            var data = new BestiarySaveData();
            foreach (var entry in _entries.Values)
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.EnemyId))
                {
                    data.Entries.Add(new BestiaryEntrySaveData(entry));
                }
            }

            return data;
        }

        public void RestoreFromSaveData(BestiarySaveData saveData)
        {
            _entries.Clear();
            if (saveData?.Entries == null)
            {
                return;
            }

            foreach (var entryData in saveData.Entries)
            {
                if (entryData == null || string.IsNullOrWhiteSpace(entryData.EnemyId))
                {
                    continue;
                }

                var entry = entryData.ToEntry();
                _entries[entry.EnemyId] = entry;
            }
        }

        public bool RegisterEnemySeen(string enemyId, int caveLevel = 0)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return false;
            }

            var entry = GetOrCreateEntry(enemyId);
            bool changed = false;

            if (!entry.FirstSeen)
            {
                entry.FirstSeen = true;
                changed = true;
            }

            if (caveLevel > 0 && entry.LastSeenCaveLevel != caveLevel)
            {
                entry.LastSeenCaveLevel = caveLevel;
                changed = true;
            }

            if (changed)
            {
                PublishUpdated(entry, "FirstSeen");
            }

            return changed;
        }

        private void OnEnemySpawned(EnemySpawnedEvent evt)
        {
            RegisterEnemySeen(evt?.EnemyId, evt?.CaveLevel ?? 0);
        }

        private void OnEnemySeen(EnemySeenEvent evt)
        {
            RegisterEnemySeen(evt?.EnemyId, evt?.CaveLevel ?? 0);
        }

        private void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            if (evt == null || string.IsNullOrWhiteSpace(evt.EnemyId) || string.IsNullOrWhiteSpace(evt.DamageType))
            {
                return;
            }

            RegisterEnemySeen(evt.EnemyId);
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            var result = evt?.DamageResult;
            if (result == null || string.IsNullOrWhiteSpace(result.TargetId))
            {
                return;
            }

            var entry = GetOrCreateEntry(result.TargetId);
            bool changed = false;

            if (!entry.FirstSeen)
            {
                entry.FirstSeen = true;
                changed = true;
            }

            var damageTypeId = result.DamageType.ToString();
            if (result.WasImmune || result.CombatResistanceMultiplier < 1f)
            {
                changed |= AddUnique(entry.ResistancesDiscovered, damageTypeId);
            }
            else if (result.CombatResistanceMultiplier > 1f)
            {
                changed |= AddUnique(entry.WeaknessesDiscovered, damageTypeId);
            }

            if (result.WasVulnerable && !entry.VulnerabilityWindowDiscovered)
            {
                entry.VulnerabilityWindowDiscovered = true;
                changed = true;
            }

            if (changed)
            {
                PublishUpdated(entry, "DamageDiscovery");
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (string.IsNullOrWhiteSpace(evt.EnemyId))
            {
                return;
            }

            var entry = GetOrCreateEntry(evt.EnemyId);
            entry.FirstSeen = true;
            entry.KillCount++;

            if (!string.IsNullOrWhiteSpace(evt.DropItemId) && evt.DropAmount > 0)
            {
                AddUnique(entry.DropsDiscovered, evt.DropItemId);
            }

            PublishUpdated(entry, "KillCount");
        }

        private void OnEnemyLootRolled(EnemyLootRolledEvent evt)
        {
            if (evt == null || string.IsNullOrWhiteSpace(evt.EnemyId) || string.IsNullOrWhiteSpace(evt.ItemId) || evt.Amount <= 0)
            {
                return;
            }

            var entry = GetOrCreateEntry(evt.EnemyId);
            entry.FirstSeen = true;
            if (AddUnique(entry.DropsDiscovered, evt.ItemId))
            {
                PublishUpdated(entry, "DropDiscovered");
            }
        }

        private BestiaryEntry GetOrCreateEntry(string enemyId)
        {
            if (!_entries.TryGetValue(enemyId, out var entry))
            {
                entry = new BestiaryEntry { EnemyId = enemyId };
                _entries[enemyId] = entry;
            }

            return entry;
        }

        private static bool AddUnique(List<string> values, string value)
        {
            if (values == null || string.IsNullOrWhiteSpace(value) || values.Contains(value))
            {
                return false;
            }

            values.Add(value);
            return true;
        }

        private static void PublishUpdated(BestiaryEntry entry, string updateType)
        {
            GameEventBus.Publish(new BestiaryEntryUpdatedEvent(
                entry.EnemyId,
                updateType,
                $"bestiary_{entry.EnemyId.Replace("enemy_", string.Empty)}"));
        }
    }

    public class BestiaryEntry
    {
        public string EnemyId;
        public bool FirstSeen;
        public int KillCount;
        public List<string> DropsDiscovered = new List<string>();
        public List<string> WeaknessesDiscovered = new List<string>();
        public List<string> ResistancesDiscovered = new List<string>();
        public bool VulnerabilityWindowDiscovered;
        public int LastSeenCaveLevel;
    }
}
