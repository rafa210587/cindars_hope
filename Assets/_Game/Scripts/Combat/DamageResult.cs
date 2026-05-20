namespace CindarsHope.Combat
{
    public readonly struct DamageResult
    {
        public int FinalDamage { get; }
        public int BaseDamage { get; }
        public int AttributeBonus { get; }
        public float TypeMultiplier { get; }
        public bool WasReducedToMinimum { get; }
        public bool WasImmune { get; }

        public DamageResult(
            int finalDamage,
            int baseDamage,
            int attributeBonus,
            float typeMultiplier,
            bool wasReducedToMinimum,
            bool wasImmune)
        {
            FinalDamage = finalDamage;
            BaseDamage = baseDamage;
            AttributeBonus = attributeBonus;
            TypeMultiplier = typeMultiplier;
            WasReducedToMinimum = wasReducedToMinimum;
            WasImmune = wasImmune;
        }
    }
}
