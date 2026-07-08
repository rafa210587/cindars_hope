using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.Bestiary;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Enemy
{
    [DisallowMultipleComponent]
    public class BestiaryManager : MonoBehaviour
    {
        // arch: Core|Enemy (spec_arch_core_enemy_cycle_reduction_v31) — self-registro estatico,
        // molde Audio/AudioManager.cs; GameBootstrap nao segura mais [SerializeField] deste manager.
        private static BestiaryManager _instance;
        public static BestiaryManager Instance => _instance;

        private readonly Dictionary<string, BestiaryEntry> _entries = new Dictionary<string, BestiaryEntry>();

        // fable_21 — discovery-knowledge layer hosted here (no new manager on the bootstrap).
        private readonly EnemyKnowledgeService _knowledge = new EnemyKnowledgeService();

        // SpoilerTier / boss lookups built once from the canonical catalog (read-only consumption;
        // the bestiary REVEALS catalog data, never authors it).
        private static Dictionary<string, int> _spoilerTierById;
        private static HashSet<string> _bossIds;

        public int EntryCount => _entries.Count;

        /// <summary>fable_21 — the discovery-knowledge service hosted by this manager (F14/F22/F25 consume it).</summary>
        public EnemyKnowledgeService Knowledge => _knowledge;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            WireKnowledgeSources();
        }

        private void OnEnable()
        {
            GameEventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            GameEventBus.Subscribe<EnemySeenEvent>(OnEnemySeen);
            GameEventBus.Subscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Subscribe<EnemyLootRolledEvent>(OnEnemyLootRolled);
            GameEventBus.Subscribe<EnemyActionStartedEvent>(OnEnemyActionStarted);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            GameEventBus.Unsubscribe<EnemySeenEvent>(OnEnemySeen);
            GameEventBus.Unsubscribe<EnemyDamagedEvent>(OnEnemyDamaged);
            GameEventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
            GameEventBus.Unsubscribe<EnemyLootRolledEvent>(OnEnemyLootRolled);
            GameEventBus.Unsubscribe<EnemyActionStartedEvent>(OnEnemyActionStarted);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void WireKnowledgeSources()
        {
            EnsureCatalogLookup();
            _knowledge.SpoilerTierSource = id =>
                id != null && _spoilerTierById.TryGetValue(id, out var tier) ? tier : 0;
            _knowledge.IsBossSource = id => id != null && _bossIds.Contains(id);
            // SkillPointGrantSink / QuestFlagSource are wired by higher-level bootstrap when available;
            // the milestone event (BestiaryMilestoneReachedEvent) is the canonical bus path regardless.
        }

        private static void EnsureCatalogLookup()
        {
            if (_spoilerTierById != null && _bossIds != null)
            {
                return;
            }

            _spoilerTierById = new Dictionary<string, int>();
            _bossIds = new HashSet<string>();
            foreach (var def in CanonicalBestiaryCatalog.All)
            {
                if (string.IsNullOrWhiteSpace(def.EnemyId))
                {
                    continue;
                }

                _spoilerTierById[def.EnemyId] = def.SpoilerTier;
                if (def.IsBoss)
                {
                    _bossIds.Add(def.EnemyId);
                }
            }
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

            // fable_21 — persist the discovery-knowledge layer in the same section.
            data.Knowledge = _knowledge.CaptureKnowledge();
            data.KnowledgeMilestonesGranted = _knowledge.CaptureMilestonesGranted();

            return data;
        }

        public void RestoreFromSaveData(BestiarySaveData saveData)
        {
            _entries.Clear();

            // fable_21 — legacy saves (no Knowledge field) restore to an empty codex with no error.
            _knowledge.RestoreKnowledge(saveData?.Knowledge, saveData?.KnowledgeMilestonesGranted ?? 0);

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
            _knowledge.RecordSighting(evt?.EnemyId);
        }

        private void OnEnemySeen(EnemySeenEvent evt)
        {
            RegisterEnemySeen(evt?.EnemyId, evt?.CaveLevel ?? 0);
            _knowledge.RecordSighting(evt?.EnemyId);
        }

        private void OnEnemyActionStarted(EnemyActionStartedEvent evt)
        {
            if (evt == null || string.IsNullOrWhiteSpace(evt.EnemyId) || string.IsNullOrWhiteSpace(evt.ActionId))
            {
                return;
            }

            // The action being telegraphed is observed (behavior threshold: same action 3x).
            _knowledge.RecordActionSeen(evt.EnemyId, evt.ActionId);
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
            bool resisted = result.WasImmune || result.CombatResistanceMultiplier < 1f;
            bool effective = result.WasVulnerable || result.CombatResistanceMultiplier > 1f;

            if (resisted)
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

            // fable_21 — feed the discovery-knowledge counters (axis = canonical damage-type string).
            var axis = ElementAxis(result.DamageType);
            if (resisted)
            {
                _knowledge.RecordResistedHit(result.TargetId);
            }
            else if (effective)
            {
                _knowledge.RecordEffectiveHit(result.TargetId, axis);
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

            _knowledge.RecordKill(evt.EnemyId);

            PublishUpdated(entry, "KillCount");
        }

        private static string ElementAxis(DamageType damageType)
        {
            // Stable lowercase axis name (matches catalog PrimaryDamageTypeId convention).
            return damageType.ToString().ToLowerInvariant();
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
