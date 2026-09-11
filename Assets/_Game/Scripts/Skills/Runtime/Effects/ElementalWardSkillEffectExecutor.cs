using CindarsHope.Foundation;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class ElementalWardSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly SkillActionSO _defaultAction;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.Self;

        public ElementalWardSkillEffectExecutor(string effectId, string displayName, SkillActionSO defaultAction)
        {
            _effectId = effectId;
            _displayName = displayName;
            _defaultAction = defaultAction;
        }

        public SkillEffectResult Validate(SkillEffectContext context)
            => MagicSkillRuntimeUtility.Validate(context, MagicSkillRuntimeUtility.Action(context, _defaultAction), false);

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var action = MagicSkillRuntimeUtility.Action(context, _defaultAction);
            var readiness = MagicSkillRuntimeUtility.Validate(context, action, false);
            if (!readiness.Success) return readiness;
            var rank = action.ResolveRank(context.Rank);
            if (!MagicSkillRuntimeUtility.TryBeginCast(context, action, out var cast))
                return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({Mathf.RoundToInt(rank.ManaCost)}).");
            ElementalWardState.Cast(action.ResolveControlStrength(context.Rank),
                action.EffectHitCharges, action.ResolveEffectDuration(context.Rank), Time.time);
            if (context.Caster.GetComponent<ElementalWardLifecycle>() == null)
                context.Caster.AddComponent<ElementalWardLifecycle>();
            MagicSkillRuntimeUtility.Commit(cast);
            return SkillEffectResult.Succeeded($"{_displayName}!", cast.Preparation.ManaCost > 0, true, rank.CooldownSeconds);
        }
    }

    [DisallowMultipleComponent]
    internal sealed class ElementalWardLifecycle : MonoBehaviour
    {
        private void OnEnable() => GameEventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
        private void Update() => ElementalWardState.ExpireIfNeeded(Time.time);
        private void OnDisable()
        {
            GameEventBus.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
            ElementalWardState.Clear();
        }
        private static void HandlePlayerDied(PlayerDiedEvent evt) => ElementalWardState.Clear();
    }

    public sealed class ElementalWardState
    {
        private static readonly System.Func<int, DamageType, float, bool, int> s_damageModifier
            = ResolveIncomingDamage;

        public static ElementalWardState Active { get; private set; }

        public float ReductionFraction { get; }
        public int RemainingHits { get; private set; }
        public float ExpiresAt { get; }

        private ElementalWardState(float reductionFraction, int hitCharges, float expiresAt)
        {
            ReductionFraction = Mathf.Clamp01(reductionFraction);
            RemainingHits = Mathf.Max(0, hitCharges);
            ExpiresAt = expiresAt;
        }

        public static ElementalWardState Cast(float reductionFraction, int hitCharges,
            float durationSeconds, float now)
        {
            Active = reductionFraction > 0f && hitCharges > 0 && durationSeconds > 0f
                ? new ElementalWardState(reductionFraction, hitCharges, now + durationSeconds)
                : null;
            IncomingDamageModifierProvider.Source = Active != null ? s_damageModifier : null;
            return Active;
        }

        public static int ResolveIncomingDamage(int rawDamage, DamageType damageType, float now,
            bool isDamageOverTime = false)
        {
            var ward = Active;
            if (ward == null || rawDamage <= 0 || isDamageOverTime) return rawDamage;
            if (now >= ward.ExpiresAt || ward.RemainingHits <= 0)
            {
                Clear();
                return rawDamage;
            }
            if (!IsElemental(damageType)) return rawDamage;
            ward.RemainingHits--;
            int result = Mathf.Max(0, Mathf.CeilToInt(rawDamage * (1f - ward.ReductionFraction)));
            if (ward.RemainingHits <= 0) Clear();
            return result;
        }

        public static bool IsElemental(DamageType damageType)
            => damageType == DamageType.Fire || damageType == DamageType.Ice
                || damageType == DamageType.Toxic || damageType == DamageType.Lightning;

        public static void ExpireIfNeeded(float now)
        {
            if (Active != null && now >= Active.ExpiresAt) Clear();
        }

        public static void Clear()
        {
            Active = null;
            if (IncomingDamageModifierProvider.Source == s_damageModifier)
                IncomingDamageModifierProvider.Source = null;
        }
    }
}
