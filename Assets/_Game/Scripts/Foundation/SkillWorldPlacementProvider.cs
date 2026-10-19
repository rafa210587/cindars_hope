namespace CindarsHope.Foundation
{
    public interface ISkillWorldPlacementRuntime
    {
        bool CanPlace(float originX, float originY, float targetX, float targetY,
            float maximumRange);
    }

    /// <summary>Scene-owned query for walkable, unobstructed skill placement.</summary>
    public static class SkillWorldPlacementProvider
    {
        public static ISkillWorldPlacementRuntime Source;

        public static bool CanPlace(float originX, float originY, float targetX, float targetY,
            float maximumRange)
            => Source != null && Source.CanPlace(originX, originY, targetX, targetY, maximumRange);
    }
}
