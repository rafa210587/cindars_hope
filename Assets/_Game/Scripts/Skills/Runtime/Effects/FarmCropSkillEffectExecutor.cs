using CindarsHope.Farm;
using UnityEngine;

namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Executor that waters a FarmPlot target via skill active slot.
    // EffectId: "farm.crop.water_skill"
    // Target: CropPlot (FarmPlot component on Target GameObject)
    // TODO_INTEGRATION_NOT_FINAL: Stamina cost from caster is not yet deducted here.
    // Final design should deduct stamina from the caster's StaminaManager via SkillEffectContext.
    // Blocks final acceptance: NO (skill effect is functional; stamina deduction is debt)
    public sealed class FarmCropSkillEffectExecutor : ISkillEffectExecutor
    {
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

            bool watered = farmPlot.TryWaterViaSkill();
            if (watered)
            {
                Debug.Log($"[FarmCropSkillEffectExecutor] Plot watered via skill. SkillActionId={context.SkillActionId}");
                return SkillEffectResult.Succeeded("Canteiro regado pela skill.", costSpent: false, cooldownStarted: true);
            }

            return SkillEffectResult.Failed("WaterFailed", "Falha ao regar o canteiro.");
        }
    }
}
