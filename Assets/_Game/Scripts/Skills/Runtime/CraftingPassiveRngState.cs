using System;
using System.Collections.Generic;
using CindarsHope.Core.Random;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime
{
    [Serializable]
    public sealed class CraftingPassiveRngSaveData
    {
        public string SaveScopeId = string.Empty;
        public List<CraftingPassiveAttemptSaveEntry> Attempts = new List<CraftingPassiveAttemptSaveEntry>();
    }

    [Serializable]
    public sealed class CraftingPassiveAttemptSaveEntry
    {
        public string OperationKind = string.Empty;
        public string EligibleInstanceId = string.Empty;
        public int NextAttempt;
    }

    public readonly struct CraftingPassiveRoll
    {
        internal CraftingPassiveRoll(
            string saveScopeId,
            string operationKind,
            string eligibleInstanceId,
            int attempt,
            int seed,
            float value,
            bool succeeded)
        {
            SaveScopeId = saveScopeId;
            OperationKind = operationKind;
            EligibleInstanceId = eligibleInstanceId;
            Attempt = attempt;
            Seed = seed;
            Value = value;
            Succeeded = succeeded;
        }

        public string SaveScopeId { get; }
        public string OperationKind { get; }
        public string EligibleInstanceId { get; }
        public int Attempt { get; }
        public int Seed { get; }
        public float Value { get; }
        public bool Succeeded { get; }
    }

    /// <summary>
    /// Estado puro dos rolls persistentes usados pelas passivas de crafting/economia.
    /// Preparar um roll não muda estado; o consumer avança a tentativa somente no commit real.
    /// </summary>
    public sealed class CraftingPassiveRngState
    {
        public const string DefaultSaveScopeId = "save_slot_1";

        private readonly string _defaultSaveScopeId;
        private readonly Dictionary<AttemptKey, int> _nextAttempts =
            new Dictionary<AttemptKey, int>();
        private string _saveScopeId;

        public CraftingPassiveRngState(string saveScopeId = DefaultSaveScopeId)
        {
            _defaultSaveScopeId = NormalizeScope(saveScopeId, DefaultSaveScopeId);
            _saveScopeId = _defaultSaveScopeId;
        }

        public string SaveScopeId => _saveScopeId;

        public bool TryPrepareRoll(
            string operationKind,
            string eligibleInstanceId,
            float chance01,
            out CraftingPassiveRoll roll)
        {
            roll = default;
            if (string.IsNullOrWhiteSpace(operationKind) || string.IsNullOrWhiteSpace(eligibleInstanceId))
            {
                return false;
            }

            var key = new AttemptKey(operationKind, eligibleInstanceId);
            var attempt = GetNextAttempt(key);
            var seedKey = BuildSeedKey(_saveScopeId, operationKind, eligibleInstanceId, attempt);
            var seed = StableHash32.Compute(seedKey);
            var value = new SeededGameplayRandomSource(seed).NextFloat();
            var chance = Math.Max(0f, Math.Min(1f, chance01));

            roll = new CraftingPassiveRoll(
                _saveScopeId,
                operationKind,
                eligibleInstanceId,
                attempt,
                seed,
                value,
                value < chance);
            return true;
        }

        public bool Commit(CraftingPassiveRoll roll)
        {
            if (!string.Equals(roll.SaveScopeId, _saveScopeId, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(roll.OperationKind) ||
                string.IsNullOrWhiteSpace(roll.EligibleInstanceId))
            {
                return false;
            }

            var key = new AttemptKey(roll.OperationKind, roll.EligibleInstanceId);
            if (GetNextAttempt(key) != roll.Attempt)
            {
                return false;
            }

            _nextAttempts[key] = checked(roll.Attempt + 1);
            return true;
        }

        public int GetNextAttempt(string operationKind, string eligibleInstanceId)
        {
            if (string.IsNullOrWhiteSpace(operationKind) || string.IsNullOrWhiteSpace(eligibleInstanceId))
            {
                return 0;
            }

            return GetNextAttempt(new AttemptKey(operationKind, eligibleInstanceId));
        }

        public CraftingPassiveRngSaveData CaptureSaveData()
        {
            var data = new CraftingPassiveRngSaveData { SaveScopeId = _saveScopeId };
            var entries = new List<KeyValuePair<AttemptKey, int>>(_nextAttempts);
            entries.Sort((left, right) => left.Key.CompareTo(right.Key));

            foreach (var entry in entries)
            {
                data.Attempts.Add(new CraftingPassiveAttemptSaveEntry
                {
                    OperationKind = entry.Key.OperationKind,
                    EligibleInstanceId = entry.Key.EligibleInstanceId,
                    NextAttempt = entry.Value
                });
            }

            return data;
        }

        public void RestoreFromSaveData(CraftingPassiveRngSaveData data)
        {
            _nextAttempts.Clear();
            _saveScopeId = NormalizeScope(data?.SaveScopeId, _defaultSaveScopeId);
            if (data?.Attempts == null)
            {
                return;
            }

            foreach (var entry in data.Attempts)
            {
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.OperationKind) ||
                    string.IsNullOrWhiteSpace(entry.EligibleInstanceId) ||
                    entry.NextAttempt < 0)
                {
                    continue;
                }

                var key = new AttemptKey(entry.OperationKind, entry.EligibleInstanceId);
                if (!_nextAttempts.TryGetValue(key, out var current) || entry.NextAttempt > current)
                {
                    _nextAttempts[key] = entry.NextAttempt;
                }
            }
        }

        private int GetNextAttempt(AttemptKey key) =>
            _nextAttempts.TryGetValue(key, out var attempt) ? attempt : 0;

        private static string NormalizeScope(string value, string fallback) =>
            string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

        private static string BuildSeedKey(
            string saveScopeId,
            string operationKind,
            string eligibleInstanceId,
            int attempt)
        {
            return $"{saveScopeId.Length}:{saveScopeId}|{operationKind.Length}:{operationKind}|" +
                   $"{eligibleInstanceId.Length}:{eligibleInstanceId}|{attempt}";
        }

        private readonly struct AttemptKey : IEquatable<AttemptKey>, IComparable<AttemptKey>
        {
            public AttemptKey(string operationKind, string eligibleInstanceId)
            {
                OperationKind = operationKind;
                EligibleInstanceId = eligibleInstanceId;
            }

            public string OperationKind { get; }
            public string EligibleInstanceId { get; }

            public bool Equals(AttemptKey other) =>
                string.Equals(OperationKind, other.OperationKind, StringComparison.Ordinal) &&
                string.Equals(EligibleInstanceId, other.EligibleInstanceId, StringComparison.Ordinal);

            public override bool Equals(object obj) => obj is AttemptKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((OperationKind != null ? StringComparer.Ordinal.GetHashCode(OperationKind) : 0) * 397) ^
                           (EligibleInstanceId != null ? StringComparer.Ordinal.GetHashCode(EligibleInstanceId) : 0);
                }
            }

            public int CompareTo(AttemptKey other)
            {
                var operationComparison = string.CompareOrdinal(OperationKind, other.OperationKind);
                return operationComparison != 0
                    ? operationComparison
                    : string.CompareOrdinal(EligibleInstanceId, other.EligibleInstanceId);
            }
        }
    }
}
