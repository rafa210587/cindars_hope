namespace CindarsHope.Quests.CaveContracts
{
    // Maps CaveContractType to canonical quest objective/trigger event names
    public static class CaveContractObjectiveAdapter
    {
        public static string GetObjectiveType(CaveContractType contractType) => contractType switch
        {
            CaveContractType.DefeatEnemy        => "DefeatEnemy",
            CaveContractType.DefeatEnemyFamily  => "DefeatEnemyFamily",
            CaveContractType.DefeatElite        => "DefeatEnemyFamily",
            CaveContractType.DefeatBoss         => "DefeatBoss",
            CaveContractType.CollectCaveResource=> "CollectItem",
            CaveContractType.ReachCaveDepth     => "ReachCaveDepth",
            CaveContractType.CompleteCaveRun    => "CompleteCaveRun",
            CaveContractType.DiscoverWeakness   => "DiscoverWeakness",
            CaveContractType.MapArea            => "ExploreArea",
            CaveContractType.RecoverCorpse      => "RecoverCorpse",
            CaveContractType.InteractWithCaveObject => "InteractWithObject",
            _ => "DefeatEnemy"
        };

        public static string GetTriggerEventName(CaveContractType contractType) => contractType switch
        {
            CaveContractType.DefeatEnemy        => "OnEnemyDefeated",
            CaveContractType.DefeatEnemyFamily  => "OnEnemyFamilyDefeated",
            CaveContractType.DefeatElite        => "OnEnemyFamilyDefeated",
            CaveContractType.DefeatBoss         => "OnBossDefeated",
            CaveContractType.CollectCaveResource=> "OnItemCollected",
            CaveContractType.ReachCaveDepth     => "OnCaveDepthReached",
            CaveContractType.CompleteCaveRun    => "OnCaveRunCompleted",
            CaveContractType.DiscoverWeakness   => "OnWeaknessDiscovered",
            CaveContractType.MapArea            => "OnAreaMapped",
            CaveContractType.RecoverCorpse      => "OnCorpseRecovered",
            CaveContractType.InteractWithCaveObject => "OnObjectInteracted",
            _ => "OnEnemyDefeated"
        };

        // Boss/Elite contracts must NEVER mutate CaveRunSeed — they read-only check progress
        public static bool MutatesCaveRunSeed(CaveContractType contractType) => false;
    }
}
