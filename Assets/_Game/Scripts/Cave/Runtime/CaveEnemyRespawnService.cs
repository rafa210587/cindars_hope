using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public class CaveEnemyRespawnService
    {
        private const int DefaultRespawnDelayGameDays = 2;

        public void MarkEnemyDefeated(
            EnemySpawnPlanEntry entry,
            int currentGameDay)
        {
            if (entry == null)
            {
                return;
            }

            entry.IsDefeated = true;
            entry.DefeatedAtGameDay = currentGameDay;
            entry.RespawnAvailableAtGameDay = currentGameDay + DefaultRespawnDelayGameDays;

            if (!entry.IsBoss)
            {
                GameEventBus.Publish(new CaveEnemyRespawnScheduledEvent(
                    entry.PlannedEnemyInstanceId,
                    entry.EnemyId,
                    entry.RespawnAvailableAtGameDay));

                Debug.Log(
                    $"CaveEnemyRespawnService: Common enemy {entry.EnemyId} " +
                    $"(planned ID: {entry.PlannedEnemyInstanceId}) scheduled for respawn " +
                    $"at game day {entry.RespawnAvailableAtGameDay}");
            }
        }

        public bool CanEnemyRespawn(EnemySpawnPlanEntry entry, int currentGameDay)
        {
            if (entry == null || !entry.IsDefeated || entry.IsBoss)
            {
                return false;
            }

            return currentGameDay >= entry.RespawnAvailableAtGameDay;
        }

        public void RespawnEnemy(
            EnemySpawnPlanEntry entry,
            string newAnchorId,
            int caveLevel)
        {
            if (entry == null || !entry.IsDefeated)
            {
                return;
            }

            entry.IsDefeated = false;
            entry.CurrentAnchorId = newAnchorId;
            entry.DefeatedAtGameDay = -1;
            entry.RespawnAvailableAtGameDay = -1;

            GameEventBus.Publish(new CaveEnemyRespawnedEvent(
                entry.PlannedEnemyInstanceId,
                entry.EnemyId,
                caveLevel));

            Debug.Log(
                $"CaveEnemyRespawnService: Enemy {entry.EnemyId} " +
                $"(planned ID: {entry.PlannedEnemyInstanceId}) respawned " +
                $"at anchor {newAnchorId}");
        }

        public void EvaluateRespawns(
            CaveLevelEnemyPlan levelPlan,
            int currentGameDay,
            int caveLevel,
            System.Func<string, string> getValidAnchorForEnemy)
        {
            if (levelPlan == null)
            {
                return;
            }

            foreach (var entry in levelPlan.EnemyPlans)
            {
                if (CanEnemyRespawn(entry, currentGameDay))
                {
                    var validAnchor = getValidAnchorForEnemy?.Invoke(entry.EnemyId);
                    if (!string.IsNullOrWhiteSpace(validAnchor))
                    {
                        RespawnEnemy(entry, validAnchor, caveLevel);
                    }
                }
            }
        }
    }
}
