namespace CindarsHope.Foundation
{
    public interface IProjectileHitPolicy
    {
        bool TryResolveHit(int targetRuntimeId, out float damageMultiplier);
    }

    public interface IRangedDamageModifierRuntime
    {
        float ResolveRangedDamageMultiplier(int casterRuntimeId);
    }
}
