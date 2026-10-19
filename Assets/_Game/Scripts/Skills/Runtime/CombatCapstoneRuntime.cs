using System;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;

namespace CindarsHope.Skills.Runtime
{
    public readonly struct CombatCapstoneActivatedEvent
    {
        public string Variant { get; }
        public int Rank { get; }
        public float DurationSeconds { get; }

        public CombatCapstoneActivatedEvent(string variant, int rank, float durationSeconds)
        {
            Variant = variant ?? string.Empty;
            Rank = rank;
            DurationSeconds = durationSeconds;
        }
    }

    public readonly struct CombatCapstoneExpiredEvent
    {
        public string Variant { get; }
        public CombatCapstoneExpiredEvent(string variant) => Variant = variant ?? string.Empty;
    }

    public readonly struct CombatCapstoneRewardEvent
    {
        public int HealthRestored { get; }
        public int StaminaRestored { get; }

        public CombatCapstoneRewardEvent(int healthRestored, int staminaRestored)
        {
            HealthRestored = Math.Max(0, healthRestored);
            StaminaRestored = Math.Max(0, staminaRestored);
        }
    }

    /// <summary>
    /// Event adapter for melee capstones. The state owns rules; this class owns subscriptions and
    /// resource side effects after a confirmed direct melee hit.
    /// </summary>
    public sealed class CombatCapstoneRuntime : IDisposable,
        ICombatCapstoneModifierSource, IPlayerControlResistanceSource
    {
        public const string NodeId = "melee_capstone_battle_rhythm";
        private const int KanthorRankThreeFullHealthStaminaReward = 10;
        private const float KanthorRankOneHealFraction = 0.02f;
        private const float KanthorRankTwoAndThreeHealFraction = 0.03f;

        private static readonly float[] KanthorDamage = { 0.10f, 0.12f, 0.15f };
        private static readonly float[] KanthorKnockback = { 0.15f, 0.20f, 0.25f };
        private static readonly float[] KanthorStun = { 0.12f, 0.16f, 0.20f };
        private static readonly float[] KaandDamage = { 0.18f, 0.24f, 0.30f };
        private static readonly float[] KaandCriticalDamage = { 0.15f, 0.20f, 0.25f };

        private readonly CombatCapstoneState _state;
        private readonly Func<int> _rankSource;
        private readonly Func<string> _variantSource;
        private readonly Func<int> _currentHealthSource;
        private readonly Func<int> _maxHealthSource;
        private readonly Action<int> _restoreHealth;
        private readonly Action<int> _restoreStamina;
        private bool _subscribed;

        public CombatCapstoneRuntime(
            CombatCapstoneState state,
            Func<int> rankSource,
            Func<string> variantSource,
            Func<int> currentHealthSource,
            Func<int> maxHealthSource,
            Action<int> restoreHealth,
            Action<int> restoreStamina)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _rankSource = rankSource ?? (() => 0);
            _variantSource = variantSource ?? (() => string.Empty);
            _currentHealthSource = currentHealthSource ?? (() => 0);
            _maxHealthSource = maxHealthSource ?? (() => 1);
            _restoreHealth = restoreHealth ?? (_ => { });
            _restoreStamina = restoreStamina ?? (_ => { });
        }

        public CombatCapstoneState State => _state;

        public float DirectMeleeDamageMultiplier
        {
            get
            {
                if (!_state.IsActive) return 1f;
                int index = RankIndex(_state.ActiveRank);
                if (string.Equals(_state.ActiveVariant, CombatCapstoneState.KanthorVariant, StringComparison.Ordinal))
                    return 1f + KanthorDamage[index];
                if (string.Equals(_state.ActiveVariant, CombatCapstoneState.KaandVariant, StringComparison.Ordinal))
                    return 1f + KaandDamage[index];
                return 1f;
            }
        }

        public float MeleeCriticalDamageBonus
            => _state.IsActive && string.Equals(_state.ActiveVariant, CombatCapstoneState.KaandVariant,
                   StringComparison.Ordinal)
                ? KaandCriticalDamage[RankIndex(_state.ActiveRank)]
                : 0f;

        public float KnockbackReductionFraction
            => _state.IsActive && string.Equals(_state.ActiveVariant, CombatCapstoneState.KanthorVariant,
                   StringComparison.Ordinal)
                ? KanthorKnockback[RankIndex(_state.ActiveRank)]
                : 0f;

        public float StunDurationReductionFraction
            => _state.IsActive && string.Equals(_state.ActiveVariant, CombatCapstoneState.KanthorVariant,
                   StringComparison.Ordinal)
                ? KanthorStun[RankIndex(_state.ActiveRank)]
                : 0f;

        public void Enable()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<PlayerPerfectBlockEvent>(OnPerfectBlock);
            GameEventBus.Subscribe<EnemyPostureBrokenEvent>(OnPostureBroken);
            GameEventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Subscribe<SkillTreeRespecCompletedEvent>(OnRespec);
            _subscribed = true;
        }

        public void Disable()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<PlayerPerfectBlockEvent>(OnPerfectBlock);
            GameEventBus.Unsubscribe<EnemyPostureBrokenEvent>(OnPostureBroken);
            GameEventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Unsubscribe<SkillTreeRespecCompletedEvent>(OnRespec);
            _subscribed = false;
        }

        public void Tick(float deltaSeconds)
        {
            string chosenVariant = _variantSource();
            if (_rankSource() <= 0 || string.IsNullOrWhiteSpace(chosenVariant) ||
                (_state.IsActive && !string.Equals(
                    chosenVariant, _state.ActiveVariant, StringComparison.Ordinal)))
            {
                ExpireTransient();
                return;
            }

            string previousVariant = _state.ActiveVariant;
            if (_state.Advance(deltaSeconds))
                GameEventBus.Publish(new CombatCapstoneExpiredEvent(previousVariant));
        }

        public void Dispose() => Disable();

        private void OnPerfectBlock(PlayerPerfectBlockEvent evt)
        {
            if (string.Equals(_variantSource(), CombatCapstoneState.KanthorVariant, StringComparison.Ordinal))
                TryActivate(CombatCapstoneState.KanthorVariant, evt.ResolutionId);
        }

        private void OnPostureBroken(EnemyPostureBrokenEvent evt)
        {
            if (!evt.CausedByPlayer) return;
            string variant = _variantSource();
            if (string.Equals(variant, CombatCapstoneState.KanthorVariant, StringComparison.Ordinal) ||
                string.Equals(variant, CombatCapstoneState.KaandVariant, StringComparison.Ordinal))
                TryActivate(variant, evt.ResolutionId);
        }

        private void TryActivate(string variant, string resolutionId)
        {
            int rank = Math.Max(0, Math.Min(3, _rankSource()));
            if (_state.TryActivate(variant, rank, resolutionId))
                GameEventBus.Publish(new CombatCapstoneActivatedEvent(variant, rank,
                    CombatCapstoneState.WindowSeconds));
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            DamageResult result = evt?.DamageResult;
            if (result == null || result.FinalDamage <= 0 ||
                result.SourceKind != DamageSourceKind.PlayerMelee ||
                !result.IsPrimaryDamage || !result.CanTriggerCapstones)
                return;

            if (!_state.TryConsumeKanthorHeal(out int rank)) return;

            int currentHealth = Math.Max(0, _currentHealthSource());
            int maxHealth = Math.Max(1, _maxHealthSource());
            if (rank >= 3 && currentHealth >= maxHealth)
            {
                _restoreStamina(KanthorRankThreeFullHealthStaminaReward);
                GameEventBus.Publish(new CombatCapstoneRewardEvent(
                    0, KanthorRankThreeFullHealthStaminaReward));
                return;
            }

            float healFraction = rank <= 1
                ? KanthorRankOneHealFraction
                : KanthorRankTwoAndThreeHealFraction;
            int requested = Math.Max(1, (int)Math.Ceiling(maxHealth * healFraction));
            int restored = Math.Max(0, Math.Min(requested, maxHealth - currentHealth));
            if (restored > 0) _restoreHealth(restored);
            GameEventBus.Publish(new CombatCapstoneRewardEvent(restored, 0));
        }

        private void OnRespec(SkillTreeRespecCompletedEvent evt) => ExpireTransient();

        private void ExpireTransient()
        {
            string previousVariant = _state.ActiveVariant;
            if (_state.ClearTransient())
                GameEventBus.Publish(new CombatCapstoneExpiredEvent(previousVariant));
        }

        private static int RankIndex(int rank) => Math.Max(0, Math.Min(2, rank - 1));
    }
}
