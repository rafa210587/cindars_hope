using System;
using System.Collections.Generic;
using CindarsHope.Core;
using CindarsHope.Core.Events;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public class CaveEnemyRedistributionService
    {
        public void RedistributeEnemies(
            CaveLevelEnemyPlan levelPlan,
            int caveLevel,
            string reason,
            System.Func<string, string> getValidAnchorForEnemy)
        {
            if (levelPlan == null)
            {
                return;
            }

            var activeEnemies = levelPlan.GetActiveEnemies();
            int successCount = 0;

            foreach (var entry in activeEnemies)
            {
                var validAnchor = getValidAnchorForEnemy?.Invoke(entry.EnemyId);
                if (!string.IsNullOrWhiteSpace(validAnchor))
                {
                    entry.CurrentAnchorId = validAnchor;
                    successCount++;
                }
            }

            if (levelPlan.RedistributionState == null)
            {
                levelPlan.RedistributionState = new EnemyRedistributionState();
            }

            levelPlan.RedistributionState.RedistributionCount++;
            levelPlan.RedistributionState.LastRedistributionReason = reason;

            GameEventBus.Publish(new CaveEnemiesRedistributedEvent(caveLevel, reason));

            Debug.Log(
                $"CaveEnemyRedistributionService: Redistributed {successCount}/{activeEnemies.Count} " +
                $"enemies on level {caveLevel}. Reason: {reason}");
        }

        public bool NeedsRedistribution(CaveLevelEnemyPlan levelPlan)
        {
            return levelPlan != null && levelPlan.HasActiveEnemies();
        }
    }
}
