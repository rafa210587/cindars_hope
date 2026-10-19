using UnityEngine;

namespace CindarsHope.Core.Events
{
    public readonly struct EnemyKilledEvent
    {
        public readonly string EnemyId;
        public readonly string DropItemId;
        public readonly int DropAmount;
        public readonly Vector3 DeathPosition;
        public readonly int XpReward;

        // fable_06 (aditivo, defaults neutros): contexto de loot determinístico (ADR-0005).
        // EnemyInstanceId + LootSeed permitem ao EnemyDropSpawner rolar a LootTableSO de forma
        // estável por run/instância. LootTableId resolve a tabela (vazio => caminho legado).
        // IsElite/IsMinibossOrBoss controlam a chance de essência. Construtores antigos continuam
        // válidos (campos novos ficam vazios/zero => comportamento legado por dropItemId fixo).
        public readonly string EnemyInstanceId;
        public readonly string LootTableId;
        public readonly int LootSeed;
        public readonly bool IsElite;
        public readonly bool IsMinibossOrBoss;

        // Constructor for simple case: (enemyId, deathPosition)
        public EnemyKilledEvent(string enemyId, Vector3 deathPosition)
        {
            EnemyId = enemyId;
            DropItemId = string.Empty;
            DropAmount = 0;
            DeathPosition = deathPosition;
            XpReward = 0;
            EnemyInstanceId = string.Empty;
            LootTableId = string.Empty;
            LootSeed = 0;
            IsElite = false;
            IsMinibossOrBoss = false;
        }

        // Constructor for full case: (enemyId, dropItemId, dropAmount, deathPosition, xpReward)
        public EnemyKilledEvent(string enemyId, string dropItemId, int dropAmount, Vector3 deathPosition, int xpReward = 0)
        {
            EnemyId = enemyId;
            DropItemId = dropItemId;
            DropAmount = dropAmount;
            DeathPosition = deathPosition;
            XpReward = xpReward;
            EnemyInstanceId = string.Empty;
            LootTableId = string.Empty;
            LootSeed = 0;
            IsElite = false;
            IsMinibossOrBoss = false;
        }

        // fable_06: full constructor with loot context (deterministic table roll).
        public EnemyKilledEvent(
            string enemyId,
            string dropItemId,
            int dropAmount,
            Vector3 deathPosition,
            int xpReward,
            string enemyInstanceId,
            string lootTableId,
            int lootSeed,
            bool isElite,
            bool isMinibossOrBoss)
        {
            EnemyId = enemyId;
            DropItemId = dropItemId;
            DropAmount = dropAmount;
            DeathPosition = deathPosition;
            XpReward = xpReward;
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            LootTableId = lootTableId ?? string.Empty;
            LootSeed = lootSeed;
            IsElite = isElite;
            IsMinibossOrBoss = isMinibossOrBoss;
        }
    }
}
