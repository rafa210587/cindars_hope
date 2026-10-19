namespace CindarsHope.Skills.Runtime.Effects
{
    // WAVE_INTEGRATION_11: Defines how a skill effect resolves its target.
    public enum SkillEffectTargetType
    {
        None = 0,
        Self,
        CurrentInteractable,
        NearestInteractable,
        WorldPointInFrontOfPlayer,
        CropPlot,
        ResourceNode,
        Enemy,
        Area,
        DebugFixed
    }
}
