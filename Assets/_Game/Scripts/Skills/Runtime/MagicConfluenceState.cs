using System;
using System.Collections.Generic;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime
{
    /// <summary>Pure deterministic authority for the magic capstone spend window and seed.</summary>
    public sealed class MagicConfluenceState
    {
        public const string AnyaVariant = "anya";
        public const string SenyaVariant = "senya";
        public const float LedgerWindowSeconds = 6f;
        public const float LockoutSeconds = 25f;
        public const float ThresholdFraction = .35f;
        public const float AnyaArmDurationSeconds = 10f;
        public const float SenyaArmDurationSeconds = 8f;

        private static readonly float[] AnyaManaDiscountFractions = { .30f, .40f, .50f };
        private static readonly float[] AnyaSpiritualOutputBonusFractions = { .20f, .28f, .35f };
        private static readonly float[] AnyaStaminaEchoDurations = { 1f, 2f, 4f };
        private static readonly int[] SenyaManaSurchargePercents = { 5, 8, 10 };
        private static readonly float[] SenyaDirectDamageBonusFractions = { .20f, .28f, .35f };
        private static readonly float[] SenyaCriticalChanceBonusFractions = { .08f, .12f, .15f };

        private readonly List<int> _ledgerAmounts = new List<int>();
        private readonly List<float> _ledgerAges = new List<float>();
        private string _reservedActionToken = string.Empty;

        public string ArmedVariant { get; private set; } = string.Empty;
        public int ArmedRank { get; private set; }
        public float ArmedRemainingSeconds { get; private set; }
        public float LockoutRemainingSeconds { get; private set; }
        public int MaxManaSnapshot { get; private set; }
        public int LedgerManaSpent { get; private set; }
        public int EchoTotal { get; private set; }
        public int EchoDelivered { get; private set; }
        public float EchoDurationSeconds { get; private set; }
        public float EchoElapsedSeconds { get; private set; }
        public bool IsArmed => ArmedRank > 0 && ArmedRemainingSeconds > 0f;
        public bool HasReservation => !string.IsNullOrEmpty(_reservedActionToken);

        public SpellCastPreparation Prepare(SpellCastPreparationRequest request,
            string selectedVariant, int selectedRank)
        {
            int selected = ClampRank(selectedRank);
            if (!IsArmed || HasReservation || selected <= 0 ||
                string.IsNullOrWhiteSpace(request.ActionToken) ||
                !string.Equals(ArmedVariant, selectedVariant, StringComparison.Ordinal) ||
                !MatchesDiscipline(ArmedVariant, request.Discipline))
                return new SpellCastPreparation(request, request.BaseManaCost);

            int rank = ArmedRank;

            _reservedActionToken = request.ActionToken;
            if (string.Equals(ArmedVariant, AnyaVariant, StringComparison.Ordinal))
            {
                float discount = RankValue(rank, AnyaManaDiscountFractions);
                int finalCost = request.BaseManaCost <= 0 ? 0 :
                    (int)Math.Ceiling(request.BaseManaCost * (1f - discount));
                float outputBonus = RankValue(rank, AnyaSpiritualOutputBonusFractions);
                int echo = (int)Math.Floor(finalCost * outputBonus);
                float echoDuration = RankValue(rank, AnyaStaminaEchoDurations);
                return new SpellCastPreparation(request, finalCost, 1f, 0f,
                    1f + outputBonus, echo, echoDuration, true);
            }

            int surchargePercent = RankValue(rank, SenyaManaSurchargePercents);
            int surchargeMana = request.BaseManaCost <= 0 ? 0 :
                (request.BaseManaCost * surchargePercent + 99) / 100;
            float damageBonus = RankValue(rank, SenyaDirectDamageBonusFractions);
            float critBonus = RankValue(rank, SenyaCriticalChanceBonusFractions);
            return new SpellCastPreparation(request, request.BaseManaCost + surchargeMana,
                1f + damageBonus, critBonus, 1f, 0, 0f, true);
        }

        /// <summary>Commits a successful resolution. Returns true when this cast arms a seed.</summary>
        public bool Commit(SpellCastPreparation preparation, int maxManaAtCommit,
            string selectedVariant, int selectedRank, out bool consumed)
        {
            consumed = false;
            if (preparation.ReservesConfluence &&
                string.Equals(_reservedActionToken, preparation.Request.ActionToken,
                    StringComparison.Ordinal))
            {
                _reservedActionToken = string.Empty;
                ClearArm();
                LockoutRemainingSeconds = LockoutSeconds;
                consumed = true;
                return false;
            }

            if (LockoutRemainingSeconds > 0f || IsArmed || HasReservation ||
                preparation.Request.Discipline == SpellDiscipline.None ||
                preparation.ManaCost <= 0 || ClampRank(selectedRank) <= 0 ||
                !IsKnownVariant(selectedVariant))
                return false;

            if (_ledgerAmounts.Count == 0)
                MaxManaSnapshot = Math.Max(0, maxManaAtCommit);
            if (MaxManaSnapshot <= 0)
                return false;

            _ledgerAmounts.Add(preparation.ManaCost);
            _ledgerAges.Add(0f);
            LedgerManaSpent += preparation.ManaCost;
            int threshold = (int)Math.Ceiling(MaxManaSnapshot * ThresholdFraction);
            if (LedgerManaSpent < threshold)
                return false;

            ArmedVariant = selectedVariant;
            ArmedRank = ClampRank(selectedRank);
            ArmedRemainingSeconds = string.Equals(selectedVariant, AnyaVariant,
                StringComparison.Ordinal) ? AnyaArmDurationSeconds : SenyaArmDurationSeconds;
            ClearLedger();
            return true;
        }

        public void Cancel(SpellCastPreparation preparation)
        {
            if (preparation.ReservesConfluence &&
                string.Equals(_reservedActionToken, preparation.Request.ActionToken,
                    StringComparison.Ordinal))
                _reservedActionToken = string.Empty;
        }

        public void StartStaminaEcho(int total, float durationSeconds)
        {
            EchoTotal = Math.Max(0, total);
            EchoDelivered = 0;
            EchoDurationSeconds = Math.Max(0f, durationSeconds);
            EchoElapsedSeconds = 0f;
        }

        public int AdvanceStaminaEcho(float deltaSeconds)
        {
            if (deltaSeconds <= 0f || EchoDelivered >= EchoTotal ||
                EchoDurationSeconds <= 0f)
                return 0;
            EchoElapsedSeconds = Math.Min(EchoDurationSeconds,
                EchoElapsedSeconds + deltaSeconds);
            int targetDelivered = (int)Math.Floor(EchoTotal *
                (EchoElapsedSeconds / EchoDurationSeconds));
            int amount = Math.Max(0, targetDelivered - EchoDelivered);
            EchoDelivered += amount;
            return amount;
        }

        public void Advance(float deltaSeconds)
        {
            if (deltaSeconds <= 0f) return;
            LockoutRemainingSeconds = Math.Max(0f, LockoutRemainingSeconds - deltaSeconds);
            if (ArmedRemainingSeconds > 0f)
            {
                ArmedRemainingSeconds = Math.Max(0f, ArmedRemainingSeconds - deltaSeconds);
                if (ArmedRemainingSeconds <= 0f) ClearArm();
            }
            for (int i = _ledgerAges.Count - 1; i >= 0; i--)
            {
                _ledgerAges[i] += deltaSeconds;
                if (_ledgerAges[i] <= LedgerWindowSeconds) continue;
                LedgerManaSpent -= _ledgerAmounts[i];
                _ledgerAmounts.RemoveAt(i);
                _ledgerAges.RemoveAt(i);
            }
            if (_ledgerAmounts.Count == 0)
            {
                LedgerManaSpent = 0;
                MaxManaSnapshot = 0;
            }
        }

        public void ClearForRespec()
        {
            ClearArm();
            ClearLedger();
            _reservedActionToken = string.Empty;
        }

        internal void CaptureInto(CombatCapstoneSaveData data)
        {
            data.MagicArmedVariant = ArmedVariant;
            data.MagicArmedRank = ArmedRank;
            data.MagicArmedRemainingSeconds = ArmedRemainingSeconds;
            data.MagicLockoutRemainingSeconds = LockoutRemainingSeconds;
            data.MagicMaxManaSnapshot = MaxManaSnapshot;
            data.MagicLedgerAmounts.AddRange(_ledgerAmounts);
            data.MagicLedgerAges.AddRange(_ledgerAges);
            data.MagicEchoTotal = EchoTotal;
            data.MagicEchoDelivered = EchoDelivered;
            data.MagicEchoDurationSeconds = EchoDurationSeconds;
            data.MagicEchoElapsedSeconds = EchoElapsedSeconds;
        }

        internal void RestoreFrom(CombatCapstoneSaveData data)
        {
            ClearForRespec();
            EchoTotal = 0;
            EchoDelivered = 0;
            EchoDurationSeconds = 0f;
            EchoElapsedSeconds = 0f;
            LockoutRemainingSeconds = Math.Max(0f, Math.Min(LockoutSeconds,
                data?.MagicLockoutRemainingSeconds ?? 0f));
            if (data == null) return;
            if (IsKnownVariant(data.MagicArmedVariant) && data.MagicArmedRank > 0 &&
                data.MagicArmedRemainingSeconds > 0f)
            {
                ArmedVariant = data.MagicArmedVariant;
                ArmedRank = ClampRank(data.MagicArmedRank);
                float maxDuration = ArmedVariant == AnyaVariant
                    ? AnyaArmDurationSeconds : SenyaArmDurationSeconds;
                ArmedRemainingSeconds = Math.Min(maxDuration, data.MagicArmedRemainingSeconds);
            }
            int count = Math.Min(data.MagicLedgerAmounts?.Count ?? 0,
                data.MagicLedgerAges?.Count ?? 0);
            MaxManaSnapshot = Math.Max(0, data.MagicMaxManaSnapshot);
            for (int i = 0; i < count; i++)
            {
                int amount = Math.Max(0, data.MagicLedgerAmounts[i]);
                float age = Math.Max(0f, data.MagicLedgerAges[i]);
                if (amount <= 0 || age > LedgerWindowSeconds) continue;
                _ledgerAmounts.Add(amount);
                _ledgerAges.Add(age);
                LedgerManaSpent += amount;
            }
            if (_ledgerAmounts.Count == 0) MaxManaSnapshot = 0;
            EchoTotal = Math.Max(0, data.MagicEchoTotal);
            EchoDelivered = Math.Max(0, Math.Min(EchoTotal, data.MagicEchoDelivered));
            EchoDurationSeconds = Math.Max(0f, data.MagicEchoDurationSeconds);
            EchoElapsedSeconds = Math.Max(0f, Math.Min(EchoDurationSeconds,
                data.MagicEchoElapsedSeconds));
        }

        private void ClearArm()
        {
            ArmedVariant = string.Empty;
            ArmedRank = 0;
            ArmedRemainingSeconds = 0f;
        }

        private void ClearLedger()
        {
            _ledgerAmounts.Clear();
            _ledgerAges.Clear();
            LedgerManaSpent = 0;
            MaxManaSnapshot = 0;
        }

        private static bool MatchesDiscipline(string variant, SpellDiscipline discipline)
            => (variant == AnyaVariant && discipline == SpellDiscipline.Spiritual) ||
               (variant == SenyaVariant && discipline == SpellDiscipline.Offensive);

        private static bool IsKnownVariant(string variant)
            => variant == AnyaVariant || variant == SenyaVariant;
        private static int ClampRank(int rank) => Math.Max(0, Math.Min(3, rank));
        private static float RankValue(int rank, float[] values)
            => values[Math.Max(0, Math.Min(values.Length - 1, rank - 1))];

        private static int RankValue(int rank, int[] values)
            => values[Math.Max(0, Math.Min(values.Length - 1, rank - 1))];
    }
}
