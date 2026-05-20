using UnityEngine;

namespace CindarsHope.Core.Events
{
    /// <summary>
    /// Publicado quando um inimigo morre e dropa itens.
    /// </summary>
    public readonly struct EnemyKilledEvent
    {
        public string EnemyId { get; }
        public string DropItemId { get; }
        public int DropAmount { get; }
        public Vector3 DeathPosition { get; }
        public int XpReward { get; }

        public EnemyKilledEvent(string enemyId, string dropItemId, int dropAmount, Vector3 deathPosition)
            : this(enemyId, dropItemId, dropAmount, deathPosition, 0)
        {
        }

        public EnemyKilledEvent(string enemyId, string dropItemId, int dropAmount, Vector3 deathPosition, int xpReward)
        {
            EnemyId = enemyId;
            DropItemId = dropItemId;
            DropAmount = dropAmount;
            DeathPosition = deathPosition;
            XpReward = xpReward;
        }
    }
}
