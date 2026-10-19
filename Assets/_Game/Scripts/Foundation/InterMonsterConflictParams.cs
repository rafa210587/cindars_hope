namespace CindarsHope.Foundation
{
    /// <summary>
    /// Parametros escalares do conflito inter-monstro (fable_78), passados como valores primitivos
    /// em vez do CaveEcosystemBalanceSO inteiro — corte do par mutuo Cave|Enemy. Populado pelo lado
    /// Cave (materializer) a partir do balance SO; consumido pelo EnemyConflictHandler.
    /// </summary>
    public readonly struct InterMonsterConflictParams
    {
        public readonly float PlayerAggroWeight;
        public readonly float RivalAggroWeight;
        public readonly float WoundedDefenseMultiplier;
        public readonly float WoundedDurationSeconds;
        public readonly float InterMonsterDamageMultiplier;
        public readonly float InterMonsterKillLootMultiplier;

        public InterMonsterConflictParams(
            float playerAggroWeight,
            float rivalAggroWeight,
            float woundedDefenseMultiplier,
            float woundedDurationSeconds,
            float interMonsterDamageMultiplier,
            float interMonsterKillLootMultiplier)
        {
            PlayerAggroWeight = playerAggroWeight;
            RivalAggroWeight = rivalAggroWeight;
            WoundedDefenseMultiplier = woundedDefenseMultiplier;
            WoundedDurationSeconds = woundedDurationSeconds;
            InterMonsterDamageMultiplier = interMonsterDamageMultiplier;
            InterMonsterKillLootMultiplier = interMonsterKillLootMultiplier;
        }
    }
}
