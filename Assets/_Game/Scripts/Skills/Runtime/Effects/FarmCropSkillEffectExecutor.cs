using CindarsHope.Farm;
using CindarsHope.Player;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Executor that waters a FarmPlot target via skill active slot.
    // EffectId: "farm.crop.water_skill"
    // Target: CropPlot (FarmPlot component on Target GameObject)
    public sealed class FarmCropSkillEffectExecutor : ISkillEffectExecutor
    {
        private const int WaterSkillStaminaCost = 10;

        public string EffectId => "farm.crop.water_skill";
        public SkillEffectCategory Category => SkillEffectCategory.Farm;
        public SkillEffectTargetType TargetType => SkillEffectTargetType.CurrentInteractable;

        public SkillEffectResult Execute(SkillEffectContext context)
        {
            if (context == null)
                return SkillEffectResult.Failed("NullContext");

            if (context.Target == null)
                return SkillEffectResult.Failed("NoTarget", "Aproxime-se de um canteiro para usar esta skill.");

            var farmPlot = context.Target.GetComponent<FarmPlot>();
            if (farmPlot == null)
                return SkillEffectResult.Failed("TargetIsNotFarmPlot", "Alvo nao e um canteiro de plantio.");

            if (!farmPlot.CanBeWatered)
                return SkillEffectResult.Failed("PlotCannotBeWatered", "Canteiro nao pode ser regado no estado atual.");

            // Deduct stamina only after confirming the plot can be watered.
            // Guard: skip deduction when no caster (slice mode / headless test).
            var stamina = context.Caster != null
                ? context.Caster.GetComponent<StaminaManager>()
                : null;
            if (stamina != null && !stamina.TrySpendStamina(WaterSkillStaminaCost))
                return SkillEffectResult.Failed("InsufficientStamina", "Stamina insuficiente para regar o canteiro.");

            bool watered = farmPlot.TryWaterViaSkill();
            if (watered)
            {
                Debug.Log($"[FarmCropSkillEffectExecutor] Plot watered via skill. SkillActionId={context.SkillActionId} StaminaSpent={stamina != null}");
                return SkillEffectResult.Succeeded("Canteiro regado pela skill.", costSpent: stamina != null, cooldownStarted: true);
            }

            return SkillEffectResult.Failed("WaterFailed", "Falha ao regar o canteiro.");
        }
    }
}
