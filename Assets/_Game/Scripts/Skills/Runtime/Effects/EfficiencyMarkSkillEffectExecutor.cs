using CindarsHope.Core.Bootstrap;
using CindarsHope.Craft;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    public sealed class EfficiencyMarkSkillEffectExecutor : ISkillEffectExecutor,
        IPreparableSkillEffectExecutor
    {
        private readonly SkillActionSO _defaultAction;

        public string EffectId => "crafting.marca_eficiencia";
        public SkillEffectCategory Category => SkillEffectCategory.Utility;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.CurrentInteractable;

        public EfficiencyMarkSkillEffectExecutor(SkillActionSO defaultAction)
        {
            _defaultAction = defaultAction;
        }

        public SkillEffectResult Validate(SkillEffectContext context)
        {
            var action = context?.ActionData != null ? context.ActionData : _defaultAction;
            if (context?.Caster == null || action == null ||
                EfficiencyMarkRuntimeCoordinator.Instance == null)
                return SkillEffectResult.Failed("RuntimeUnavailable",
                    "Sistemas da Marca de Eficiência indisponíveis.");
            var station = context.Target != null
                ? context.Target.GetComponent<CraftingPoint>() : null;
            if (station == null || string.IsNullOrWhiteSpace(station.StationInstanceId))
                return SkillEffectResult.Failed("TargetIsNotCraftingStation",
                    "A Marca exige uma estação de ofício materializada.");
            if (action.EffectRadius <= 0f || action.ResolveSecondaryDuration(context.Rank) <= 0f ||
                action.ResolveEffectMagnitude(context.Rank) <= 0f)
                return SkillEffectResult.Failed("InvalidSkillData",
                    "Dados da Marca de Eficiência estão inválidos.");
            int cost = Mathf.CeilToInt(action.ResolveRank(context.Rank).StaminaCost);
            var stamina = GameBootstrap.Instance?.StaminaManager as StaminaManager;
            return stamina == null || stamina.CurrentStamina >= cost
                ? SkillEffectResult.Succeeded("Pronto.")
                : SkillEffectResult.Failed("InsufficientStamina", "Stamina insuficiente.");
        }

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            var validation = Validate(context);
            if (!validation.Success) return validation;
            var action = context.ActionData ?? _defaultAction;
            var station = context.Target.GetComponent<CraftingPoint>();
            var stamina = GameBootstrap.Instance?.StaminaManager as StaminaManager;
            int cost = Mathf.CeilToInt(action.ResolveRank(context.Rank).StaminaCost);
            if (stamina != null && !stamina.TrySpendStamina(cost))
                return SkillEffectResult.Failed("InsufficientStamina", "Stamina insuficiente.");

            bool activated = EfficiencyMarkRuntimeCoordinator.Instance.Activate(
                station.StationInstanceId, station, station.transform.position, action.EffectRadius,
                action.ResolveSecondaryDuration(context.Rank),
                action.ResolveEffectMagnitude(context.Rank));
            return activated
                ? SkillEffectResult.Succeeded("Marca de Eficiência ancorada na estação.",
                    costSpent: stamina != null && cost > 0, cooldownStarted: true)
                : RefundAndFail(stamina, cost);
        }

        private static SkillEffectResult RefundAndFail(StaminaManager stamina, int cost)
        {
            if (stamina != null && cost > 0) stamina.AddStamina(cost);
            return SkillEffectResult.Failed("InvalidCraftingStation",
                "A estação não pode receber a Marca de Eficiência.");
        }
    }
}
