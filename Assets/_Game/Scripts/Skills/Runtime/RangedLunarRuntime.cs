using System;
using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime
{
    public readonly struct RangedHostileSnapshot
    {
        public string EnemyInstanceId { get; }
        public float PositionX { get; }
        public float PositionY { get; }
        public bool IsAlive { get; }

        public RangedHostileSnapshot(string enemyInstanceId, float positionX,
            float positionY, bool isAlive = true)
        {
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            PositionX = positionX;
            PositionY = positionY;
            IsAlive = isAlive;
        }
    }

    public static class RangedLunarRules
    {
        public const string AlihanaVariant = "alihana";
        public const string SenyaVariant = "senya";
        public const string NyxVariant = "nyx";
        public const float MaxFocusDurationSeconds = 10f;
        public const float IsolationRadiusTiles = 2.5f;
        public const float BaseRangedCriticalChance = 0.05f;
        public const float BaseRangedCriticalDamageMultiplier = 1.5f;
        public const float AlihanaRankThreeProjectileSpeedBonus = .15f;
        public const float NyxRankThreeCriticalDamageBonus = .15f;

        private static readonly float[] FocusDurations = { 6f, 8f, 10f };
        private static readonly float[] AlihanaRangeBonuses = { .08f, .12f, .16f };
        private static readonly float[] SenyaSecondaryFractions = { .08f, .12f, .16f };
        private static readonly float[] NyxCriticalChanceBonuses = { .08f, .12f, .15f };

        public static float FocusDuration(int rank) => FocusDurations[RankIndex(rank)];

        public static bool IsKnownVariant(string variant)
            => string.Equals(variant, AlihanaVariant, StringComparison.Ordinal) ||
               string.Equals(variant, SenyaVariant, StringComparison.Ordinal) ||
               string.Equals(variant, NyxVariant, StringComparison.Ordinal);

        public static RangedLunarLaunchModifier AlihanaLaunch(int rank)
            => new RangedLunarLaunchModifier(
                1f + AlihanaRangeBonuses[RankIndex(rank)],
                rank >= 3 ? 1f + AlihanaRankThreeProjectileSpeedBonus : 1f);

        public static float SenyaSecondaryFraction(int rank)
            => SenyaSecondaryFractions[RankIndex(rank)];

        public static RangedLunarImpactModifier NyxImpact(int rank, bool isolated)
            => isolated
                ? new RangedLunarImpactModifier(
                    NyxCriticalChanceBonuses[RankIndex(rank)],
                    rank >= 3 ? NyxRankThreeCriticalDamageBonus : 0f)
                : RangedLunarImpactModifier.Neutral;

        public static bool IsIsolated(string targetInstanceId, float targetX, float targetY,
            IReadOnlyList<RangedHostileSnapshot> hostiles)
        {
            if (string.IsNullOrWhiteSpace(targetInstanceId) || hostiles == null)
                return false;

            float radiusSquared = IsolationRadiusTiles * IsolationRadiusTiles;
            bool targetAlive = false;
            for (int i = 0; i < hostiles.Count; i++)
            {
                RangedHostileSnapshot hostile = hostiles[i];
                if (!hostile.IsAlive) continue;
                if (string.Equals(hostile.EnemyInstanceId, targetInstanceId,
                        StringComparison.Ordinal))
                {
                    targetAlive = true;
                    continue;
                }

                float deltaX = hostile.PositionX - targetX;
                float deltaY = hostile.PositionY - targetY;
                if (deltaX * deltaX + deltaY * deltaY <= radiusSquared)
                    return false;
            }

            return targetAlive;
        }

        public static int ApplyCritical(int nonCriticalDamage, bool guaranteedCritical,
            float criticalRoll, RangedLunarImpactModifier modifier, out bool isCritical)
        {
            float chance = Math.Min(1f,
                BaseRangedCriticalChance + modifier.CriticalChanceBonus);
            isCritical = guaranteedCritical || criticalRoll < chance;
            if (!isCritical) return Math.Max(1, nonCriticalDamage);
            double multiplier = BaseRangedCriticalDamageMultiplier + modifier.CriticalDamageBonus;
            return Math.Max(1, (int)Math.Round(
                nonCriticalDamage * multiplier, MidpointRounding.AwayFromZero));
        }

        public static int SenyaSecondaryDamage(int primaryFinalDamage, int rank)
            => primaryFinalDamage <= 0
                ? 0
                : Math.Max(1, (int)Math.Ceiling(
                    primaryFinalDamage * SenyaSecondaryFraction(rank)));

        private static int RankIndex(int rank) => Math.Max(0, Math.Min(2, rank - 1));
    }

    public sealed class RangedLunarRuntime : IDisposable, IRangedLunarModifierSource
    {
        private const string SenyaSecondarySourceId = "ranged_lunar_senya";
        public const string NodeId = "ranged_capstone_eagle_focus";
        public const string AlihanaVariant = RangedLunarRules.AlihanaVariant;
        public const string SenyaVariant = RangedLunarRules.SenyaVariant;
        public const string NyxVariant = RangedLunarRules.NyxVariant;

        private readonly SurvivalSkillState _state;
        private readonly Func<int> _rankSource;
        private readonly Func<string> _variantSource;
        private readonly Func<IReadOnlyList<RangedHostileSnapshot>> _hostileSnapshots;
        private readonly Action<string, DamageRequest> _applySecondaryDamage;
        private bool _subscribed;

        public RangedLunarRuntime(SurvivalSkillState state, Func<int> rankSource,
            Func<string> variantSource,
            Func<IReadOnlyList<RangedHostileSnapshot>> hostileSnapshots = null,
            Action<string, DamageRequest> applySecondaryDamage = null)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _rankSource = rankSource ?? (() => 0);
            _variantSource = variantSource ?? (() => string.Empty);
            _hostileSnapshots = hostileSnapshots ?? CaptureLiveEncounterHostiles;
            _applySecondaryDamage = applySecondaryDamage ?? ApplySecondaryDamage;
        }

        public void Enable()
        {
            if (_subscribed) return;
            GameEventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Subscribe<PlayerMagicCastCommittedEvent>(OnMagicCastCommitted);
            GameEventBus.Subscribe<SkillTreeRespecCompletedEvent>(OnRespec);
            _subscribed = true;
        }

        public void Disable()
        {
            if (!_subscribed) return;
            GameEventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            GameEventBus.Unsubscribe<PlayerMagicCastCommittedEvent>(OnMagicCastCommitted);
            GameEventBus.Unsubscribe<SkillTreeRespecCompletedEvent>(OnRespec);
            _subscribed = false;
        }

        public void Tick(float deltaSeconds)
        {
            _state.AdvanceLunarFocusTime(deltaSeconds, out _);
        }

        public bool TryActivateFirstTarget(string targetInstanceId, string actionToken)
        {
            int rank = Math.Max(0, Math.Min(3, _rankSource()));
            string variant = _variantSource() ?? string.Empty;
            if (rank <= 0 || !RangedLunarRules.IsKnownVariant(variant)) return false;
            return _state.TryActivateLunarFocus(targetInstanceId, variant, rank,
                RangedLunarRules.FocusDuration(rank), actionToken);
        }

        public RangedLunarLaunchModifier ResolveLaunchModifier()
        {
            if (!IsLiveFocus(AlihanaVariant)) return RangedLunarLaunchModifier.Neutral;
            return RangedLunarRules.AlihanaLaunch(_state.ActiveLunarRank);
        }

        public RangedLunarImpactModifier ResolveImpactModifier(
            string targetInstanceId, float targetPositionX, float targetPositionY)
        {
            if (!IsLiveFocus(NyxVariant) || !string.Equals(
                    _state.ActiveLunarTargetInstanceId, targetInstanceId,
                    StringComparison.Ordinal))
                return RangedLunarImpactModifier.Neutral;

            bool isolated = RangedLunarRules.IsIsolated(targetInstanceId,
                targetPositionX, targetPositionY, _hostileSnapshots());
            return RangedLunarRules.NyxImpact(_state.ActiveLunarRank, isolated);
        }

        public void Dispose() => Disable();

        private bool IsLiveFocus(string variant)
        {
            if (!_state.HasActiveLunarFocus || !string.Equals(
                    _state.ActiveLunarVariant, variant, StringComparison.Ordinal))
                return false;

            var hostiles = _hostileSnapshots();
            if (hostiles == null) return false;
            for (int i = 0; i < hostiles.Count; i++)
                if (hostiles[i].IsAlive && string.Equals(hostiles[i].EnemyInstanceId,
                        _state.ActiveLunarTargetInstanceId, StringComparison.Ordinal))
                    return true;
            return false;
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            DamageResult result = evt?.DamageResult;
            if (result == null || result.FinalDamage <= 0 || !result.IsPrimaryDamage ||
                !result.CanTriggerCapstones)
                return;

            if (result.SourceKind != DamageSourceKind.PlayerRanged ||
                !IsLiveFocus(SenyaVariant) || !string.Equals(
                    result.TargetInstanceId, _state.ActiveLunarTargetInstanceId,
                    StringComparison.Ordinal) ||
                _state.LastOffensiveMagicDamageType == DamageType.Physical)
                return;

            int damage = RangedLunarRules.SenyaSecondaryDamage(
                result.FinalDamage, _state.ActiveLunarRank);
            if (damage <= 0) return;

            _applySecondaryDamage(result.TargetInstanceId, new DamageRequest(
                result.TargetId, damage, _state.LastOffensiveMagicDamageType,
                sourceId: SenyaSecondarySourceId)
            {
                SourceKind = DamageSourceKind.CapstoneSecondary,
                SourceInstanceId = result.SourceInstanceId,
                TargetInstanceId = result.TargetInstanceId,
                ActionToken = string.IsNullOrWhiteSpace(result.ActionToken)
                    ? _state.ActiveLunarActionToken + ":senya"
                    : result.ActionToken + ":senya",
                IsCritical = false,
                IsPrimaryDamage = false,
                CanTriggerCapstones = false,
                CanTriggerStatusEffects = false,
                CanTriggerReactions = false,
                CanTriggerVulnerability = false,
                KnockbackForce = 0f
            });
        }

        private void OnMagicCastCommitted(PlayerMagicCastCommittedEvent evt)
        {
            if (evt == null || evt.Discipline != SpellDiscipline.Offensive ||
                !Enum.TryParse(evt.DamageTypeId, out DamageType damageType) ||
                damageType == DamageType.Physical)
                return;
            _state.RecordOffensiveMagicDamageType(damageType);
        }

        private void OnRespec(SkillTreeRespecCompletedEvent evt)
        {
            _state.ClearActiveLunarFocus();
        }

        private IReadOnlyList<RangedHostileSnapshot> CaptureLiveEncounterHostiles()
        {
            var result = new List<RangedHostileSnapshot>();
            var active = EnemyHealth.ActiveInstances;
            for (int i = 0; i < active.Count; i++)
            {
                EnemyHealth enemy = active[i];
                if (enemy == null || !_state.IsActiveEncounterEnemy(enemy.EnemyInstanceId))
                    continue;
                Vector2 position = enemy.transform.position;
                result.Add(new RangedHostileSnapshot(enemy.EnemyInstanceId,
                    position.x, position.y, !enemy.IsDead && enemy.gameObject.activeInHierarchy));
            }
            result.Sort((left, right) => string.CompareOrdinal(
                left.EnemyInstanceId, right.EnemyInstanceId));
            return result;
        }

        private static void ApplySecondaryDamage(string targetInstanceId, DamageRequest request)
        {
            var active = EnemyHealth.ActiveInstances;
            for (int i = 0; i < active.Count; i++)
            {
                EnemyHealth enemy = active[i];
                if (enemy == null || enemy.IsDead || !string.Equals(
                        enemy.EnemyInstanceId, targetInstanceId, StringComparison.Ordinal))
                    continue;
                enemy.TakeDamage(request);
                return;
            }
        }

    }
}
