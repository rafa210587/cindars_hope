namespace CindarsHope.Core.Events
{
    /// <summary>
    /// fable_78 — publicado quando um monstro mata outro (conflito inter-monstro). Dispara o caminho de
    /// corpo com loot/XP reduzidos, NÃO a rota normal de loot do jogador (EnemyKilledEvent permanece
    /// exclusivo de kills causados pelo jogador — XP/quests/bestiário só contam para kills do player).
    ///
    /// Os campos de drop (DropItemId/DropAmount/LootTableId/LootSeed) viajam no evento — assim o
    /// EnemyDropSpawner EXISTENTE (único caminho de drop) concede o corpo reduzido (× InterMonsterKillLootMultiplier)
    /// sem um segundo sistema de loot e sem GameObject.Find. Aditivo; campos de instância da seção 16.3 preservados.
    /// </summary>
    public readonly struct EnemyKilledByEnemyEvent
    {
        public readonly string VictimInstanceId;
        public readonly string KillerInstanceId;
        public readonly int CaveLevel;

        // fable_78 (SLICE 4): payload do corpo reduzido (mesmos campos de EnemyKilledEvent que o
        // EnemyDropSpawner consome). DropAmount já vem REDUZIDO (× InterMonsterKillLootMultiplier).
        public readonly string VictimEnemyId;
        public readonly string DropItemId;
        public readonly int DropAmount;
        public readonly string LootTableId;
        public readonly int LootSeed;
        public readonly bool IsElite;
        public readonly bool IsMinibossOrBoss;
        public readonly float ReducedLootMultiplier;

        public EnemyKilledByEnemyEvent(
            string victimInstanceId,
            string killerInstanceId,
            int caveLevel,
            string victimEnemyId = "",
            string dropItemId = "",
            int dropAmount = 0,
            string lootTableId = "",
            int lootSeed = 0,
            bool isElite = false,
            bool isMinibossOrBoss = false,
            float reducedLootMultiplier = 1f)
        {
            VictimInstanceId = victimInstanceId;
            KillerInstanceId = killerInstanceId;
            CaveLevel = caveLevel;
            VictimEnemyId = victimEnemyId ?? string.Empty;
            DropItemId = dropItemId ?? string.Empty;
            DropAmount = dropAmount;
            LootTableId = lootTableId ?? string.Empty;
            LootSeed = lootSeed;
            IsElite = isElite;
            IsMinibossOrBoss = isMinibossOrBoss;
            ReducedLootMultiplier = reducedLootMultiplier;
        }
    }
}
