using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Combat.StatusEffect;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class PersistentZoneSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly SkillActionSO _defaultAction;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.WorldPointInFrontOfPlayer;

        public PersistentZoneSkillEffectExecutor(string effectId, string displayName, SkillActionSO defaultAction)
        {
            _effectId = effectId;
            _displayName = displayName;
            _defaultAction = defaultAction;
        }

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            var action = MagicSkillRuntimeUtility.Action(context, _defaultAction);
            var result = MagicSkillRuntimeUtility.Validate(context, action, true);
            if (!result.Success) return result;
            Vector2 origin = MagicSkillRuntimeUtility.Origin(context);
            Vector2 aim = MagicSkillRuntimeUtility.AimPoint(context, action.TargetingRange);
            return MagicSkillRuntimeUtility.HasLineOfEffect(origin, aim, context.Caster, context.Target)
                ? result : SkillEffectResult.Failed("BlockedLineOfEffect", "O ponto de mira está bloqueado.");
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var action = MagicSkillRuntimeUtility.Action(context, _defaultAction);
            var readiness = Validate(context);
            if (!readiness.Success) return readiness;
            var rank = action.ResolveRank(context.Rank);
            if (!MagicSkillRuntimeUtility.TryBeginCast(context, action, out var cast))
                return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({Mathf.RoundToInt(rank.ManaCost)}).");

            var zoneObject = new GameObject($"SkillZone_{context.SkillActionId}");
            zoneObject.transform.position = MagicSkillRuntimeUtility.AimPoint(context, action.TargetingRange);
            var zone = zoneObject.AddComponent<PersistentDamageZone>();
            zone.Configure(context.SkillActionId, context.Rank, rank.Damage, action.DamageType,
                rank.Range, action.ResolveEffectDuration(context.Rank), action.PulseCount,
                MagicSkillRuntimeUtility.ResolveStatus(action.StatusEffectId), Time.time, cast);
            MagicSkillRuntimeUtility.Commit(cast);
            return SkillEffectResult.Succeeded($"{_displayName}!", cast.Preparation.ManaCost > 0, true, rank.CooldownSeconds);
        }
    }

    [DisallowMultipleComponent]
    public sealed class PersistentDamageZone : MonoBehaviour
    {
        private string _actionId;
        private int _rank;
        private int _totalDamage;
        private DamageType _damageType;
        private float _radius;
        private float _duration;
        private int _pulseCount;
        private StatusEffectSO _status;
        private float _startedAt;
        private float _nextPulseAt;
        private int _pulsesApplied;
        private readonly HashSet<int> _poisonedTargets = new HashSet<int>();
        private readonly Dictionary<int, int> _ticksByTarget = new Dictionary<int, int>();
        private MagicSkillRuntimeUtility.ActiveMagicCast _cast;

        public string ActionId => _actionId;
        public int Rank => _rank;
        public int PulsesApplied => _pulsesApplied;

        public void Configure(string actionId, int rank, int totalDamage, DamageType damageType,
            float radius, float duration, int pulseCount, StatusEffectSO status, float now)
            => ConfigureCore(actionId, rank, totalDamage, damageType, radius, duration,
                pulseCount, status, now, null);

        internal void Configure(string actionId, int rank, int totalDamage, DamageType damageType,
            float radius, float duration, int pulseCount, StatusEffectSO status, float now,
            MagicSkillRuntimeUtility.ActiveMagicCast cast)
            => ConfigureCore(actionId, rank, totalDamage, damageType, radius, duration,
                pulseCount, status, now, cast);

        private void ConfigureCore(string actionId, int rank, int totalDamage,
            DamageType damageType, float radius, float duration, int pulseCount,
            StatusEffectSO status, float now, MagicSkillRuntimeUtility.ActiveMagicCast cast)
        {
            _actionId = actionId ?? string.Empty;
            _rank = Mathf.Max(1, rank);
            _totalDamage = Mathf.Max(0, totalDamage);
            _damageType = damageType;
            _radius = Mathf.Max(0f, radius);
            _duration = Mathf.Max(0f, duration);
            _pulseCount = Mathf.Max(1, pulseCount);
            _status = status;
            _cast = cast;
            _startedAt = now;
            _nextPulseAt = now;
            Advance(now);
        }

        private void Update() => Advance(Time.time);

        public void Advance(float now)
        {
            while (_pulsesApplied < _pulseCount && now >= _nextPulseAt)
            {
                ApplyPulse(_pulsesApplied);
                _pulsesApplied++;
                _nextPulseAt = _startedAt + _duration * _pulsesApplied / _pulseCount;
            }
            if (_pulsesApplied >= _pulseCount && now >= _startedAt + _duration)
                Destroy(gameObject);
        }

        public static int ResolvePulseDamage(int totalDamage, int pulseCount, int pulseIndex)
        {
            int safePulses = Mathf.Max(1, pulseCount);
            int safeTotal = Mathf.Max(0, totalDamage);
            int basePulse = safeTotal / safePulses;
            return pulseIndex >= safePulses - 1
                ? basePulse + safeTotal % safePulses
                : basePulse;
        }

        private void ApplyPulse(int pulseIndex)
        {
            Vector2 center = transform.position;
            float radiusSquared = _radius * _radius;
            var targets = MagicSkillRuntimeUtility.OrderedEnemies(center, enemy =>
                ((Vector2)enemy.transform.position - center).sqrMagnitude <= radiusSquared);
            int pulseDamage = ResolvePulseDamage(_totalDamage, _pulseCount, pulseIndex);
            foreach (var target in targets)
            {
                int targetId = target.GetEntityId().GetHashCode();
                _ticksByTarget.TryGetValue(targetId, out int ticks);
                if (ticks >= _pulseCount) continue;
                _ticksByTarget[targetId] = ticks + 1;
                MagicSkillRuntimeUtility.Damage(target, pulseDamage, _damageType, center,
                    _cast, isPrimaryDamage: false, canTriggerCapstones: false,
                    canTriggerStatusEffects: false, canTriggerReactions: false);
                if (_status != null && _poisonedTargets.Add(targetId))
                    MagicSkillRuntimeUtility.RefreshStatus(target, _status, 0f);
            }
        }
    }
}
