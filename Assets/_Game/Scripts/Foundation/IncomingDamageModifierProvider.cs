namespace CindarsHope.Foundation
{
    /// <summary>
    /// Neutral runtime port for temporary player damage modifiers. The combat pipeline reads this
    /// hook without depending on the feature that owns the modifier (for example, a skill ward).
    /// </summary>
    public static class IncomingDamageModifierProvider
    {
        public static System.Func<int, DamageType, float, bool, int> Source;

        public static int Resolve(int rawDamage, DamageType damageType, float now,
            bool isDamageOverTime)
            => Source != null
                ? Source(rawDamage, damageType, now, isDamageOverTime)
                : rawDamage;
    }
}
