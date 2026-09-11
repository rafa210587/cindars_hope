using System.Collections.Generic;
using System.Threading;
using CindarsHope.Combat;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class SlowFieldSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly SkillActionSO _defaultAction;
        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public SlowFieldSkillEffectExecutor(string effectId, string displayName, SkillActionSO defaultAction)
        { _effectId = effectId; _displayName = displayName; _defaultAction = defaultAction; }

        public SkillEffectResult Validate(SkillEffectContext context) => MagicSkillRuntimeUtility.Validate(
            context, MagicSkillRuntimeUtility.Action(context, _defaultAction), false);

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var action = MagicSkillRuntimeUtility.Action(context, _defaultAction);
            var readiness = Validate(context);
            if (!readiness.Success) return readiness;
            var rank = action.ResolveRank(context.Rank);
            if (!MagicSkillRuntimeUtility.TryBeginCast(context, action, out var cast))
                return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({Mathf.RoundToInt(rank.ManaCost)}).");
            var fieldObject = new GameObject($"SkillSlowField_{context.SkillActionId}");
            fieldObject.transform.position = MagicSkillRuntimeUtility.Origin(context);
            fieldObject.AddComponent<MagicSlowField>().Configure(context.SkillActionId, rank.Range,
                action.ResolveEffectDuration(context.Rank), action.ResolveControlStrength(context.Rank),
                action.StatusEffectId, Time.time);
            MagicSkillRuntimeUtility.Commit(cast);
            return SkillEffectResult.Succeeded($"{_displayName}!", cast.Preparation.ManaCost > 0, true, rank.CooldownSeconds);
        }
    }

    [DisallowMultipleComponent]
    public sealed class MagicSlowField : MonoBehaviour
    {
        private static int s_sequence;
        private readonly HashSet<int> _enteredTargets = new HashSet<int>();
        private string _sourceId;
        private float _radius;
        private float _reductionFraction;
        private string _statusEffectId;
        private float _expiresAt;
        private float _nextScanAt;
        private const float ScanIntervalSeconds = .10f;

        public void Configure(string actionId, float radius, float duration, float reductionFraction,
            string statusEffectId, float now)
        {
            _sourceId = $"{actionId}:{Interlocked.Increment(ref s_sequence)}";
            _radius = Mathf.Max(0f, radius);
            _reductionFraction = Mathf.Clamp01(reductionFraction);
            _statusEffectId = statusEffectId ?? string.Empty;
            _expiresAt = now + Mathf.Max(0f, duration);
            _nextScanAt = now + ScanIntervalSeconds;
            ApplyNewEntries(now);
        }

        private void Update()
        {
            if (Time.time >= _expiresAt) { Destroy(gameObject); return; }
            if (Time.time < _nextScanAt) return;
            _nextScanAt = Time.time + ScanIntervalSeconds;
            ApplyNewEntries(Time.time);
        }

        private void ApplyNewEntries(float now)
        {
            Vector2 center = transform.position;
            float radiusSquared = _radius * _radius;
            var targets = MagicSkillRuntimeUtility.OrderedEnemies(center, enemy =>
                ((Vector2)enemy.transform.position - center).sqrMagnitude <= radiusSquared);
            foreach (var target in targets)
            {
                int id = target.GetEntityId().GetHashCode();
                if (_enteredTargets.Contains(id)) continue;
                float remaining = Mathf.Max(0f, _expiresAt - now);
                if (!MagicControlRules.TryResolveField(target.gameObject, remaining, _reductionFraction,
                        _statusEffectId, now, out float duration, out float reduction, out bool bossBound)) continue;
                _enteredTargets.Add(id);
                MagicSlowState.GetOrCreate(target.gameObject).Apply(_sourceId, reduction, duration, bossBound, now);
            }
        }
    }

    [DisallowMultipleComponent]
    public sealed class MagicSlowState : MonoBehaviour, IStatusMovementOverrideRuntime
    {
        private sealed class Lease
        {
            public string SourceId;
            public string StatusId;
            public float Reduction;
            public float ExpiresAt;
            public bool BossWindowBound;
        }
        private readonly List<Lease> _leases = new List<Lease>();
        private IEnemyBrainController _brain;
        private IEnemyVulnerabilityWindow _window;
        public float CurrentReduction { get; private set; }

        public static MagicSlowState GetOrCreate(GameObject target)
            => target.GetComponent<MagicSlowState>() ?? target.AddComponent<MagicSlowState>();

        public void Apply(string sourceId, float reduction, float duration, bool bossWindowBound, float now,
            string statusId = null)
        {
            if (duration <= 0f || reduction <= 0f) return;
            for (int i = _leases.Count - 1; i >= 0; i--)
                if (_leases[i].SourceId == sourceId) _leases.RemoveAt(i);
            _leases.Add(new Lease { SourceId = sourceId, Reduction = Mathf.Clamp01(reduction),
                ExpiresAt = now + duration, BossWindowBound = bossWindowBound,
                StatusId = statusId ?? string.Empty });
            Refresh(now);
        }

        public bool HasActiveStatus(string statusId, float now)
        {
            if (_leases.Count == 0) return false;
            Refresh(now);
            return !string.IsNullOrWhiteSpace(statusId)
                && _leases.Exists(item => item.StatusId == statusId);
        }

        public bool SuppressesStatusMovement(string statusId, string sourceId)
            => !string.IsNullOrWhiteSpace(statusId)
                && _leases.Exists(item => item.StatusId == statusId
                    && item.SourceId == (sourceId ?? string.Empty));

        private void Update() => Refresh(Time.time);
        private void Refresh(float now)
        {
            if (_brain == null) _brain = GetComponent<IEnemyBrainController>();
            if (_window == null) _window = GetComponent<IEnemyVulnerabilityWindow>();
            for (int i = _leases.Count - 1; i >= 0; i--)
            {
                var item = _leases[i];
                if (now < item.ExpiresAt
                    && (!item.BossWindowBound || (_window != null && _window.IsVulnerable))) continue;
                if (!string.IsNullOrWhiteSpace(item.StatusId))
                    GetComponent<EnemyHealth>()?.StatusEffects.RemoveStatusEffect(
                        item.StatusId, item.SourceId);
                _leases.RemoveAt(i);
            }
            float strongest = 0f;
            foreach (var lease in _leases) strongest = Mathf.Max(strongest, lease.Reduction);
            CurrentReduction = strongest;
            if (_brain != null) _brain.ApplyExternalBehaviorOverride(
                1f - strongest, .12f, false, 0f, false, 0f);
            if (_leases.Count == 0 && Application.isPlaying) Destroy(this);
        }
    }

    public static class MagicControlRules
    {
        public static bool TryResolveLightDuration(GameObject target, float baseDuration, string statusId,
            out float duration, out bool bossWindowBound)
        {
            var difficulty = ResolveDifficulty(target);
            bossWindowBound = difficulty == EnemyDifficulty.Boss;
            if (bossWindowBound && !HasBossWindow(target)) { duration = 0f; return false; }
            float statusMultiplier = ResolveStatusMultiplier(target, statusId);
            if (statusMultiplier <= 0f) { duration = 0f; return false; }
            float resistedDuration = baseDuration * statusMultiplier;
            duration = bossWindowBound ? resistedDuration * EnemySkillReactionAdapter.BossDurationMultiplier
                : IsElite(difficulty, target)
                    ? Mathf.Min(resistedDuration * EnemySkillReactionAdapter.EliteDurationMultiplier, 2.4f)
                    : resistedDuration;
            return duration > 0f;
        }

        public static bool TryResolveStrongSlow(GameObject target, float baseDuration, string statusId, float now,
            out float duration, out bool bossWindowBound)
        {
            var difficulty = ResolveDifficulty(target);
            bossWindowBound = difficulty == EnemyDifficulty.Boss;
            var adapter = EnemySkillReactionAdapter.GetOrCreate(target);
            if (adapter == null || (bossWindowBound && !adapter.CanReceiveBossControl))
            { duration = 0f; return false; }
            float statusMultiplier = ResolveStatusMultiplier(target, statusId);
            if (statusMultiplier <= 0f) { duration = 0f; return false; }
            float resistance = bossWindowBound ? EnemySkillReactionAdapter.BossDurationMultiplier
                : IsElite(difficulty, target) ? EnemySkillReactionAdapter.EliteDurationMultiplier : 1f;
            var result = adapter.ControlDr.TryApply(baseDuration * statusMultiplier * resistance, now);
            duration = result.CanApply ? result.EffectiveDurationSeconds : 0f;
            return result.CanApply;
        }

        public static bool TryResolveField(GameObject target, float remainingDuration,
            float authoredReduction, string statusId, float now, out float duration, out float reduction,
            out bool bossWindowBound)
        {
            var difficulty = ResolveDifficulty(target);
            bossWindowBound = difficulty == EnemyDifficulty.Boss;
            var adapter = EnemySkillReactionAdapter.GetOrCreate(target);
            if (adapter == null || (bossWindowBound && !adapter.CanReceiveBossControl))
            { duration = 0f; reduction = 0f; return false; }
            float statusMultiplier = ResolveStatusMultiplier(target, statusId);
            if (statusMultiplier <= 0f) { duration = 0f; reduction = 0f; return false; }
            float resistance = bossWindowBound ? EnemySkillReactionAdapter.BossDurationMultiplier
                : IsElite(difficulty, target) ? EnemySkillReactionAdapter.EliteDurationMultiplier : 1f;
            var result = adapter.ControlDr.TryApply(remainingDuration * statusMultiplier * resistance, now);
            duration = result.EffectiveDurationSeconds;
            if (IsElite(difficulty, target)) duration = Mathf.Min(duration, 3.2f);
            reduction = bossWindowBound ? .10f : Mathf.Clamp01(authoredReduction);
            return result.CanApply && duration > 0f;
        }

        private static EnemyDifficulty ResolveDifficulty(GameObject target)
        {
            var posture = target != null ? target.GetComponent<EnemyPostureState>() : null;
            if (posture != null) return posture.Difficulty;
            var reaction = target != null ? target.GetComponent<IEnemySkillReactionRuntime>() : null;
            return reaction != null ? reaction.Difficulty : EnemyDifficulty.Normal;
        }

        private static bool IsElite(EnemyDifficulty difficulty, GameObject target)
        {
            var reaction = target != null ? target.GetComponent<IEnemySkillReactionRuntime>() : null;
            return difficulty == EnemyDifficulty.Elite || difficulty == EnemyDifficulty.MiniBoss
                || (reaction != null && reaction.HasEliteClassification);
        }

        private static bool HasBossWindow(GameObject target)
        {
            var window = target != null ? target.GetComponent<IEnemyVulnerabilityWindow>() : null;
            return window != null && window.IsVulnerable;
        }

        private static float ResolveStatusMultiplier(GameObject target, string statusId)
        {
            var health = target != null ? target.GetComponent<EnemyHealth>() : null;
            return health != null ? health.ResolveStatusDurationMultiplier(statusId) : 1f;
        }
    }
}
