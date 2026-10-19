using System;
using System.Collections.Generic;
using CindarsHope.Cave.Generation;
using UnityEngine;

namespace CindarsHope.Cave.Runtime
{
    public sealed class CaveEnemySpawnPlanService
    {
        public CaveLevelEnemyPlan CreatePlanForLevel(CaveGeneratedLevel generatedLevel, List<string> spawnedEnemyIds)
        {
            if (generatedLevel == null)
            {
                Debug.LogError("CaveEnemySpawnPlanService: Cannot create plan for null CaveGeneratedLevel.");
                return new CaveLevelEnemyPlan();
            }

            var plan = new CaveLevelEnemyPlan();

            if (spawnedEnemyIds != null && spawnedEnemyIds.Count > 0)
            {
                for (int i = 0; i < spawnedEnemyIds.Count && i < generatedLevel.EnemySpawnPoints.Count; i++)
                {
                    var spawnPoint = generatedLevel.EnemySpawnPoints[i];
                    var enemyId = spawnedEnemyIds[i];
                    var anchorId = $"anchor_{spawnPoint.Position.x}_{spawnPoint.Position.y}";

                    var entry = new EnemySpawnPlanEntry(
                        $"enemy_{i}_{spawnPoint.Position}",
                        enemyId,
                        anchorId)
                    {
                        IsBoss = false
                    };

                    plan.EnemyPlans.Add(entry);
                }
            }

            return plan;
        }

        public void PopulatePlanFromSpawns(CaveLevelEnemyPlan plan, List<string> spawnedEnemyIds)
        {
            if (plan == null || spawnedEnemyIds == null)
            {
                return;
            }

            plan.EnemyPlans.Clear();

            for (int i = 0; i < spawnedEnemyIds.Count; i++)
            {
                var enemyId = spawnedEnemyIds[i];
                var anchorId = $"anchor_{i}";

                var entry = new EnemySpawnPlanEntry(
                    $"enemy_{i}",
                    enemyId,
                    anchorId)
                {
                    IsBoss = false
                };

                plan.EnemyPlans.Add(entry);
            }
        }

        public CaveLevelEnemyPlan CreatePlanFromCaveEnemySpawnPlan(CaveEnemySpawnPlan spawnPlan)
        {
            var plan = new CaveLevelEnemyPlan();
            if (spawnPlan?.Entries == null)
            {
                return plan;
            }

            foreach (var spawnEntry in spawnPlan.Entries)
            {
                if (spawnEntry == null || string.IsNullOrWhiteSpace(spawnEntry.EnemyId))
                {
                    continue;
                }

                var anchorId = !string.IsNullOrWhiteSpace(spawnEntry.RoomId)
                    ? $"{spawnEntry.RoomId}_{spawnEntry.GridPosition.x}_{spawnEntry.GridPosition.y}"
                    : $"anchor_{spawnEntry.SpawnIndex}";

                plan.EnemyPlans.Add(new EnemySpawnPlanEntry(
                    spawnEntry.EnemyInstanceId,
                    spawnEntry.EnemyId,
                    anchorId)
                {
                    IsBoss = spawnEntry.SizeClass == "Boss"
                });
            }

            return plan;
        }

        public bool IsPlanValid(CaveLevelEnemyPlan plan)
        {
            return plan != null && plan.EnemyPlans.Count > 0;
        }
    }
}
