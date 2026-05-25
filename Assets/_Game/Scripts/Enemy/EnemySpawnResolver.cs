using CindarsHope.Combat;
using CindarsHope.Core.Bootstrap;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CindarsHope.Enemy
{
    public class EnemySpawnResolver
    {
        private EnemyDatabaseSO _database;

        public EnemySpawnResolver(EnemyDatabaseSO database)
        {
            _database = database;
        }

        public EnemyDataSO ResolveSpawn(
            int caveLevel,
            List<string> biomeTags = null,
            List<string> environmentTags = null,
            int maxCount = 3,
            bool allowElite = false)
        {
            if (_database == null)
            {
                Debug.LogError("EnemySpawnResolver: Database not initialized");
                return null;
            }

            biomeTags = biomeTags ?? new List<string>();
            environmentTags = environmentTags ?? new List<string>();

            var candidates = new List<EnemyDataSO>();

            foreach (var enemy in _database.All)
            {
                if (enemy == null)
                    continue;

                if (enemy.IsBoss || enemy.IsMiniBoss)
                    continue;

                if (enemy.IsElite && !allowElite)
                    continue;

                if (enemy.CaveBand > 0)
                {
                    int bandMin = (enemy.CaveBand - 1) * 10 + 1;
                    int bandMax = enemy.CaveBand * 10;
                    if (caveLevel < bandMin || caveLevel > bandMax)
                        continue;
                }

                bool biomesMatch = biomeTags.Count == 0 || enemy.BiomeTags.Any(t => biomeTags.Contains(t));
                bool envMatch = environmentTags.Count == 0 || enemy.EnvironmentTags.Any(t => environmentTags.Contains(t));

                if (biomesMatch && envMatch)
                {
                    candidates.Add(enemy);
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning($"EnemySpawnResolver: No valid candidates for level {caveLevel}");
                return null;
            }

            return candidates[Random.Range(0, candidates.Count)];
        }

        public List<EnemyDataSO> ResolveMultiple(
            int caveLevel,
            int count,
            List<string> biomeTags = null,
            List<string> environmentTags = null,
            bool allowElite = false)
        {
            var result = new List<EnemyDataSO>();
            for (int i = 0; i < count; i++)
            {
                var enemy = ResolveSpawn(caveLevel, biomeTags, environmentTags, 1, allowElite);
                if (enemy != null)
                    result.Add(enemy);
            }
            return result;
        }
    }
}
