using System.Collections.Generic;
using CindarsHope.Combat;
using CindarsHope.Foundation;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class ChainSkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private readonly string _effectId;
        private readonly string _displayName;
        private readonly SkillActionSO _defaultAction;

        public string EffectId => _effectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public ChainSkillEffectExecutor(string effectId, string displayName, SkillActionSO defaultAction)
        {
            _effectId = effectId;
            _displayName = displayName;
            _defaultAction = defaultAction;
        }

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            var action = MagicSkillRuntimeUtility.Action(context, _defaultAction);
            var result = MagicSkillRuntimeUtility.Validate(context, action, false);
            if (!result.Success) return result;
            Vector2 origin = MagicSkillRuntimeUtility.Origin(context);
            float range = action.ResolveRank(context.Rank).Range;
            return FindNext(origin, range, null, context.Caster) != null
                ? result
                : SkillEffectResult.Failed("NoTarget", "Nenhum alvo disponível para a corrente.");
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var action = MagicSkillRuntimeUtility.Action(context, _defaultAction);
            var readiness = Validate(context);
            if (!readiness.Success) return readiness;
            var rank = action.ResolveRank(context.Rank);
            if (!MagicSkillRuntimeUtility.TryBeginCast(context, action, out var cast))
                return SkillEffectResult.Failed("InsufficientMana", $"Mana insuficiente ({Mathf.RoundToInt(rank.ManaCost)}).");

            Vector2 origin = MagicSkillRuntimeUtility.Origin(context);
            var hit = new HashSet<int>();
            EnemyHealth current = FindNext(origin, rank.Range, hit, context.Caster);
            int maxTargets = Mathf.Max(1, action.MaxTargets);
            int affected = 0;
            while (current != null && affected < maxTargets)
            {
                int id = current.GetEntityId().GetHashCode();
                hit.Add(id);
                int damage = ResolveDamage(rank.Damage, action.TargetDamageMultipliers, affected);
                int finalDamage = MagicSkillRuntimeUtility.Damage(current, damage, action.DamageType, origin, cast);
                if (action.PostureDamageMultiplier > 0f
                    && current.IsCreatureFamily("Construct")
                    && current.ResolveElementVulnerabilityMultiplier(DamageType.Lightning) > 1f)
                    current.GetComponent<EnemyPostureState>()?.ApplyPostureDamage(
                        finalDamage * action.PostureDamageMultiplier);
                affected++;
                current = affected < maxTargets
                    ? FindNext(current.transform.position, action.ChainJumpRange, hit, current.gameObject)
                    : null;
            }

            MagicSkillRuntimeUtility.Commit(cast);
            return SkillEffectResult.Succeeded($"{_displayName}: {affected} alvo(s).",
                cast.Preparation.ManaCost > 0, true, rank.CooldownSeconds);
        }

        public static int ResolveDamage(int baseDamage, float[] multipliers, int targetIndex)
        {
            float multiplier = multipliers != null && multipliers.Length > 0
                ? multipliers[Mathf.Clamp(targetIndex, 0, multipliers.Length - 1)]
                : 1f;
            return Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(0, baseDamage) * Mathf.Clamp01(multiplier)));
        }

        private static EnemyHealth FindNext(Vector2 origin, float range, HashSet<int> excluded,
            GameObject source = null)
        {
            float rangeSquared = Mathf.Max(0f, range) * Mathf.Max(0f, range);
            var candidates = MagicSkillRuntimeUtility.OrderedEnemies(origin, enemy =>
                (excluded == null || !excluded.Contains(enemy.GetEntityId().GetHashCode()))
                && ((Vector2)enemy.transform.position - origin).sqrMagnitude <= rangeSquared
                && MagicSkillRuntimeUtility.HasLineOfEffect(origin, enemy.transform.position,
                    source, enemy.gameObject));
            candidates.Sort((left, right) =>
            {
                Vector2 lp = left.transform.position;
                Vector2 rp = right.transform.position;
                int distance = (lp - origin).sqrMagnitude.CompareTo((rp - origin).sqrMagnitude);
                if (distance != 0) return distance;
                int x = lp.x.CompareTo(rp.x);
                if (x != 0) return x;
                int y = lp.y.CompareTo(rp.y);
                return y != 0 ? y : string.CompareOrdinal(left.EnemyInstanceId, right.EnemyInstanceId);
            });
            return candidates.Count > 0 ? candidates[0] : null;
        }
    }
}
