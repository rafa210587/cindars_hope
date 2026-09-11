using System;
using System.Collections.Generic;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>
    /// Pure authority for Kanthor/Kaand's refresh-only window and Kanthor's single heal charge.
    /// </summary>
    public sealed class CombatCapstoneState
    {
        public const string KanthorVariant = "kanthor";
        public const string KaandVariant = "kaand";
        public const float WindowSeconds = 8f;
        private const int ResolutionLedgerCapacity = 64;

        private readonly HashSet<string> _processedResolutionIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Queue<string> _resolutionOrder = new Queue<string>();
        public MagicConfluenceState MagicConfluence { get; } = new MagicConfluenceState();

        public string ActiveVariant { get; private set; } = string.Empty;
        public int ActiveRank { get; private set; }
        public float RemainingSeconds { get; private set; }
        public bool HealChargeArmed { get; private set; }
        public string LastResolutionId { get; private set; } = string.Empty;
        public bool IsActive => RemainingSeconds > 0f && ActiveRank > 0 && IsKnownVariant(ActiveVariant);

        public bool TryActivate(string variant, int rank, string resolutionId)
        {
            if (!IsKnownVariant(variant) || rank <= 0)
                return false;

            string normalizedResolution = resolutionId ?? string.Empty;
            if (!string.IsNullOrEmpty(normalizedResolution) &&
                _processedResolutionIds.Contains(normalizedResolution))
                return false;

            RememberResolution(normalizedResolution);

            ActiveVariant = variant;
            ActiveRank = Math.Max(1, Math.Min(3, rank));
            RemainingSeconds = WindowSeconds;
            HealChargeArmed = string.Equals(variant, KanthorVariant, StringComparison.Ordinal);
            LastResolutionId = normalizedResolution;
            return true;
        }

        public bool Advance(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || RemainingSeconds <= 0f)
                return false;

            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaSeconds);
            if (RemainingSeconds > 0f)
                return false;

            ActiveVariant = string.Empty;
            ActiveRank = 0;
            HealChargeArmed = false;
            return true;
        }

        public bool TryConsumeKanthorHeal(out int rank)
        {
            rank = ActiveRank;
            if (!IsActive || !HealChargeArmed ||
                !string.Equals(ActiveVariant, KanthorVariant, StringComparison.Ordinal))
                return false;

            HealChargeArmed = false;
            return true;
        }

        public bool ClearTransient()
        {
            bool changed = IsActive || HealChargeArmed;
            ActiveVariant = string.Empty;
            ActiveRank = 0;
            RemainingSeconds = 0f;
            HealChargeArmed = false;
            return changed;
        }

        public CombatCapstoneSaveData CaptureSaveData()
        {
            var data = new CombatCapstoneSaveData
            {
                ActiveVariant = ActiveVariant,
                ActiveRank = ActiveRank,
                RemainingSeconds = RemainingSeconds,
                HealChargeArmed = HealChargeArmed,
                LastResolutionId = LastResolutionId
            };
            data.ProcessedResolutionIds.AddRange(_resolutionOrder);
            MagicConfluence.CaptureInto(data);
            return data;
        }

        public void RestoreFromSaveData(CombatCapstoneSaveData data)
        {
            ClearTransient();
            _processedResolutionIds.Clear();
            _resolutionOrder.Clear();
            LastResolutionId = data?.LastResolutionId ?? string.Empty;
            if (data?.ProcessedResolutionIds != null)
            {
                foreach (string resolutionId in data.ProcessedResolutionIds)
                    RememberResolution(resolutionId);
            }
            RememberResolution(LastResolutionId);
            MagicConfluence.RestoreFrom(data);
            if (data == null || !IsKnownVariant(data.ActiveVariant) || data.ActiveRank <= 0 ||
                data.RemainingSeconds <= 0f)
                return;

            ActiveVariant = data.ActiveVariant;
            ActiveRank = Math.Max(1, Math.Min(3, data.ActiveRank));
            RemainingSeconds = Math.Max(0f, Math.Min(WindowSeconds, data.RemainingSeconds));
            HealChargeArmed = string.Equals(ActiveVariant, KanthorVariant, StringComparison.Ordinal) &&
                data.HealChargeArmed;
        }

        private static bool IsKnownVariant(string variant)
            => string.Equals(variant, KanthorVariant, StringComparison.Ordinal) ||
               string.Equals(variant, KaandVariant, StringComparison.Ordinal);

        private void RememberResolution(string resolutionId)
        {
            if (string.IsNullOrEmpty(resolutionId) || !_processedResolutionIds.Add(resolutionId))
                return;

            _resolutionOrder.Enqueue(resolutionId);
            while (_resolutionOrder.Count > ResolutionLedgerCapacity)
                _processedResolutionIds.Remove(_resolutionOrder.Dequeue());
        }
    }
}
