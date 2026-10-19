using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using CindarsHope.Core;
using CindarsHope.Foundation;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class MarkedPreySkillEffectExecutor : ISkillEffectExecutor, IPreparableSkillEffectExecutor
    {
        private readonly SkillActionSO _action;
        private MarkedPreyState _currentMark;

        public string EffectId => SkillActionEffectCatalog.RangedMarkedPreyEffectId;
        public SkillEffectCategory Category => SkillEffectCategory.Combat;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.None;

        public MarkedPreySkillEffectExecutor(SkillActionSO action) => _action = action;

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            if (context?.Caster == null || !context.Caster.activeInHierarchy)
                return SkillEffectResult.Failed("NoCaster", "Jogador não encontrado.");
            var state = DomainManagerRegistry.Get<SurvivalSkillState>();
            var action = context.ActionData ?? _action;
            if (ResolveNearestTarget(context.Caster.transform.position,
                    action.ResolveRank(context.Rank).Range, state) == null)
                return SkillEffectResult.Failed("NoTarget", "Nenhum alvo disponível para marcar.");
            var stamina = GameBootstrap.Instance?.StaminaManager as StaminaManager;
            int cost = Mathf.RoundToInt((context.ActionData ?? _action).ResolveRank(context.Rank).StaminaCost);
            if (stamina == null) return SkillEffectResult.Failed("MissingStamina", "Stamina indisponível.");
            if (stamina.CurrentStamina < cost)
                return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({cost}).");
            return SkillEffectResult.Succeeded(string.Empty);
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var readiness = Validate(context);
            if (!readiness.Success) return readiness;
            var state = DomainManagerRegistry.Get<SurvivalSkillState>();
            var action = context.ActionData ?? _action;
            var target = ResolveNearestTarget(context.Caster.transform.position,
                action.ResolveRank(context.Rank).Range, state);
            if (target == null) return SkillEffectResult.Failed("NoTarget", "O alvo não está mais disponível.");
            var rankData = action.ResolveRank(context.Rank);
            var stamina = (StaminaManager)GameBootstrap.Instance.StaminaManager;
            int cost = Mathf.RoundToInt(rankData.StaminaCost);
            if (!stamina.TrySpendStamina(cost))
                return SkillEffectResult.Failed("InsufficientStamina", $"Stamina insuficiente ({cost}).");

            if (_currentMark != null && _currentMark.gameObject != target.gameObject)
                _currentMark.Clear();
            _currentMark = target.GetComponent<MarkedPreyState>() ?? target.gameObject.AddComponent<MarkedPreyState>();
            _currentMark.Apply(context.Caster.GetEntityId().GetHashCode(), action.ResolveEffectDuration(context.Rank),
                action.ResolveRangedDamageBonus(context.Rank));
            string actionToken = state != null && state.HasActiveEncounter
                ? state.ActiveEncounterId + "|marked:" + target.EnemyInstanceId
                : action.SkillActionId + "|marked:" + target.EnemyInstanceId;
            RangedLunarModifierProvider.TryActivateFirstTarget(
                target.EnemyInstanceId, actionToken);
            return SkillEffectResult.Succeeded("Presa Marcada!", costSpent: cost > 0,
                cooldownStarted: true, cooldownSeconds: rankData.CooldownSeconds);
        }

        private static EnemyHealth ResolveNearestTarget(
            Vector2 origin, float range, SurvivalSkillState state)
        {
            EnemyHealth best = null;
            float maximumDistance = Mathf.Max(0f, range) * Mathf.Max(0f, range);
            float bestDistance = float.MaxValue;
            foreach (var candidate in EnemyHealth.ActiveInstances)
            {
                if (candidate == null || candidate.IsDead || !candidate.gameObject.activeInHierarchy ||
                    (state != null && state.HasActiveEncounter &&
                     !state.IsActiveEncounterEnemy(candidate.EnemyInstanceId))) continue;
                float distance = ((Vector2)candidate.transform.position - origin).sqrMagnitude;
                if (distance > maximumDistance && !Mathf.Approximately(distance, maximumDistance)) continue;
                if (best == null || distance < bestDistance || (Mathf.Approximately(distance, bestDistance)
                    && string.CompareOrdinal(candidate.EnemyInstanceId, best.EnemyInstanceId) < 0))
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }
            return best;
        }
    }
}
