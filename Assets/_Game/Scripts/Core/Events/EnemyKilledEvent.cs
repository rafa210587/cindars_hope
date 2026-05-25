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

        // Constructor for simple case: (enemyId, deathPosition)
        public EnemyKilledEvent(string enemyId, Vector3 deathPosition)
        {
            EnemyId = enemyId;
            DropItemId = string.Empty;
            DropAmount = 0;
            DeathPosition = deathPosition;
            XpReward = 0;
        }

        // Constructor for full case: (enemyId, dropItemId, dropAmount, deathPosition, xpReward)
        public EnemyKilledEvent(string enemyId, string dropItemId, int dropAmount, Vector3 deathPosition, int xpReward = 0)
        {
            EnemyId = enemyId;
            DropItemId = dropItemId;
            DropAmount = dropAmount;
            DeathPosition = deathPosition;
            XpReward = xpReward;
        }
    }
}
