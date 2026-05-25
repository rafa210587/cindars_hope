using CindarsHope.Core;
using CindarsHope.Core.Events;
using System.Collections.Generic;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [DisallowMultipleComponent]
    public class BestiaryManager : MonoBehaviour
    {
        [SerializeField] private BestiaryDataSO _bestiaryData;

        private Dictionary<string, BestiaryEntry> _entries = new Dictionary<string, BestiaryEntry>();

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemySeenEvent>(OnEnemySeen);
            GameEventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemySeenEvent>(OnEnemySeen);
            GameEventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        }

        private void OnEnemySeen(EnemySeenEvent evt)
        {
            if (!_entries.ContainsKey(evt.EnemyId))
            {
                _entries[evt.EnemyId] = new BestiaryEntry
                {
                    EnemyId = evt.EnemyId,
                    FirstSeen = Time.time,
                    KillCount = 0
                };
                GameEventBus.Publish(new BestiaryEntryUpdatedEvent(evt.EnemyId, "FirstSeen"));
            }
        }

        private void OnEnemyDamaged(EnemyDamagedEvent evt)
        {
            if (_entries.ContainsKey(evt.EnemyId))
            {
                if (evt.DamageType == "weak")
                {
                    _entries[evt.EnemyId].WeaknessesDiscovered.Add(evt.DamageType);
                    GameEventBus.Publish(new BestiaryEntryUpdatedEvent(evt.EnemyId, "WeaknessDiscovered"));
                }
                else if (evt.DamageType == "resistant")
                {
                    _entries[evt.EnemyId].ResistancesDiscovered.Add(evt.DamageType);
                    GameEventBus.Publish(new BestiaryEntryUpdatedEvent(evt.EnemyId, "ResistanceDiscovered"));
                }
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (_entries.ContainsKey(evt.EnemyId))
            {
                _entries[evt.EnemyId].KillCount++;
                GameEventBus.Publish(new BestiaryEntryUpdatedEvent(evt.EnemyId, "KillCount"));
            }
        }

        public BestiaryEntry GetEntry(string enemyId)
        {
            return _entries.ContainsKey(enemyId) ? _entries[enemyId] : null;
        }

        public List<BestiaryEntry> GetAllEntries()
        {
            return new List<BestiaryEntry>(_entries.Values);
        }

        public void SaveBestiary()
        {
            if (_bestiaryData != null)
            {
                _bestiaryData.SaveEntries(new List<BestiaryEntry>(_entries.Values));
            }
        }

        public void LoadBestiary()
        {
            if (_bestiaryData != null)
            {
                var loaded = _bestiaryData.LoadEntries();
                _entries.Clear();
                foreach (var entry in loaded)
                {
                    _entries[entry.EnemyId] = entry;
                }
            }
        }
    }

    public class BestiaryEntry
    {
        public string EnemyId;
        public float FirstSeen;
        public int KillCount;
        public List<string> DropsDiscovered = new List<string>();
        public List<string> WeaknessesDiscovered = new List<string>();
        public List<string> ResistancesDiscovered = new List<string>();
        public bool VulnerabilityWindowDiscovered;
        public int LastSeenCaveLevel;
    }
}
