namespace CindarsHope.Combat.Weapon
{
    /// <summary>
    /// Visual archetype used by RuntimeProjectileFactory when no authored prefab exists.
    /// Arrow renders as an elongated shaft; bolts render as pulsing orbs tinted by damage type.
    /// </summary>
    public enum ProjectileVisualStyle
    {
        Auto,
        Arrow,
        MagicBolt,
        SkillBolt
    }
}
